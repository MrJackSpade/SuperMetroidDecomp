/// <summary>Compares captured .smframe sequences in software and on both Direct3D 11 devices.</summary>
internal static class RenderTools
{
    /// <summary>Runs the tool named by <paramref name="args"/>, or returns null when none matches.</summary>
    internal static int? Run(string[] args)
    {
        switch (args)
        {
            case ["--compare-sparse-sequence", var sparseDirectory]:
                SnapshotSequenceComparison.RunSparse(sparseDirectory);
                return 0;
            case ["--compare-sequence", var directory]:
                SnapshotSequenceComparison.Run(directory);
                return 0;
            default:
                return null;
        }
    }
}
