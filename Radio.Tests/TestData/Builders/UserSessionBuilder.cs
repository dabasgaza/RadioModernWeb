using DataAccess.Common;
using Radio.Web.Services;

namespace Radio.Tests.TestData.Builders;

public static class UserSessionBuilder
{
    public static UserSession CreateAdmin()
        => new()
        {
            UserId = 1, Username = "admin", FullName = "Admin User",
            RoleName = "Admin", Permissions = []
        };

    public static UserSession CreateLimited(params string[] permissions)
        => new()
        {
            UserId = 2, Username = "operator", FullName = "Operator User",
            RoleName = "Operator", Permissions = [.. permissions]
        };

    public static UserSession CreateWithAllPermissions()
        => CreateLimited(
            AppPermissions.EpisodeManage, AppPermissions.EpisodeEdit,
            AppPermissions.EpisodeExecute, AppPermissions.EpisodePublish,
            AppPermissions.EpisodeWebPublish, AppPermissions.EpisodeDelete,
            AppPermissions.EpisodeRevert, AppPermissions.ProgramManage,
            AppPermissions.GuestManage, AppPermissions.StaffManage,
            AppPermissions.CoordinationManage, AppPermissions.ViewReports,
            AppPermissions.DatabaseManage, AppPermissions.ViewAuditLogs
        );

    public static Mock<ICurrentUserService> CreateMock()
        => CreateMock(CreateAdmin());

    public static Mock<ICurrentUserService> CreateMock(params string[] permissions)
        => CreateMock(CreateLimited(permissions));

    public static Mock<ICurrentUserService> CreateMock(UserSession session)
    {
        var mock = new Mock<ICurrentUserService>();
        mock.Setup(m => m.ToUserSession()).Returns(session);
        return mock;
    }
}
