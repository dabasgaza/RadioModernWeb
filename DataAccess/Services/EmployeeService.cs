// ============================================================
// EmployeeService — الموظف
// ============================================================
// المسؤولية: تعريف الموظف.
// ============================================================
using DataAccess.Common;
using DataAccess.DTOs;
using Domain.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess.Services;

/// <summary>
/// واجهة I الموظف.
/// </summary>
public interface IEmployeeService
{
    Task<List<EmployeeDto>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<Result<int>> CreateAsync(EmployeeDto dto, UserSession session, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(EmployeeDto dto, UserSession session, CancellationToken cancellationToken = default);
    Task<Result> SoftDeleteAsync(int employeeId, UserSession session, CancellationToken cancellationToken = default);

    Task<List<StaffRoleDto>> GetAllRolesAsync(CancellationToken cancellationToken = default);
    Task<Result<int>> CreateRoleAsync(StaffRoleDto dto, UserSession session, CancellationToken cancellationToken = default);
    Task<Result> UpdateRoleAsync(StaffRoleDto dto, UserSession session, CancellationToken cancellationToken = default);
    Task<Result> SoftDeleteRoleAsync(int roleId, UserSession session, CancellationToken cancellationToken = default);
}

// ✨ استخدام Primary Constructor
/// <summary>
/// صنف الموظف.
/// </summary>
public class EmployeeService : IEmployeeService
{
    private readonly IDbContextFactory<BroadcastWorkflowDBContext> _contextFactory;
    private readonly ICachedLookupService _cachedLookup;
    private readonly ILogger<EmployeeService> _logger;
    private readonly IValidator<EmployeeDto> _employeeValidator;
    private readonly IValidator<StaffRoleDto> _staffRoleValidator;

    public EmployeeService(
        IDbContextFactory<BroadcastWorkflowDBContext> contextFactory,
        ICachedLookupService cachedLookup,
        ILogger<EmployeeService> logger,
        IValidator<EmployeeDto> employeeValidator,
        IValidator<StaffRoleDto> staffRoleValidator)
    {
        _contextFactory = contextFactory;
        _cachedLookup = cachedLookup;
        _logger = logger;
        _employeeValidator = employeeValidator;
        _staffRoleValidator = staffRoleValidator;
    }
    // ──────────────────────────────────────────────────────────────
    // Compiled Queries — تقليل وقت ترجمة LINQ في المسارات الساخنة
    // ──────────────────────────────────────────────────────────────
    private static readonly Func<BroadcastWorkflowDBContext, IAsyncEnumerable<EmployeeDto>> s_compiledGetAllActive =
        EF.CompileAsyncQuery((BroadcastWorkflowDBContext context) =>
            context.Employees
                .AsNoTracking()
                .Select(e => new EmployeeDto(
                    e.EmployeeId,
                    e.FullName,
                    e.StaffRoleId,
                    e.StaffRole != null ? e.StaffRole.RoleName : null,
                    e.Notes)));

    private static readonly Func<BroadcastWorkflowDBContext, IAsyncEnumerable<StaffRoleDto>> s_compiledGetAllRoles =
        EF.CompileAsyncQuery((BroadcastWorkflowDBContext context) =>
            context.StaffRoles
                .AsNoTracking()
                .Where(r => r.IsActive)
                .Select(r => new StaffRoleDto(r.StaffRoleId, r.RoleName)));

    /// <summary>
    /// استرجاع النشط Async.
    /// </summary>
    public async Task<List<EmployeeDto>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var result = new List<EmployeeDto>();
        await foreach (var dto in s_compiledGetAllActive(context))
            result.Add(dto);
        return result;
    }

    /// <summary>
    /// إنشاء Async.
    /// </summary>
    public async Task<Result<int>> CreateAsync(EmployeeDto dto, UserSession session, CancellationToken cancellationToken = default)
    {
        var permCheck = session.EnsurePermission(AppPermissions.StaffManage);
        if (!permCheck.IsSuccess) return Result<int>.Fail(permCheck.ErrorMessage!);

        var validation = await _employeeValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return Result<int>.Fail(string.Join(Environment.NewLine, validation.Errors.Select(e => e.ErrorMessage)));

        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var employee = new Employee
            {
                FullName = dto.FullName,
                StaffRoleId = dto.StaffRoleId,
                Notes = dto.Notes,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.Employees.Add(employee);
            await context.SaveChangesAsync(cancellationToken);
            await _cachedLookup.InvalidateByEntity("Employee");
            return Result<int>.Success(employee.EmployeeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Employee: {FullName}", dto.FullName);
            return Result<int>.Fail("حدث خطأ في قاعدة البيانات أثناء إضافة الموظف. يرجى المحاولة لاحقاً.");
        }
    }

    /// <summary>
    /// تحديث Async.
    /// </summary>
    public async Task<Result> UpdateAsync(EmployeeDto dto, UserSession session, CancellationToken cancellationToken = default)
    {
        var permCheck = session.EnsurePermission(AppPermissions.StaffManage);
        if (!permCheck.IsSuccess) return Result.Fail(permCheck.ErrorMessage!);

        var validation = await _employeeValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return Result.Fail(string.Join(Environment.NewLine, validation.Errors.Select(e => e.ErrorMessage)));

        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var employee = await context.Employees.FindAsync(dto.EmployeeId);
            if (employee == null)
                return Result.Fail("الموظف غير موجود");

            employee.FullName = dto.FullName;
            employee.StaffRoleId = dto.StaffRoleId;
            employee.Notes = dto.Notes;
            employee.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);
            await _cachedLookup.InvalidateByEntity("Employee");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update Employee: {EmployeeId}, {FullName}", dto.EmployeeId, dto.FullName);
            return Result.Fail("حدث خطأ في قاعدة البيانات أثناء تعديل بيانات الموظف. يرجى المحاولة لاحقاً.");
        }
    }

    /// <summary>
    /// Soft Delete Async.
    /// </summary>
    public async Task<Result> SoftDeleteAsync(int employeeId, UserSession session, CancellationToken cancellationToken = default)
    {
        var permCheck = session.EnsurePermission(AppPermissions.StaffManage);
        if (!permCheck.IsSuccess) return Result.Fail(permCheck.ErrorMessage!);

        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var employee = await context.Employees.FindAsync(employeeId);
            if (employee == null)
                return Result.Fail("الموظف غير موجود");

            employee.IsActive = false;
            employee.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);
            await _cachedLookup.InvalidateByEntity("Employee");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to soft delete Employee: {EmployeeId}", employeeId);
            return Result.Fail("حدث خطأ في قاعدة البيانات أثناء حذف الموظف. يرجى المحاولة لاحقاً.");
        }
    }

    /// <summary>
    /// استرجاع الكل الأدوار Async.
    /// </summary>
    public async Task<List<StaffRoleDto>> GetAllRolesAsync(CancellationToken cancellationToken = default)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var result = new List<StaffRoleDto>();
        await foreach (var dto in s_compiledGetAllRoles(context))
            result.Add(dto);
        return result;
    }

    /// <summary>
    /// إنشاء الدور Async.
    /// </summary>
    public async Task<Result<int>> CreateRoleAsync(StaffRoleDto dto, UserSession session, CancellationToken cancellationToken = default)
    {
        var permCheck = session.EnsurePermission(AppPermissions.StaffManage);
        if (!permCheck.IsSuccess) return Result<int>.Fail(permCheck.ErrorMessage!);

        var validation = await _staffRoleValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return Result<int>.Fail(string.Join(Environment.NewLine, validation.Errors.Select(e => e.ErrorMessage)));

        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var role = new StaffRole
            {
                RoleName = dto.RoleName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.StaffRoles.Add(role);
            await context.SaveChangesAsync(cancellationToken);
            await _cachedLookup.InvalidateByEntity("StaffRole");
            return Result<int>.Success(role.StaffRoleId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create StaffRole: {RoleName}", dto.RoleName);
            return Result<int>.Fail("حدث خطأ في قاعدة البيانات أثناء إضافة الدور الوظيفي. يرجى المحاولة لاحقاً.");
        }
    }

    /// <summary>
    /// تحديث الدور Async.
    /// </summary>
    public async Task<Result> UpdateRoleAsync(StaffRoleDto dto, UserSession session, CancellationToken cancellationToken = default)
    {
        var permCheck = session.EnsurePermission(AppPermissions.StaffManage);
        if (!permCheck.IsSuccess) return Result.Fail(permCheck.ErrorMessage!);

        var validation = await _staffRoleValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return Result.Fail(string.Join(Environment.NewLine, validation.Errors.Select(e => e.ErrorMessage)));

        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var role = await context.StaffRoles.FindAsync(dto.StaffRoleId);
            if (role == null)
                return Result.Fail("الدور الوظيفي غير موجود");

            role.RoleName = dto.RoleName;
            role.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);
            await _cachedLookup.InvalidateByEntity("StaffRole");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update StaffRole: {RoleId}, {RoleName}", dto.StaffRoleId, dto.RoleName);
            return Result.Fail("حدث خطأ في قاعدة البيانات أثناء تعديل الدور الوظيفي. يرجى المحاولة لاحقاً.");
        }
    }

    /// <summary>
    /// Soft Delete الدور Async.
    /// </summary>
    public async Task<Result> SoftDeleteRoleAsync(int roleId, UserSession session, CancellationToken cancellationToken = default)
    {
        var permCheck = session.EnsurePermission(AppPermissions.StaffManage);
        if (!permCheck.IsSuccess) return Result.Fail(permCheck.ErrorMessage!);

        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var role = await context.StaffRoles.FindAsync(roleId);
            if (role == null)
                return Result.Fail("الدور الوظيفي غير موجود");

            role.IsActive = false;
            role.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);
            await _cachedLookup.InvalidateByEntity("StaffRole");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to soft delete StaffRole: {RoleId}", roleId);
            return Result.Fail("حدث خطأ في قاعدة البيانات أثناء حذف الدور الوظيفي. يرجى المحاولة لاحقاً.");
        }
    }
}