namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>
/// The Samus wireframe the equipment page shows (#2560): the suit it draws her in and the boots she wears, two
/// independent choices.
/// </summary>
/// <param name="Suit">The suit the wireframe draws.</param>
/// <param name="Boots">The boots the wireframe draws.</param>
public readonly record struct PauseWireframe(PauseWireframeSuit Suit, PauseWireframeBoots Boots)
{
    /// <summary>
    /// Every wireframe, in the order the game lists them: the Power Suit's with regular boots and
    /// with Hi-Jump Boots, then the Varia Suit's likewise.
    /// </summary>
    public static IReadOnlyList<PauseWireframe> All { get; } =
    [
        new(PauseWireframeSuit.PowerSuit, PauseWireframeBoots.RegularBoots),
        new(PauseWireframeSuit.PowerSuit, PauseWireframeBoots.HiJumpBoots),
        new(PauseWireframeSuit.VariaSuit, PauseWireframeBoots.RegularBoots),
        new(PauseWireframeSuit.VariaSuit, PauseWireframeBoots.HiJumpBoots),
    ];
}
