namespace SuperMetroid.Core.Assets;

/// <summary>
/// Where one load's sprite colour sets hold the colourings drawn under it (#1464): a room's loaded graphics set places
/// its members' colours, the shared roles' and its encounters', and a frontend screen places its colour groups'. Actors
/// and screens name colourings; this is the one place a colouring meets the set that holds it. Draws resolve their
/// colours through it, and gameplay decides through it which colourings hold the same colours, as the cartridge's
/// comparisons of palette selections decide.
/// </summary>
/// <remarks>
/// The sprite sets stay eight stores the colour writers address, and the cartridge shows a colouring placed in a set
/// whatever that set last held, whoever wrote it (the escape Climb's silver Ninja Space Pirate shows a set its room never
/// loads). Placing a colouring therefore still chooses a store; each load decides that here and nowhere else.
/// </remarks>
public abstract class SpriteColourPlacement
{
    /// <summary>The sprite set holding <paramref name="colouring"/> under this load.</summary>
    /// <exception cref="InvalidOperationException">This load places no such colouring.</exception>
    internal abstract int HoldingSet(SpriteColouring colouring);

    /// <summary>Whether <paramref name="first"/> and <paramref name="second"/> are held in one set, so show the same colours.</summary>
    internal bool SharesColours(SpriteColouring first, SpriteColouring second) => HoldingSet(first) == HoldingSet(second);

    /// <summary>The colours <paramref name="colouring"/> holds among <paramref name="sets"/>, read and written in place.</summary>
    internal ColourRun ColoursOf(SpriteColouring colouring, IColourSets sets) => Ensure.NotNull(sets).Set(HoldingSet(colouring));

    /// <summary>
    /// The number the cartridge's palette selection of <paramref name="colouring"/> reads as, for an AI that reads the
    /// selection as a number rather than drawing in it.
    /// </summary>
    internal int SelectionNumber(SpriteColouring colouring) => HoldingSet(colouring);

    /// <summary>
    /// What a draw in <paramref name="drawn"/> shows over an image coloured in <paramref name="imageColouring"/>: none
    /// while the two hold the same colours, otherwise the drawn colouring as this load places it.
    /// </summary>
    internal DrawnColouring? InPlaceOf(SpriteColouring drawn, SpriteColouring imageColouring) =>
        SharesColours(drawn, imageColouring) ? null : new DrawnColouring(drawn, this);

    /// <summary>
    /// What a draw in <paramref name="drawn"/> shows over an image whose own colours are its colour group
    /// <paramref name="imageGroup"/>: none while that group's set holds the drawn colours, otherwise the drawn colouring
    /// as this load places it.
    /// </summary>
    internal DrawnColouring? InPlaceOfGroup(SpriteColouring drawn, int imageGroup) =>
        HoldingSet(drawn) == imageGroup ? null : new DrawnColouring(drawn, this);

    /// <summary>
    /// The colours a draw in <paramref name="drawn"/> shows over its image's part group <paramref name="partGroup"/> among
    /// <paramref name="sets"/>, and whether they blend with what lies behind them. A part that keeps its own colours shows
    /// its own set; any other shows the set the cartridge selects by ORing the drawn colouring's palette bits into the
    /// part's.
    /// </summary>
    internal ReadOnlySpan<RgbColor> ShownOver(SpriteColouring drawn, int partGroup, ShownColourSets sets, out bool blends)
    {
        Ensure.NotNull(sets);
        int set = (partGroup & SpriteImage.FixedPartRow) != 0
            ? partGroup & SpriteImage.MaximumPaletteRow
            : partGroup | HoldingSet(drawn);
        blends = DrawColours.SetBlends(set);
        return sets.Set(set);
    }
}
