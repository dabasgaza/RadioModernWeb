using System.Threading;
using System.Threading.Tasks;
using DataAccess.Common;
using DataAccess.Services;
using FluentAssertions;
using Xunit;

namespace Radio.Tests.Services
{
    public class PermissionServiceTests
    {
        private readonly PermissionService _sut;

        public PermissionServiceTests()
        {
            _sut = new PermissionService();
        }

        [Fact]
        public async Task GetAllPermissionsAsync_ReturnsAll()
        {
            var result = await _sut.GetAllPermissionsAsync(CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Contain(p => p.SystemName == AppPermissions.UserManage);
            result.Value.Should().Contain(p => p.SystemName == AppPermissions.ProgramManage);
            result.Value.Count.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetPermissionByIdAsync_Existing_ReturnsPermission()
        {
            // بما أن المعرفات افتراضية وتبدأ من 1، فإن المعرف 1 موجود دائماً
            var result = await _sut.GetPermissionByIdAsync(1, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.SystemName.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetPermissionByIdAsync_NonExisting_ReturnsFail()
        {
            var result = await _sut.GetPermissionByIdAsync(999, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }
    }
}
