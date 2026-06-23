using DataAccess.Common;
using DataAccess.DTOs;
using Domain.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace DataAccess.Services;

public interface IProgramQueryService
{
    Task<List<ProgramDto>> GetAllActiveAsync(CancellationToken cancellationToken = default);
}

public interface IProgramCommandService
{
    Task<Result> CreateProgramAsync(ProgramDto dto, UserSession session, CancellationToken cancellationToken = default);
    Task<Result> UpdateProgramAsync(ProgramDto dto, UserSession session, CancellationToken cancellationToken = default);
    Task<Result> SoftDeleteAsync(int programId, UserSession session, CancellationToken cancellationToken = default);
}

public interface IProgramService : IProgramQueryService, IProgramCommandService { }

// ✨ استخدام Primary Constructor
public class ProgramService : IProgramService
{
    private readonly IDbContextFactory<BroadcastWorkflowDBContext> _contextFactory;
    private readonly ICachedLookupService _cachedLookup;
    private readonly ILogger<ProgramService> _logger;
    private readonly IValidator<ProgramDto> _programValidator;

    public ProgramService(
        IDbContextFactory<BroadcastWorkflowDBContext> contextFactory,
        ICachedLookupService cachedLookup,
        ILogger<ProgramService> logger,
        IValidator<ProgramDto> programValidator)
    {
        _contextFactory = contextFactory;
        _cachedLookup = cachedLookup;
        _logger = logger;
        _programValidator = programValidator;
    }
    // ──────────────────────────────────────────────────────────────
    // Compiled Query — تقليل وقت ترجمة LINQ في المسارات الساخنة
    // ──────────────────────────────────────────────────────────────
    private static readonly Func<BroadcastWorkflowDBContext, IAsyncEnumerable<ProgramDto>> s_compiledGetAllActive =
        EF.CompileAsyncQuery((BroadcastWorkflowDBContext context) =>
            context.Programs
                .AsNoTracking()
                .Select(p => new ProgramDto
                (
                    p.ProgramId,
                    p.ProgramName,
                    p.Category,
                    p.ProgramDescription)));

    public async Task<List<ProgramDto>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var result = new List<ProgramDto>();
        await foreach (var dto in s_compiledGetAllActive(context))
            result.Add(dto);
        return result;
    }

    public async Task<Result> CreateProgramAsync(ProgramDto dto, UserSession session, CancellationToken cancellationToken = default)
    {
        var permCheck = session.EnsurePermission(AppPermissions.CoordinationManage);
        if (!permCheck.IsSuccess) return Result.Fail(permCheck.ErrorMessage!);

        var validation = await _programValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return Result.Fail(string.Join(Environment.NewLine, validation.Errors.Select(e => e.ErrorMessage)));

        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            context.Programs.Add(new Program
            {
                ProgramName = dto.ProgramName,
                Category = dto.Category,
                ProgramDescription = dto.ProgramDescription
            });

            await context.SaveChangesAsync(cancellationToken);
            await _cachedLookup.InvalidateByEntity("Program");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Program: {ProgramName}", dto.ProgramName);
            return Result.Fail("حدث خطأ في قاعدة البيانات أثناء إضافة البرنامج. يرجى المحاولة لاحقاً.");
        }
    }

    public async Task<Result> UpdateProgramAsync(ProgramDto dto, UserSession session, CancellationToken cancellationToken = default)
    {
        var permCheck = session.EnsurePermission(AppPermissions.CoordinationManage);
        if (!permCheck.IsSuccess) return Result.Fail(permCheck.ErrorMessage!);

        var validation = await _programValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return Result.Fail(string.Join(Environment.NewLine, validation.Errors.Select(e => e.ErrorMessage)));

        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var prog = await context.Programs.FindAsync(dto.ProgramId);

            if (prog == null) return Result.Fail("البرنامج غير موجود.");

            prog.ProgramName = dto.ProgramName;
            prog.Category = dto.Category;
            prog.ProgramDescription = dto.ProgramDescription;

            await context.SaveChangesAsync(cancellationToken);
            await _cachedLookup.InvalidateByEntity("Program");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update Program: {ProgramId}, {ProgramName}", dto.ProgramId, dto.ProgramName);
            return Result.Fail("حدث خطأ في قاعدة البيانات أثناء تعديل البرنامج. يرجى المحاولة لاحقاً.");
        }
    }


    /// <summary>
    /// حذف برنامج بشكل ناعم (Soft Delete).
    /// </summary>
    /// <param name="programId">معرّف البرنامج المراد حذفه.</param>
    /// <param name="session">جلسة المستخدم الحالي للتدقيق.</param>
    public async Task<Result> SoftDeleteAsync(int programId, UserSession session, CancellationToken cancellationToken = default)
    {
        var permCheck = session.EnsurePermission(AppPermissions.ProgramManage);
        if (!permCheck.IsSuccess) return Result.Fail(permCheck.ErrorMessage!);

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var program = await context.Programs.FindAsync(programId);

            if (program == null) return Result.Fail("البرنامج المحدد غير موجود أو تم حذفه مسبقاً.");

            // ── فحص وجود حلقات نشطة باستخدام AnyAsync بدلاً من Lazy Loading ──
            var hasActiveEpisodes = await context.Episodes.AnyAsync(e => e.ProgramId == programId, cancellationToken);
            if (hasActiveEpisodes)
                return Result.Fail("لا يمكن حذف برنامج مرتبط بحلقات نشطة. يرجى حذف الحلقات أولاً.");

            program.IsActive = false;

            await context.SaveChangesAsync(cancellationToken);
            await _cachedLookup.InvalidateByEntity("Program");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to soft delete Program: {ProgramId}", programId);
            return Result.Fail("حدث خطأ في قاعدة البيانات أثناء حذف البرنامج. يرجى المحاولة لاحقاً.");
        }
    }
}