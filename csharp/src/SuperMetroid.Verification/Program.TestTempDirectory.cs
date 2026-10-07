internal static partial class Program
{
    /// <summary>
    /// One verifier call's uniquely named scratch directory inside ignored <c>csharp/test-temp</c>,
    /// deleted on dispose. Never touches pre-existing siblings.
    /// </summary>
    private sealed class TestTempDirectory : IDisposable
    {
        private static readonly string parent = Path.GetFullPath(Path.Combine("csharp", "test-temp"));
        public string Root { get; }

        public TestTempDirectory(string prefix)
        {
            Root = Path.GetFullPath(Path.Combine(parent, prefix + "-" + Guid.NewGuid().ToString("N")));
            if (Path.GetDirectoryName(Root) != parent ||
                !Path.GetFileName(Root).StartsWith(prefix + "-", StringComparison.Ordinal))
                throw new InvalidOperationException($"Verifier temporary directory {Root} escaped test-temp.");
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }
    }
}
