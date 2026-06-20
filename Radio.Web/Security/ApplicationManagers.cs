using Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Radio.Web.Security;

/// <summary>
/// مدير المستخدمين المخصص — يضيف منطقاً للتعامل مع ApplicationUser.
/// </summary>
public class ApplicationUserManager : UserManager<ApplicationUser>
{
    public ApplicationUserManager(
        IUserStore<ApplicationUser> store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<ApplicationUser> passwordHasher,
        IEnumerable<IUserValidator<ApplicationUser>> userValidators,
        IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<UserManager<ApplicationUser>> logger)
        : base(store, optionsAccessor, passwordHasher, userValidators,
               passwordValidators, keyNormalizer, errors, services, logger)
    {
    }
}

/// <summary>
/// مدير الأدوار المخصص.
/// </summary>
public class ApplicationRoleManager : RoleManager<ApplicationRole>
{
    public ApplicationRoleManager(
        IRoleStore<ApplicationRole> store,
        IEnumerable<IRoleValidator<ApplicationRole>> roleValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        ILogger<RoleManager<ApplicationRole>> logger)
        : base(store, roleValidators, keyNormalizer, errors, logger)
    {
    }
}

/// <summary>
/// مدير تسجيل الدخول المخصص — يحدّث LastLoginAt ويتحقق من IsActive.
/// </summary>
public class ApplicationSignInManager : SignInManager<ApplicationUser>
{
    private readonly ApplicationUserManager _userManager;

    public ApplicationSignInManager(
        ApplicationUserManager userManager,
        Microsoft.AspNetCore.Http.IHttpContextAccessor contextAccessor,
        IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
        IOptions<IdentityOptions> optionsAccessor,
        ILogger<SignInManager<ApplicationUser>> logger,
        Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider schemes,
        Microsoft.AspNetCore.Identity.IUserConfirmation<ApplicationUser> confirmation)
        : base(userManager, contextAccessor, claimsFactory, optionsAccessor,
               logger, schemes, confirmation)
    {
        _userManager = userManager;
    }

    public override async Task<SignInResult> PasswordSignInAsync(
        string userName, string password, bool isPersistent, bool lockoutOnFailure)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
            return SignInResult.Failed;

        if (!user.IsActive)
            return SignInResult.NotAllowed;

        var result = await base.PasswordSignInAsync(userName, password, isPersistent, lockoutOnFailure);

        if (result.Succeeded)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
        }

        return result;
    }
}
