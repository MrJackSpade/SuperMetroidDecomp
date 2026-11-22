namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>
/// The installed key of each <see cref="PauseWireframe"/> (#2520, #2560): its suit's key and its boots' key, each
/// written out so renaming a member never changes a name the installation or an installed document spells it by.
/// </summary>
internal static partial class PauseWireframeKeys
{
    /// <summary>Each wireframe suit's installed key.</summary>
    internal static InstalledKeys<PauseWireframeSuit> Suits { get; } = new(new Dictionary<PauseWireframeSuit, string>
    {
        [PauseWireframeSuit.PowerSuit] = "PowerSuit",
        [PauseWireframeSuit.VariaSuit] = "VariaSuit",
    });

    /// <summary>Each wireframe boots' installed key.</summary>
    internal static InstalledKeys<PauseWireframeBoots> Boots { get; } = new(new Dictionary<PauseWireframeBoots, string>
    {
        [PauseWireframeBoots.RegularBoots] = "RegularBoots",
        [PauseWireframeBoots.HiJumpBoots] = "HiJumpBoots",
    });

    /// <summary>The installed key of <paramref name="wireframe"/>: its suit's key, a dot, and its boots' key.</summary>
    internal static string Of(PauseWireframe wireframe) => $"{Suits.Of(wireframe.Suit)}.{Boots.Of(wireframe.Boots)}";
}
