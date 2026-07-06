using DataAccess.Services;
using Domain.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Radio.Web.Security
{
    /// <summary>
    /// معالج أحداث مصادقة الكوكيز لتحديث حالة حساب المستخدم ودوره وصلاحياته ديناميكياً ولحظياً.
    /// </summary>
    public class CustomCookieAuthenticationEvents : CookieAuthenticationEvents
    {
        private readonly IPermissionEvaluationService _evaluationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CustomCookieAuthenticationEvents> _logger;

        public CustomCookieAuthenticationEvents(
            IPermissionEvaluationService evaluationService,
            UserManager<ApplicationUser> userManager,
            ILogger<CustomCookieAuthenticationEvents> logger)
        {
            _evaluationService = evaluationService;
            _userManager = userManager;
            _logger = logger;
        }

        public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            var principal = context.Principal;
            if (principal?.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = principal.FindFirst("DomainUserId");
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
                {
                    var user = await _userManager.FindByIdAsync(userId.ToString());
                    if (user == null || !user.IsActive)
                    {
                        context.RejectPrincipal();
                        _logger.LogWarning("تم رفض جلسة المستخدم {UserId} - الحساب غير نشط أو تم تعطيله حديثاً", userId);
                        return;
                    }

                    var currentRoleId = user.RoleId;

                    // 1. تسخين وتحديث ذاكرة الصلاحيات المؤقتة بشكل غير متزامن في بداية الطلب
                    // هذا يضمن توفر الصلاحيات في الكاش طوال فترة دورة حياة الطلب الحالي للاستدعاء المتزامن في الـ Views
                    await _evaluationService.GetEffectivePermissionsAsync(userId, currentRoleId);
                    await _evaluationService.GetUserOverridesAsync(userId);

                    if (principal.Identity is ClaimsIdentity identity)
                    {
                        // 2. التحقق من تطابق دور المستخدم الحالي وتحديث الكوكي عند الاختلاف فقط
                        var roleIdClaim = identity.FindFirst("DomainRoleId");
                        if (roleIdClaim != null)
                        {
                            if (roleIdClaim.Value != currentRoleId.ToString())
                            {
                                identity.RemoveClaim(roleIdClaim);
                                identity.AddClaim(new Claim("DomainRoleId", currentRoleId.ToString()));
                                context.ShouldRenew = true; // تجديد الكوكي مع حفظ التعديلات في المتصفح
                                _logger.LogInformation("تم تحديث دور المستخدم {UserId} من {OldRole} إلى {NewRole} ديناميكياً وتجديد الجلسة", userId, roleIdClaim.Value, currentRoleId);
                            }
                        }
                        else
                        {
                            identity.AddClaim(new Claim("DomainRoleId", currentRoleId.ToString()));
                            context.ShouldRenew = true;
                        }

                        // لم نعد نقوم بحشو صلاحيات المستخدم كـ Claims داخل الكوكي لتجنب تضخم حجم الكوكي (Cookie Bloat).
                        // بدلاً من ذلك، نعتمد بالكامل على التحقق اللحظي عبر الكاش المحدث من خلال IPermissionEvaluationService.
                        var existingPermissionClaims = identity.FindAll("Permission").ToList();
                        foreach (var claim in existingPermissionClaims)
                            identity.RemoveClaim(claim);
                    }
                }
            }

            await base.ValidatePrincipal(context);
        }
    }
}
