using DataAccess.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Radio.Web.Security
{
    /// <summary>
    /// متطلب الصلاحية للسياسات الأمنية.
    /// </summary>
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string PermissionName { get; }

        public PermissionRequirement(string permissionName)
        {
            PermissionName = permissionName;
        }
    }

    /// <summary>
    /// المعالج المركزي لتقييم سياسات الصلاحيات المخصصة.
    /// </summary>
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IPermissionEvaluationService _evaluationService;

        public PermissionHandler(IPermissionEvaluationService evaluationService)
        {
            _evaluationService = evaluationService;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (_evaluationService.HasPermission(context.User, requirement.PermissionName))
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// موفر السياسات الديناميكية لـ ASP.NET Core لمنع الحاجة لتسجيل كل سياسة يدوياً.
    /// </summary>
    public class DynamicPermissionPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        public DynamicPermissionPolicyProvider(IOptions<AuthorizationOptions> options) : base(options)
        {
        }

        public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            // أولاً: البحث في السياسات المسجلة بشكل صريح (مثل سياسات الهوية الافتراضية)
            var policy = await base.GetPolicyAsync(policyName);
            if (policy != null) return policy;

            // ثانياً: توليد سياسة جديدة ديناميكياً لأي اسم صلاحية
            var policyBuilder = new AuthorizationPolicyBuilder();
            policyBuilder.AddRequirements(new PermissionRequirement(policyName));
            return policyBuilder.Build();
        }
    }
}
