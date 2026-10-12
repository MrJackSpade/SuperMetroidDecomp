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

    /// <summary>Invokes an actual nonpublic toolbar handler without adding a verification hook to the host.</summary>
    internal static void Call(object owner, string name, params object[] args) =>
        (owner.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingMethodException(name)).Invoke(owner, args);
    /// <summary>Invokes a nonpublic asynchronous handler and requires it to return a task.</summary>
    internal static Task CallAsync(object owner, string name, params object[] args) =>
        (Task)((owner.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingMethodException(name)).Invoke(owner, args)
            ?? throw new InvalidOperationException("Async handler returned no task."));
    /// <summary>Reads a nonpublic instance field and fails when that field is absent or null.</summary>
    internal static T Field<T>(object owner, string name) =>
        (T)(owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(owner)
            ?? throw new MissingFieldException(name));
    /// <summary>Raises a verification failure with the supplied context when an assertion is false.</summary>
    internal static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
