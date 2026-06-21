using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace DataAccess.Data
{
    public class DbQueryPerformanceInterceptor : DbCommandInterceptor
    {
        private readonly int _slowQueryThresholdMs;
        private readonly ILogger<DbQueryPerformanceInterceptor> _logger;

        public DbQueryPerformanceInterceptor(ILogger<DbQueryPerformanceInterceptor> logger, int slowQueryThresholdMs = 100)
        {
            _logger = logger;
            _slowQueryThresholdMs = slowQueryThresholdMs;
        }

        public override DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
        {
            LogQuery(command, eventData);
            return base.ReaderExecuted(command, eventData, result);
        }

        public override ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData, DbDataReader result, CancellationToken cancellationToken = default)
        {
            LogQuery(command, eventData);
            return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
        }

        public override int NonQueryExecuted(DbCommand command, CommandExecutedEventData eventData, int result)
        {
            LogQuery(command, eventData);
            return base.NonQueryExecuted(command, eventData, result);
        }

        public override ValueTask<int> NonQueryExecutedAsync(DbCommand command, CommandExecutedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            LogQuery(command, eventData);
            return base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
        }

        public override object? ScalarExecuted(DbCommand command, CommandExecutedEventData eventData, object? result)
        {
            LogQuery(command, eventData);
            return base.ScalarExecuted(command, eventData, result);
        }

        public override ValueTask<object?> ScalarExecutedAsync(DbCommand command, CommandExecutedEventData eventData, object? result, CancellationToken cancellationToken = default)
        {
            LogQuery(command, eventData);
            return base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
        }

        public override void CommandFailed(DbCommand command, CommandErrorEventData eventData)
        {
            _logger.LogError(eventData.Exception, "SQL Command Failed: {Sql}", command.CommandText);
            base.CommandFailed(command, eventData);
        }

        public override Task CommandFailedAsync(DbCommand command, CommandErrorEventData eventData, CancellationToken cancellationToken = default)
        {
            _logger.LogError(eventData.Exception, "SQL Command Failed: {Sql}", command.CommandText);
            return base.CommandFailedAsync(command, eventData, cancellationToken);
        }

        private void LogQuery(DbCommand command, CommandExecutedEventData eventData)
        {
            var durationMs = eventData.Duration.TotalMilliseconds;
            var sql = command.CommandText;

            if (durationMs >= _slowQueryThresholdMs)
            {
                _logger.LogWarning("Slow SQL Query Detected: {Sql} took {DurationMs}ms", sql, durationMs);
            }
            else
            {
                _logger.LogInformation("SQL Query Executed: {Sql} took {DurationMs}ms", sql, durationMs);
            }
        }
    }
}
