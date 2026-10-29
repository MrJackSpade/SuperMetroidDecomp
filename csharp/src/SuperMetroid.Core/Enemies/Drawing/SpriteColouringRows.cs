using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Drawing;

/// <summary>
/// Places each sprite colouring in the sprite palette row that holds it under one loaded graphics set (#1464): a
/// member's colours in the row the set loads them into, a shared set in the row its role names, an encounter's colours
/// in the set of their colour run. It is the one place a colouring meets a row, until colour sets are keyed by owner;
/// gameplay compares colourings through it wherever the cartridge compares rows.
/// </summary>
internal sealed class SpriteColouringRows
{
    /// <summary>The graphics set whose loaded members' rows are placed.</summary>
    private readonly EnemyGraphicsSet _graphicsSet;

    /// <summary>Resolves colourings under <paramref name="graphicsSet"/>.</summary>
    internal SpriteColouringRows(EnemyGraphicsSet graphicsSet) => _graphicsSet = Ensure.NotNull(graphicsSet);

    /// <summary>The sprite palette row holding <paramref name="colouring"/>.</summary>
    /// <exception cref="InvalidOperationException">The colouring names a member the graphics set does not load.</exception>
    internal int Row(SpriteColouring colouring) =>
        Ensure.NotNull(colouring) is GraphicsSetMemberColouring member ? MemberRow(member.Enemy) : FixedRow(colouring);

    /// <summary>
    /// The row holding <paramref name="colouring"/> in every room: a shared set in the row its role names, an
    /// encounter's colours in the set of their colour run, an unnamed row in itself.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The colouring is a graphics-set member's, whose row depends on the set loaded.
    /// </exception>
    internal static int FixedRow(SpriteColouring colouring) => Ensure.NotNull(colouring) switch
    {
        SharedSpriteColouring shared => SharedRow(shared.Set),
        EncounterSpriteColouring encounter => encounter.Colours.Set,
        UnnamedSpriteRowColouring unnamed => unnamed.Row,
        GraphicsSetMemberColouring member => throw new InvalidOperationException(
            $"The colours of graphics-set member {member.Enemy} lie in the row the loaded set gives them."),
        _ => throw new InvalidOperationException($"Undefined sprite colouring {colouring}."),
    };

    /// <summary>
    /// Whether <paramref name="first"/> and <paramref name="second"/> are held in one row, as the cartridge's row
    /// comparisons decide.
    /// </summary>
    internal bool SameRow(SpriteColouring first, SpriteColouring second) => Row(first) == Row(second);

    /// <summary>
    /// The colours a draw in <paramref name="drawn"/> shows over an image coloured in sprite row
    /// <paramref name="imageRow"/>: none while that row holds them, otherwise the colouring and its row.
    /// </summary>
    internal DrawnColouring? InPlaceOf(SpriteColouring drawn, int imageRow)
    {
        int row = Row(drawn);
        return row == imageRow ? null : new DrawnColouring(drawn, row);
    }

    /// <summary>The row the graphics set loads <paramref name="enemy"/>'s colours into.</summary>
    /// <exception cref="InvalidOperationException">The set does not load the enemy.</exception>
    private int MemberRow(EnemyDefinitionId enemy)
    {
        foreach (RoomEnemyGraphicsSetEntry entry in _graphicsSet.Entries)
        {
            // Loading the entry checked its row is one of the eight sprite rows, so the cartridge's low-nibble
            // palette decode reads the row unchanged.
            if (entry.DefinitionId == enemy)
                return entry.PaletteRow;
        }
        throw new InvalidOperationException($"Graphics set {_graphicsSet.LoadedSet.Name} loads no colours for {enemy}.");
    }

    /// <summary>The row the room's shared set <paramref name="set"/> occupies.</summary>
    private static int SharedRow(SharedSpriteColours set) => set switch
    {
        SharedSpriteColours.Common => SpriteColourSetDefinitions.Common,
        SharedSpriteColours.EnemyProjectiles => SpriteColourSetDefinitions.EnemyProjectiles,
        SharedSpriteColours.Frozen => SpriteColourSetDefinitions.Frozen,
        SharedSpriteColours.DeadSidehopperFade => SpriteColourSetDefinitions.DeadSidehopper,
        SharedSpriteColours.NinjaSpacePirateGoldWall => SpriteColourSetDefinitions.NinjaSpacePirate,
        SharedSpriteColours.NinjaSpacePirateStance => SpriteColourSetDefinitions.NinjaSpacePirateStance,
        _ => throw new ArgumentOutOfRangeException(nameof(set), set, "Undefined shared sprite colour set."),
    };
}
