// ============================================================
// CorrespondentService — المراسل
// ============================================================
// المسؤولية: تعريف المراسل.
// ============================================================
using DataAccess.Common;
using DataAccess.DTOs;
using Domain.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess.Services
{
    /// <summary>
    /// واجهة I المراسل.
    /// </summary>
    public interface ICorrespondentService
    {
        // ✨ إرجاع DTOs بدلاً من الكيانات
        Task<List<CorrespondentDto>> GetAllActiveAsync(CancellationToken cancellationToken = default);
        Task<Result> CreateAsync(CorrespondentDto dto, UserSession session, CancellationToken cancellationToken = default);
        Task<Result> UpdateAsync(CorrespondentDto dto, UserSession session, CancellationToken cancellationToken = default);
        Task<Result> SoftDeleteAsync(int id, UserSession session, CancellationToken cancellationToken = default);
        Task<List<CorrespondentCoverageDto>> GetCoverageAsync(int correspondentId, CancellationToken cancellationToken = default);
    }

    // ✨ استخدام Primary Constructor
    /// <summary>
    /// صنف المراسل.
    /// </summary>
    public class CorrespondentService : ICorrespondentService
    {
        private readonly IDbContextFactory<BroadcastWorkflowDBContext> _contextFactory;
        private readonly ICachedLookupService _cachedLookup;
        private readonly ILogger<CorrespondentService> _logger;
        private readonly IValidator<CorrespondentDto> _correspondentValidator;

        public CorrespondentService(
            IDbContextFactory<BroadcastWorkflowDBContext> contextFactory,
            ICachedLookupService cachedLookup,
            ILogger<CorrespondentService> logger,
            IValidator<CorrespondentDto> correspondentValidator)
        {
            _contextFactory = contextFactory;
            _cachedLookup = cachedLookup;
            _logger = logger;
            _correspondentValidator = correspondentValidator;
        }
        // ──────────────────────────────────────────────────────────────
        // Compiled Query — تقليل وقت ترجمة LINQ في المسارات الساخنة
        // ──────────────────────────────────────────────────────────────
        private static readonly Func<BroadcastWorkflowDBContext, IAsyncEnumerable<CorrespondentDto>> s_compiledGetAllActive =
            EF.CompileAsyncQuery((BroadcastWorkflowDBContext context) =>
                context.Correspondents
                    .AsNoTracking()
                    .Select(c => new CorrespondentDto
                    (
                        c.CorrespondentId,
                        c.FullName,
                        c.PhoneNumber,
                        c.AssignedLocations
                    )));

        /// <summary>
        /// استرجاع النشط Async.
        /// </summary>
        public async Task<List<CorrespondentDto>> GetAllActiveAsync(CancellationToken cancellationToken = default)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var result = new List<CorrespondentDto>();
            await foreach (var dto in s_compiledGetAllActive(context))
                result.Add(dto);
            return result;
        }

        /// <summary>
        /// إنشاء Async.
        /// </summary>
        public async Task<Result> CreateAsync(CorrespondentDto dto, UserSession session, CancellationToken cancellationToken = default)
        {
            var permCheck = session.EnsurePermission(AppPermissions.CoordinationManage);
            if (!permCheck.IsSuccess) return Result.Fail(permCheck.ErrorMessage!);

            var validation = await _correspondentValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return Result.Fail(string.Join(Environment.NewLine, validation.Errors.Select(e => e.ErrorMessage)));

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                context.Correspondents.Add(new Correspondent
                {
                    FullName = dto.FullName,
                    PhoneNumber = dto.PhoneNumber,
                    AssignedLocations = dto.AssignedLocations
                });

                await context.SaveChangesAsync(cancellationToken);
                await _cachedLookup.InvalidateByEntity("Correspondent");
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create Correspondent: {CorrespondentName}", dto.FullName);
                return Result.Fail("حدث خطأ في قاعدة البيانات أثناء إضافة المراسل. يرجى المحاولة لاحقاً.");
            }
        }

        /// <summary>
        /// تحديث Async.
        /// </summary>
        public async Task<Result> UpdateAsync(CorrespondentDto dto, UserSession session, CancellationToken cancellationToken = default)
        {
            var permCheck = session.EnsurePermission(AppPermissions.CoordinationManage);
            if (!permCheck.IsSuccess) return Result.Fail(permCheck.ErrorMessage!);

            var validation = await _correspondentValidator.ValidateAsync(dto);
            if (!validation.IsValid)
                return Result.Fail(string.Join(Environment.NewLine, validation.Errors.Select(e => e.ErrorMessage)));

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var cor = await context.Correspondents.FindAsync(dto.CorrespondentId);

                if (cor == null) return Result.Fail("المراسل غير موجود.");

                cor.FullName = dto.FullName;
                cor.PhoneNumber = dto.PhoneNumber;
                cor.AssignedLocations = dto.AssignedLocations;

                await context.SaveChangesAsync(cancellationToken);
                await _cachedLookup.InvalidateByEntity("Correspondent");
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update Correspondent: {CorrespondentId}, {CorrespondentName}", dto.CorrespondentId, dto.FullName);
                return Result.Fail("حدث خطأ في قاعدة البيانات أثناء تعديل بيانات المراسل. يرجى المحاولة لاحقاً.");
            }
        }

        /// <summary>
        /// Soft Delete Async.
        /// </summary>
        public async Task<Result> SoftDeleteAsync(int id, UserSession session, CancellationToken cancellationToken = default)
        {
            var permCheck = session.EnsurePermission(AppPermissions.CoordinationManage);
            if (!permCheck.IsSuccess) return Result.Fail(permCheck.ErrorMessage!);

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var cor = await context.Correspondents.FindAsync(id);

                if (cor == null) return Result.Fail("المراسل غير موجود.");

                cor.IsActive = false;

                await context.SaveChangesAsync(cancellationToken);
                await _cachedLookup.InvalidateByEntity("Correspondent");
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to soft delete Correspondent: {CorrespondentId}", id);
                return Result.Fail("حدث خطأ في قاعدة البيانات أثناء حذف المراسل. يرجى المحاولة لاحقاً.");
            }
        }

        /// <summary>
        /// استرجاع التغطية Async.
        /// </summary>
        public async Task<List<CorrespondentCoverageDto>> GetCoverageAsync(int correspondentId, CancellationToken cancellationToken = default)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            // ✅ إزالة Include غير الضروري — Select يجلب العلاقة تلقائياً عبر SQL JOIN
            return await context.CorrespondentCoverages
                .AsNoTracking()
                .Where(c => c.CorrespondentId == correspondentId)
                .Select(c => new CorrespondentCoverageDto
                {
                    CoverageId = c.CoverageId,
                    Topic = c.Topic,
                    Location = c.Location,
                    GuestName = c.Guest != null ? c.Guest.FullName : "لا يوجد ضيف"
                })
                .ToListAsync(cancellationToken);
        }
    }
}