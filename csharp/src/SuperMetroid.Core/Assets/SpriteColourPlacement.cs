namespace SuperMetroid.Core.Assets;

/// <summary>
/// Where one load's shared sprite colour stores hold the colourings drawn under it (#1464): a room's loaded graphics set
/// places its members' colours, the shared roles' and its encounters', and a frontend screen places its colour groups'.
/// Actors and screens name colourings; this is the one place a colouring meets the store that holds it. Draws resolve
/// their colours through it, and gameplay decides through it which colourings hold the same colours, as the cartridge's
/// comparisons of palette selections decide.
/// </summary>
/// <remarks>
/// A placement is an immutable record of what one load decided, taken when the load happens, so a draw that carries one
/// keeps the placement current when it was published and reaches no live gameplay state. The stores stay eight shared
/// stores the colour writers address, and the cartridge shows a colouring placed in a store whatever that store last
/// held, whoever wrote it (the escape Climb's silver Ninja Space Pirate shows a store its room never loads). Placing a
/// colouring therefore still chooses a store; each load decides that here and nowhere else.
/// </remarks>
public abstract class SpriteColourPlacement
{
    /// <summary>The shared store holding <paramref name="colouring"/> under this load.</summary>
    /// <exception cref="InvalidOperationException">This load places no such colouring.</exception>
    internal abstract SpriteColourStore StoreOf(SpriteColouring colouring);

    /// <summary>Whether <paramref name="first"/> and <paramref name="second"/> are held in one store, so show the same colours.</summary>
    internal bool SharesColours(SpriteColouring first, SpriteColouring second) => StoreOf(first) == StoreOf(second);

    /// <summary>The colours <paramref name="colouring"/> holds among <paramref name="sets"/>, read and written in place.</summary>
    internal ColourRun ColoursOf(SpriteColouring colouring, IColourSets sets) => StoreOf(colouring).In(sets);

    /// <summary>
    /// The number the cartridge's palette selection of <paramref name="colouring"/> reads as, for an AI that reads the
    /// selection as a number rather than drawing in it.
    /// </summary>
    internal int SelectionNumber(SpriteColouring colouring) => StoreOf(colouring).Set;

    /// <summary>
    /// What a draw in <paramref name="drawn"/> shows over an image coloured in <paramref name="imageColouring"/>: none
    /// while the two hold the same colours, otherwise the drawn colouring as this load places it.
    /// </summary>
    internal DrawnColouring? InPlaceOf(SpriteColouring drawn, SpriteColouring imageColouring) =>
        SharesColours(drawn, imageColouring) ? null : new DrawnColouring(drawn, this);

    /// <summary>
    /// What a draw in <paramref name="drawn"/> shows over an image whose own colours are its colour group
    /// <paramref name="imageGroup"/>: none while that group's store holds the drawn colours, otherwise the drawn colouring
    /// as this load places it.
    /// </summary>
    internal DrawnColouring? InPlaceOfGroup(SpriteColouring drawn, int imageGroup) =>
        StoreOf(drawn).Set == imageGroup ? null : new DrawnColouring(drawn, this);

    /// <summary>
    /// The colours a draw in <paramref name="drawn"/> shows over its image's part group <paramref name="part"/> among
    /// <paramref name="sets"/>, and whether they blend with what lies behind them. A part that keeps its own colours shows
    /// its own group's store; a following part shows the store its group combines with the drawn colouring's into.
    /// </summary>
    internal ReadOnlySpan<RgbColor> ShownOver(SpriteColouring drawn, PartColourGroup part, ShownColourSets sets, out bool blends)
    {
        Ensure.NotNull(sets);
        SpriteColourStore shown = part.KeepsOwnColours ? part.Group.Store : Combined(part.Group, StoreOf(drawn));
        blends = shown.Blends;
        return sets.Set(shown.Set);
    }

    /// <summary>
    /// The store a following part of colour group <paramref name="partGroup"/> shows under a colouring held in
    /// <paramref name="drawn"/>. The cartridge selects it by ORing the colouring's palette bits into the part's own, so a
    /// part whose group shares bits with the colouring's store shows a third store rather than the colouring's; that
    /// combination is kept exactly.
    /// </summary>
    private static SpriteColourStore Combined(ImageColourGroup partGroup, SpriteColourStore drawn) =>
        new(partGroup.Number | drawn.Set);
}
