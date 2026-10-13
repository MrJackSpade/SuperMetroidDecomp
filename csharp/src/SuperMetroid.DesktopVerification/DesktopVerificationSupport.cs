using System.Reflection;
using SuperMetroid.Rendering.Direct3D11;

/// <summary>Shared waits, assertions and host-handler access for the desktop verification checks.</summary>
internal static class DesktopVerificationSupport
{
    /// <summary>Polls for a rendered condition for up to ten seconds, surfacing worker faults while waiting.</summary>
    internal static async Task Until(Func<bool> condition, D3D11RenderWorker worker)
    {
        long deadline = Environment.TickCount64 + 10000;
        while (!condition())
        {
            worker.ThrowIfFaulted();
            if (Environment.TickCount64 >= deadline) throw new TimeoutException("Desktop GPU did not consume latest display.");
            await Task.Delay(10);
        }
    }

    /// <summary>Raises a verification failure with the supplied context when an assertion is false.</summary>
    internal static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
