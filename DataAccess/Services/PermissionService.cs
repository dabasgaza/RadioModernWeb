using DataAccess.Common;
using DataAccess.DTOs;
using System;
using System.Linq;
using System.Reflection;

namespace DataAccess.Services
{
    /// <summary>
    /// واجهة خدمة الصلاحيات.
    /// </summary>
    public interface IPermissionService
    {
        Task<Result<List<PermissionDto>>> GetAllPermissionsAsync(CancellationToken cancellationToken = default);
        Task<Result<PermissionDto>> GetPermissionByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<List<PermissionDto>> GetPermissionsListAsync();
    }

    /// <summary>
    /// خدمة الصلاحيات باستخدام القراءة بالانعكاس (Reflection) من ثوابت AppPermissions
    /// وتعيين معرفات افتراضية (Synthetic IDs) للتوافق التام مع الواجهات والنظام.
    /// </summary>
    public class PermissionService : IPermissionService
    {
        private static readonly List<PermissionDto> _cachedPermissions;

        static PermissionService()
        {
            _cachedPermissions = new List<PermissionDto>();
            var fields = typeof(AppPermissions).GetFields(
                BindingFlags.Public |
                BindingFlags.Static |
                BindingFlags.FlattenHierarchy);

            int idCounter = 1;
            foreach (var field in fields)
            {
                if (field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
                {
                    var attr = (PermissionInfoAttribute?)Attribute.GetCustomAttribute(field, typeof(PermissionInfoAttribute));
                    if (attr == null) continue; // تجاوز الصلاحيات القديمة المتوافقة تلقائياً لتفادي تكرارها وقسم "عام"

                    var systemName = (string)field.GetValue(null)!;
                    _cachedPermissions.Add(new PermissionDto(idCounter++, systemName, attr.DisplayName, attr.Module));
                }
            }
        }

        public Task<List<PermissionDto>> GetPermissionsListAsync()
        {
            return Task.FromResult(_cachedPermissions);
        }

        public Task<Result<List<PermissionDto>>> GetAllPermissionsAsync(CancellationToken cancellationToken = default)
        {
            var ordered = _cachedPermissions
                .OrderBy(p => p.Module)
                .ThenBy(p => p.DisplayName)
                .ToList();

            return Task.FromResult(Result<List<PermissionDto>>.Success(ordered));
        }

        public Task<Result<PermissionDto>> GetPermissionByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var p = _cachedPermissions.FirstOrDefault(x => x.PermissionId == id);
            if (p is null)
                return Task.FromResult(Result<PermissionDto>.Fail("الصلاحية المطلوبة غير موجودة."));

            return Task.FromResult(Result<PermissionDto>.Success(p));
        }
    }
}
