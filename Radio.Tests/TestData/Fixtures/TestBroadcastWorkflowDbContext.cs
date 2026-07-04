// ============================================================
// TestBroadcastWorkflowDbContext — سياق قاعدة البيانات للاختبار
// ============================================================
// المسؤولية: تعريف سياق قاعدة البيانات للاختبار.
// ============================================================
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Radio.Tests.TestData.Fixtures;

/// <summary>
/// صنف سياق قاعدة البيانات للاختبار.
/// </summary>
public class TestBroadcastWorkflowDbContext : BroadcastWorkflowDBContext
{
    public TestBroadcastWorkflowDbContext(DbContextOptions<BroadcastWorkflowDBContext> options)
        : base(options) { }

    /// <summary>
    /// عند نموذج Creating.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // No custom seeding override needed here for Role since it uses ASP.NET Core Identity
    }

    /// <summary>
    /// حفظ Changes.
    /// </summary>
    public override int SaveChanges()
    {
        FixRowVersions();
        return base.SaveChanges();
    }

    /// <summary>
    /// حفظ Changes Async.
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        FixRowVersions();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Fix Row Versions.
    /// </summary>
    private void FixRowVersions()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State != EntityState.Added && entry.State != EntityState.Modified)
                continue;

            if (entry.Entity is BaseEntity { RowVersion: null } be)
                be.RowVersion = [];
        }
    }
}
