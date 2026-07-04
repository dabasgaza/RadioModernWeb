// ============================================================
// ValidatorTests — المدققات
// ============================================================
// المسؤولية: تعريف المدققات.
// ============================================================
using DataAccess.DTOs;
using DataAccess.Validation.Validators;
using Domain.Models;

namespace Radio.Tests.Validation;

/// <summary>
/// صنف المدققات.
/// </summary>
public class ValidatorTests
{
    /// <summary>
    /// الضيف Dto Validator_ Valid_ Passes.
    /// </summary>
    [Fact]
    public async Task GuestDtoValidator_Valid_Passes()
    {
        var sut = new GuestDtoValidator();
        var dto = new GuestDto(0, "محمد", null, "010000", null, null, null);
        var result = await sut.ValidateAsync(dto);
        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// الضيف Dto Validator_ Missing Full Name_ Fails.
    /// </summary>
    [Fact]
    public async Task GuestDtoValidator_MissingFullName_Fails()
    {
        var sut = new GuestDtoValidator();
        var dto = new GuestDto(0, "", null, "010000", null, null, null);
        var result = await sut.ValidateAsync(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FullName");
    }

    /// <summary>
    /// الضيف Dto Validator_ Missing Both Phone And Email_ Fails.
    /// </summary>
    [Fact]
    public async Task GuestDtoValidator_MissingBothPhoneAndEmail_Fails()
    {
        var sut = new GuestDtoValidator();
        var dto = new GuestDto(0, "محمد", null, null, null, null, null);
        var result = await sut.ValidateAsync(dto);
        result.IsValid.Should().BeFalse();
    }

    /// <summary>
    /// البرنامج Dto Validator_ Valid_ Passes.
    /// </summary>
    [Fact]
    public async Task ProgramDtoValidator_Valid_Passes()
    {
        var sut = new ProgramDtoValidator();
        var dto = new ProgramDto(0, "برنامج", null, null);
        var result = await sut.ValidateAsync(dto);
        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// البرنامج Dto Validator_ Missing Name_ Fails.
    /// </summary>
    [Fact]
    public async Task ProgramDtoValidator_MissingName_Fails()
    {
        var sut = new ProgramDtoValidator();
        var dto = new ProgramDto(0, "", null, null);
        var result = await sut.ValidateAsync(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ProgramName");
    }

    /// <summary>
    /// الحلقة Dto Validator_ Valid_ Passes.
    /// </summary>
    [Fact]
    public async Task EpisodeDtoValidator_Valid_Passes()
    {
        var sut = new EpisodeDtoValidator();
        var dto = new EpisodeDto(0, 1, [], [], [], "حلقة", null, DateTime.UtcNow, null, null);
        var result = await sut.ValidateAsync(dto);
        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// الحلقة Dto Validator_ Missing البرنامج Id_ Fails.
    /// </summary>
    [Fact]
    public async Task EpisodeDtoValidator_MissingProgramId_Fails()
    {
        var sut = new EpisodeDtoValidator();
        var dto = new EpisodeDto(0, 0, [], [], [], "حلقة", null, DateTime.UtcNow, null, null);
        var result = await sut.ValidateAsync(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ProgramId");
    }

    /// <summary>
    /// الحلقة Dto Validator_ Missing Name_ Fails.
    /// </summary>
    [Fact]
    public async Task EpisodeDtoValidator_MissingName_Fails()
    {
        var sut = new EpisodeDtoValidator();
        var dto = new EpisodeDto(0, 1, [], [], [], "", null, DateTime.UtcNow, null, null);
        var result = await sut.ValidateAsync(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EpisodeName");
    }

    /// <summary>
    /// الحلقة Dto Validator_ Missing Scheduled Date_ Fails.
    /// </summary>
    [Fact]
    public async Task EpisodeDtoValidator_MissingScheduledDate_Fails()
    {
        var sut = new EpisodeDtoValidator();
        var dto = new EpisodeDto(0, 1, [], [], [], "حلقة", null, null, null, null);
        var result = await sut.ValidateAsync(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ScheduledDate");
    }

    /// <summary>
    /// التغطية Dto Validator_ Valid_ Passes.
    /// </summary>
    [Fact]
    public async Task CoverageDtoValidator_Valid_Passes()
    {
        var sut = new CoverageDtoValidator();
        var dto = new CoverageDto { CorrespondentId = 1, Topic = "موضوع" };
        var result = await sut.ValidateAsync(dto);
        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// التغطية Dto Validator_ Missing المراسل Id_ Fails.
    /// </summary>
    [Fact]
    public async Task CoverageDtoValidator_MissingCorrespondentId_Fails()
    {
        var sut = new CoverageDtoValidator();
        var dto = new CoverageDto { CorrespondentId = 0, Topic = "موضوع" };
        var result = await sut.ValidateAsync(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CorrespondentId");
    }

    /// <summary>
    /// التغطية Dto Validator_ Missing Topic_ Fails.
    /// </summary>
    [Fact]
    public async Task CoverageDtoValidator_MissingTopic_Fails()
    {
        var sut = new CoverageDtoValidator();
        var dto = new CoverageDto { CorrespondentId = 1, Topic = "" };
        var result = await sut.ValidateAsync(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Topic");
    }

    /// <summary>
    /// الموظف Dto Validator_ Valid_ Passes.
    /// </summary>
    [Fact]
    public async Task EmployeeDtoValidator_Valid_Passes()
    {
        var sut = new EmployeeDtoValidator();
        var dto = new EmployeeDto(0, "موظف", 1, null, null);
        var result = await sut.ValidateAsync(dto);
        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// الموظف Dto Validator_ Missing Name_ Fails.
    /// </summary>
    [Fact]
    public async Task EmployeeDtoValidator_MissingName_Fails()
    {
        var sut = new EmployeeDtoValidator();
        var dto = new EmployeeDto(0, "", 1, null, null);
        var result = await sut.ValidateAsync(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FullName");
    }

    /// <summary>
    /// الموظف Dto Validator_ Missing الموظفين الدور Id_ Fails.
    /// </summary>
    [Fact]
    public async Task EmployeeDtoValidator_MissingStaffRoleId_Fails()
    {
        var sut = new EmployeeDtoValidator();
        var dto = new EmployeeDto(0, "موظف", null, null, null);
        var result = await sut.ValidateAsync(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "StaffRoleId");
    }

    /// <summary>
    /// الموظفين الدور Dto Validator_ Valid_ Passes.
    /// </summary>
    [Fact]
    public async Task StaffRoleDtoValidator_Valid_Passes()
    {
        var sut = new StaffRoleDtoValidator();
        var dto = new StaffRoleDto(0, "دور");
        var result = await sut.ValidateAsync(dto);
        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// الموظفين الدور Dto Validator_ Missing Name_ Fails.
    /// </summary>
    [Fact]
    public async Task StaffRoleDtoValidator_MissingName_Fails()
    {
        var sut = new StaffRoleDtoValidator();
        var dto = new StaffRoleDto(0, "");
        var result = await sut.ValidateAsync(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "RoleName");
    }

    /// <summary>
    /// المراسل Dto Validator_ Valid_ Passes.
    /// </summary>
    [Fact]
    public async Task CorrespondentDtoValidator_Valid_Passes()
    {
        var sut = new CorrespondentDtoValidator();
        var dto = new CorrespondentDto(0, "مراسل", "010000", null);
        var result = await sut.ValidateAsync(dto);
        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// المراسل Dto Validator_ Missing Name_ Fails.
    /// </summary>
    [Fact]
    public async Task CorrespondentDtoValidator_MissingName_Fails()
    {
        var sut = new CorrespondentDtoValidator();
        var dto = new CorrespondentDto(0, "", "010000", null);
        var result = await sut.ValidateAsync(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FullName");
    }

    /// <summary>
    /// المراسل Dto Validator_ Missing Phone_ Fails.
    /// </summary>
    [Fact]
    public async Task CorrespondentDtoValidator_MissingPhone_Fails()
    {
        var sut = new CorrespondentDtoValidator();
        var dto = new CorrespondentDto(0, "مراسل", "", null);
        var result = await sut.ValidateAsync(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PhoneNumber");
    }

    /// <summary>
    /// Social Media Platform Dto Validator_ Valid_ Passes.
    /// </summary>
    [Fact]
    public async Task SocialMediaPlatformDtoValidator_Valid_Passes()
    {
        var sut = new SocialMediaPlatformDtoValidator();
        var dto = new SocialMediaPlatformDto(1, "فيسبوك", "https://facebook.com");
        var result = await sut.ValidateAsync(dto);
        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Social Media Platform Dto Validator_ Missing Name_ Fails.
    /// </summary>
    [Fact]
    public async Task SocialMediaPlatformDtoValidator_MissingName_Fails()
    {
        var sut = new SocialMediaPlatformDtoValidator();
        var dto = new SocialMediaPlatformDto(0, "", null);
        var result = await sut.ValidateAsync(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    /// <summary>
    /// المستخدم Dto Validator_ Valid_ Passes.
    /// </summary>
    [Fact]
    public async Task UserDtoValidator_Valid_Passes()
    {
        var sut = new UserDtoValidator();
        var dto = new UserDto { FullName = "مستخدم", Username = "user", RoleId = 1 };
        var result = await sut.ValidateAsync(dto);
        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// المستخدم Dto Validator_ Missing Full Name_ Fails.
    /// </summary>
    [Fact]
    public async Task UserDtoValidator_MissingFullName_Fails()
    {
        var sut = new UserDtoValidator();
        var dto = new UserDto { FullName = "", Username = "user", RoleId = 1 };
        var result = await sut.ValidateAsync(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FullName");
    }

    /// <summary>
    /// المستخدم Dto Validator_ Missing Username_ Fails.
    /// </summary>
    [Fact]
    public async Task UserDtoValidator_MissingUsername_Fails()
    {
        var sut = new UserDtoValidator();
        var dto = new UserDto { FullName = "مستخدم", Username = "", RoleId = 1 };
        var result = await sut.ValidateAsync(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Username");
    }

    /// <summary>
    /// المستخدم Dto Validator_ Missing الدور Id_ Fails.
    /// </summary>
    [Fact]
    public async Task UserDtoValidator_MissingRoleId_Fails()
    {
        var sut = new UserDtoValidator();
        var dto = new UserDto { FullName = "مستخدم", Username = "user", RoleId = 0 };
        var result = await sut.ValidateAsync(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "RoleId");
    }

    /// <summary>
    /// المستخدم Create Validator_ New User_ With Password_ Passes.
    /// </summary>
    [Fact]
    public async Task UserCreateValidator_NewUser_WithPassword_Passes()
    {
        var sut = new UserCreateValidator();
        var dto = new UserDto { UserId = 0, FullName = "مستخدم", Username = "user", RoleId = 1 };
        var result = await sut.ValidateAsync((dto, "password123"));
        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// المستخدم Create Validator_ New User_ Without Password_ Fails.
    /// </summary>
    [Fact]
    public async Task UserCreateValidator_NewUser_WithoutPassword_Fails()
    {
        var sut = new UserCreateValidator();
        var dto = new UserDto { UserId = 0, FullName = "مستخدم", Username = "user", RoleId = 1 };
        var result = await sut.ValidateAsync((dto, ""));
        result.IsValid.Should().BeFalse();
    }

    /// <summary>
    /// المستخدم Create Validator_ Existing User_ Without Password_ Passes.
    /// </summary>
    [Fact]
    public async Task UserCreateValidator_ExistingUser_WithoutPassword_Passes()
    {
        var sut = new UserCreateValidator();
        var dto = new UserDto { UserId = 5, FullName = "مستخدم", Username = "user", RoleId = 1 };
        var result = await sut.ValidateAsync((dto, ""));
        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// المستخدم Create Validator_ Short Password_ Fails.
    /// </summary>
    [Fact]
    public async Task UserCreateValidator_ShortPassword_Fails()
    {
        var sut = new UserCreateValidator();
        var dto = new UserDto { UserId = 0, FullName = "مستخدم", Username = "user", RoleId = 1 };
        var result = await sut.ValidateAsync((dto, "123"));
        result.IsValid.Should().BeFalse();
    }

    /// <summary>
    /// Platform Publish Dto Validator_ Valid_ Passes.
    /// </summary>
    [Fact]
    public async Task PlatformPublishDtoValidator_Valid_Passes()
    {
        var sut = new PlatformPublishDtoValidator();
        var dto = new PlatformPublishDto(1, "فيسبوك", "https://facebook.com/post");
        var result = await sut.ValidateAsync(dto);
        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Platform Publish Dto Validator_ Missing Platform Id_ Fails.
    /// </summary>
    [Fact]
    public async Task PlatformPublishDtoValidator_MissingPlatformId_Fails()
    {
        var sut = new PlatformPublishDtoValidator();
        var dto = new PlatformPublishDto(0, "", "https://example.com");
        var result = await sut.ValidateAsync(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PlatformId");
    }

    /// <summary>
    /// Platform Publish Dto Validator_ Missing Url_ Fails.
    /// </summary>
    [Fact]
    public async Task PlatformPublishDtoValidator_MissingUrl_Fails()
    {
        var sut = new PlatformPublishDtoValidator();
        var dto = new PlatformPublishDto(1, "", "");
        var result = await sut.ValidateAsync(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Url");
    }

    /// <summary>
    /// Platform Publish Dto Validator_ Invalid Url With Spaces_ Fails.
    /// </summary>
    [Fact]
    public async Task PlatformPublishDtoValidator_InvalidUrlWithSpaces_Fails()
    {
        var sut = new PlatformPublishDtoValidator();
        var dto = new PlatformPublishDto(1, "", "https://exa mple.com");
        var result = await sut.ValidateAsync(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Url");
    }

    /// <summary>
    /// Platform Publish Dto Validator_ Invalid Url No Dot_ Fails.
    /// </summary>
    [Fact]
    public async Task PlatformPublishDtoValidator_InvalidUrlNoDot_Fails()
    {
        var sut = new PlatformPublishDtoValidator();
        var dto = new PlatformPublishDto(1, "", "https://example");
        var result = await sut.ValidateAsync(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Url");
    }

    /// <summary>
    /// Platform Publish Dto Validator_ Http Url_ Passes.
    /// </summary>
    [Fact]
    public async Task PlatformPublishDtoValidator_HttpUrl_Passes()
    {
        var sut = new PlatformPublishDtoValidator();
        var dto = new PlatformPublishDto(1, "", "http://example.com");
        var result = await sut.ValidateAsync(dto);
        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Social Media Publishing سجل Dto Validator_ Valid_ Passes.
    /// </summary>
    [Fact]
    public async Task SocialMediaPublishingLogDtoValidator_Valid_Passes()
    {
        var sut = new SocialMediaPublishingLogDtoValidator();
        var dto = new SocialMediaPublishingLogDto(0, 1, 1, "Clip", TimeSpan.FromMinutes(5), MediaType.Audio,
            [new PlatformPublishDto(1, "", "https://facebook.com")]);
        var result = await sut.ValidateAsync(dto);
        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Social Media Publishing سجل Dto Validator_ Missing الحلقة الضيف Id_ Fails.
    /// </summary>
    [Fact]
    public async Task SocialMediaPublishingLogDtoValidator_MissingEpisodeGuestId_Fails()
    {
        var sut = new SocialMediaPublishingLogDtoValidator();
        var dto = new SocialMediaPublishingLogDto(0, 0, 1, "Clip", TimeSpan.FromMinutes(5), MediaType.Audio,
            [new PlatformPublishDto(1, "", "https://facebook.com")]);
        var result = await sut.ValidateAsync(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EpisodeGuestId");
    }

    /// <summary>
    /// Social Media Publishing سجل Dto Validator_ Missing Clip Title_ Fails.
    /// </summary>
    [Fact]
    public async Task SocialMediaPublishingLogDtoValidator_MissingClipTitle_Fails()
    {
        var sut = new SocialMediaPublishingLogDtoValidator();
        var dto = new SocialMediaPublishingLogDto(0, 1, 1, "", TimeSpan.FromMinutes(5), MediaType.Audio,
            [new PlatformPublishDto(1, "", "https://facebook.com")]);
        var result = await sut.ValidateAsync(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ClipTitle");
    }

    /// <summary>
    /// Social Media Publishing سجل Dto Validator_ Empty Platforms_ Fails.
    /// </summary>
    [Fact]
    public async Task SocialMediaPublishingLogDtoValidator_EmptyPlatforms_Fails()
    {
        var sut = new SocialMediaPublishingLogDtoValidator();
        var dto = new SocialMediaPublishingLogDto(0, 1, 1, "Clip", TimeSpan.FromMinutes(5), MediaType.Audio, []);
        var result = await sut.ValidateAsync(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Platforms");
    }

    /// <summary>
    /// Social Media Publishing سجل Dto Validator_ Duration Exceeds12 Hours_ Fails.
    /// </summary>
    [Fact]
    public async Task SocialMediaPublishingLogDtoValidator_DurationExceeds12Hours_Fails()
    {
        var sut = new SocialMediaPublishingLogDtoValidator();
        var dto = new SocialMediaPublishingLogDto(0, 1, 1, "Clip", TimeSpan.FromHours(13), MediaType.Audio,
            [new PlatformPublishDto(1, "", "https://facebook.com")]);
        var result = await sut.ValidateAsync(dto);
        result.IsValid.Should().BeFalse();
    }

    /// <summary>
    /// Social Media Publishing سجل Dto Validator_ Null Duration_ Passes.
    /// </summary>
    [Fact]
    public async Task SocialMediaPublishingLogDtoValidator_NullDuration_Passes()
    {
        var sut = new SocialMediaPublishingLogDtoValidator();
        var dto = new SocialMediaPublishingLogDto(0, 1, 1, "Clip", null, MediaType.Audio,
            [new PlatformPublishDto(1, "", "https://facebook.com")]);
        var result = await sut.ValidateAsync(dto);
        result.IsValid.Should().BeTrue();
    }
}
