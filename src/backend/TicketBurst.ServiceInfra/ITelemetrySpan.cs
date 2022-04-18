using System.Diagnostics;
using OpenTelemetry.Trace;

namespace TicketBurst.ServiceInfra;

public interface ITelemetrySpan : IDisposable
{
    void Succeed();
    void Fail(Exception error);
    void Fail(string reason);
}

public class NoopSpan : ITelemetrySpan
{
    public static readonly NoopSpan Instance = new NoopSpan();
    
    public void Dispose()
    {
    }

    public void Succeed()
    {
    }

    public void Fail(Exception error)
    {
    }

    public void Fail(string reason)
    {
    }
}

public class ActivitySpan : ITelemetrySpan
{
    private static readonly string __s_error = "error";
    private static readonly string __s_exception = "exception";
    private static readonly string __s_reason = "reason";
    private static readonly string __s_true = "true";
    
    public readonly Activity Activity;

    public ActivitySpan(Activity activity)
    {
        Activity = activity;
    }

    public void Dispose()
    {
        Activity.Dispose();
    }

    public void Succeed()
    {
        Activity.SetStatus(ActivityStatusCode.Ok);
    }

    public void Fail(Exception error)
    {
        Activity.SetTag(__s_error, __s_true);
        Activity.SetTag(__s_exception, error.Message);
    }

    public void Fail(string reason)
    {
        Activity.SetTag(__s_error, __s_true);
        Activity.SetTag(__s_reason, reason);
    }
}
