namespace SuperMetroid.DesktopVerification;

/// <summary>A named desktop check.</summary>
/// <param name="Name">The name.</param>
/// <param name="Run">The run.</param>
internal readonly record struct DesktopCheck(string Name, Func<Task> Run);
