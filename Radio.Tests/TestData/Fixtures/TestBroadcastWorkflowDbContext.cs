using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Radio.Tests.TestData.Fixtures;

public class TestBroadcastWorkflowDbContext : BroadcastWorkflowDBContext
{
    public TestBroadcastWorkflowDbContext(DbContextOptions<BroadcastWorkflowDBContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Remove seed data from model configuration to avoid InMemory RowVersion conflicts
        modelBuilder.Entity<Role>().HasData();
    }

    public override int SaveChanges()
    {
        FixRowVersions();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        FixRowVersions();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void FixRowVersions()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State != EntityState.Added && entry.State != EntityState.Modified)
                continue;

            if (entry.Entity is BaseEntity { RowVersion: null } be)
                be.RowVersion = [];

            // Role has its own RowVersion property and doesn't inherit BaseEntity
            if (entry.Entity is Role { RowVersion: null } role)
                role.RowVersion = [];
        }
    }
}
