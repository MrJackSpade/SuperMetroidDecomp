internal static partial class Program
{
    /// <summary>
    /// One verifier call's uniquely named scratch directory inside ignored <c>csharp/test-temp</c>,
    /// deleted on dispose. Never touches pre-existing siblings.
    /// </summary>
    private sealed class TestTempDirectory : IDisposable
    {
        /// <summary>Absolute path of the shared ignored scratch parent under <c>csharp/test-temp</c>.</summary>
        private static readonly string parent = Path.GetFullPath(Path.Combine("csharp", "test-temp"));

        /// <summary>Unique child directory reserved for this verifier call's temporary files.</summary>
        public string Root { get; }

        /// <summary>Creates a uniquely named scratch directory beneath the verifier's ignored temporary parent.</summary>
        /// <param name="prefix">Name prefix identifying the fixture or verifier that owns the scratch directory.</param>
        public TestTempDirectory(string prefix)
        {
            Root = Path.GetFullPath(Path.Combine(parent, prefix + "-" + Guid.NewGuid().ToString("N")));
            if (Path.GetDirectoryName(Root) != parent ||
                !Path.GetFileName(Root).StartsWith(prefix + "-", StringComparison.Ordinal))
                throw new InvalidOperationException($"Verifier temporary directory {Root} escaped test-temp.");
        }

        /// <summary>Deletes this instance's scratch directory and its contents when it exists.</summary>
        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }
    }
}
