namespace SuperMetroid.DesktopVerification.Harness;

/// <summary>
/// The desktop verifier's own assembly file, which a child verifier run through <c>dotnet</c> is started from (#2602). It is
/// named by its file in the verifier's output directory, never found by reading a loaded type.
/// </summary>
internal static class VerifierAssembly
{
    /// <summary>The verifier's assembly file, which must exist.</summary>
    /// <exception cref="FileNotFoundException">The verifier's output directory holds no verifier assembly.</exception>
    internal static string Path
    {
        get
        {
            string path = System.IO.Path.Combine(AppContext.BaseDirectory, "SuperMetroid.DesktopVerification.dll");
            return File.Exists(path) ? path : throw new FileNotFoundException("The verifier's output directory holds no verifier assembly.", path);
        }
    }
}
