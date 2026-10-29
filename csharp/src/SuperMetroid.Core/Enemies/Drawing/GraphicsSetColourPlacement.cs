using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Drawing;

/// <summary>
/// Where a room's loaded graphics set places the sprite colourings drawn under it (#1464): each member's colours in the
/// set its record loads them into, a shared role's in the set the role names, an encounter's colours in the set of their
/// colour run. Gameplay asks it which colourings hold the same colours wherever the cartridge compares palette
/// selections, and draws resolve their colours through it.
/// </summary>
internal sealed class GraphicsSetColourPlacement : SpriteColourPlacement
{
    /// <summary>The graphics set whose loaded members are placed.</summary>
    private readonly EnemyGraphicsSet _graphicsSet;

    /// <summary>Places colourings under <paramref name="graphicsSet"/>.</summary>
    internal GraphicsSetColourPlacement(EnemyGraphicsSet graphicsSet) => _graphicsSet = Ensure.NotNull(graphicsSet);

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">The colouring names a member the graphics set does not load.</exception>
    internal override int HoldingSet(SpriteColouring colouring) =>
        Ensure.NotNull(colouring) is GraphicsSetMemberColouring member ? MemberSet(member.Enemy) : FixedSet(colouring);

    /// <summary>
    /// The set holding <paramref name="colouring"/> in every room: a shared role's in the set the role names, an
    /// encounter's colours in the set of their colour run.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The colouring is a graphics-set member's, whose set depends on the set loaded, or one no room places.
    /// </exception>
    internal static int FixedSet(SpriteColouring colouring) => Ensure.NotNull(colouring) switch
    {
        SharedSpriteColouring shared => SharedSet(shared.Set),
        EncounterSpriteColouring encounter => encounter.Colours.Set,
        GraphicsSetMemberColouring member => throw new InvalidOperationException(
            $"The colours of graphics-set member {member.Enemy} lie in the set the loaded graphics set gives them."),
        _ => throw new InvalidOperationException($"A room places no sprite colouring {colouring}."),
    };

    /// <summary>The set the graphics set loads <paramref name="enemy"/>'s colours into.</summary>
    /// <exception cref="InvalidOperationException">The set does not load the enemy.</exception>
    private int MemberSet(EnemyDefinitionId enemy)
    {
        foreach (RoomEnemyGraphicsSetEntry entry in _graphicsSet.Entries)
        {
            // Loading the entry checked its row is one of the eight sprite sets, so the cartridge's low-nibble palette
            // decode reads the set unchanged.
            if (entry.DefinitionId == enemy)
                return entry.PaletteRow;
        }
        throw new InvalidOperationException($"Graphics set {_graphicsSet.LoadedSet.Name} loads no colours for {enemy}.");
    }

    /// <summary>The set the room's shared role <paramref name="set"/> occupies.</summary>
    private static int SharedSet(SharedSpriteColours set) => set switch
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
