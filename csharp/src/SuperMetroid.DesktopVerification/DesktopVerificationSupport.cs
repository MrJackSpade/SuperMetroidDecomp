using System.Reflection;
using SuperMetroid.Rendering.Direct3D11;

/// <summary>Shared waits, assertions and host-handler access for the desktop verification checks.</summary>
internal static class DesktopVerificationSupport
{
    /// <summary>until.</summary>
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

    // Exercise the actual toolbar handlers without exposing debug-only production APIs.
    /// <summary>call.</summary>
    internal static void Call(object owner, string name, params object[] args) =>
        (owner.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingMethodException(name)).Invoke(owner, args);
    /// <summary>call async.</summary>
    internal static Task CallAsync(object owner, string name, params object[] args) =>
        (Task)((owner.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingMethodException(name)).Invoke(owner, args)
            ?? throw new InvalidOperationException("Async handler returned no task."));
    /// <summary>field.</summary>
    internal static T Field<T>(object owner, string name) =>
        (T)(owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(owner)
            ?? throw new MissingFieldException(name));
    /// <summary>check.</summary>
    internal static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
