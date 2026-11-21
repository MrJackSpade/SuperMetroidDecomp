namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The installed key of each <see cref="TorizoLookKind"/> (#2409, #2548): the colouring's name in its installed shades'
/// names, written out so renaming a colouring's identity never changes a name on disk. A health colouring's key is
/// followed by its band's.
/// </summary>
internal static partial class TorizoLookKindKeys
{
    /// <summary>Each TorizoLookKind member's installed key.</summary>
    internal static InstalledKeys<TorizoLookKind> Installed { get; } = new(new Dictionary<TorizoLookKind, string>
    {
        [TorizoLookKind.BombStatue] = "bombstatue",
        [TorizoLookKind.GoldenInitial] = "goldeninitial",
        [TorizoLookKind.Normal] = "normal",
        [TorizoLookKind.Golden] = "golden",
        [TorizoLookKind.White] = "white",
        [TorizoLookKind.Black] = "black",
        [TorizoLookKind.GoldenHealth] = "golden-health",
    });
}
