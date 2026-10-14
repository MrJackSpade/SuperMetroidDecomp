namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// One test's uniquely named scratch directory inside ignored <c>csharp/test-temp/scratch</c>, deleted when disposed.
/// Tests own their scratch: they create it with <c>using</c>, so it is removed whether the test passes or
/// throws, and nothing a test writes outlives it. <c>tools/verify-all.ps1</c> also clears <c>scratch</c> at the start
/// of each run, so a killed run's leftovers never accumulate. Never touches pre-existing siblings.
/// </summary>
internal sealed class TestScratchDirectory : IDisposable
{
    /// <summary>Absolute parent boundary used to reject a generated scratch path outside the ignored test directory.</summary>
    private static readonly string Parent = Path.GetFullPath(Path.Combine("csharp", "test-temp", "scratch"));

    /// <summary>Reserves the path <c>csharp/test-temp/scratch/{prefix}-{guid}</c>; the test creates it when it needs it.</summary>
    public TestScratchDirectory(string prefix)
    {
        Root = Path.GetFullPath(Path.Combine(Parent, prefix + "-" + Guid.NewGuid().ToString("N")));
        if (Path.GetDirectoryName(Root) != Parent || !Path.GetFileName(Root).StartsWith(prefix + "-", StringComparison.Ordinal))
            throw new InvalidOperationException($"Test scratch directory {Root} escaped test-temp/scratch.");
    }

    /// <summary>The scratch directory's absolute path.</summary>
    public string Root { get; }

    /// <summary>Deletes the scratch directory and everything the test wrote into it.</summary>
    public void Dispose()
    {
        if (Directory.Exists(Root))
            Directory.Delete(Root, recursive: true);
    }
}
