using System.Diagnostics;

namespace SuperMetroid.DesktopVerification.Harness;

/// <summary>A started desktop check process and its captured output.</summary>
/// <param name="Name">The name.</param>
/// <param name="Process">The process.</param>
/// <param name="Output">The output.</param>
/// <param name="Error">The error.</param>
internal readonly record struct DesktopCheckProcess(string Name, Process Process, Task<string> Output, Task<string> Error);
