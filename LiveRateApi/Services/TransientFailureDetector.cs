using System.Data.Common;

namespace LiveRateApi.Services;

internal static class TransientFailureDetector
{
    public static bool IsTransient(Exception exception, CancellationToken requestCancellation)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is OperationCanceledException)
            {
                // Do not turn a client disconnect or application shutdown into a retry.
                return !requestCancellation.IsCancellationRequested;
            }

            if (current is TimeoutException or HttpRequestException or System.Text.Json.JsonException)
            {
                return true;
            }

            if (current is DbException databaseException && IsDatabaseTimeout(databaseException))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsDatabaseTimeout(DbException exception)
    {
        // SqlException.Number is -2 for a command timeout. Reflection keeps this
        // project provider-neutral and avoids taking a dependency on one SQL client.
        var number = exception.GetType().GetProperty("Number")?.GetValue(exception);
        return number is -2 || exception.ErrorCode == -2;
    }
}
