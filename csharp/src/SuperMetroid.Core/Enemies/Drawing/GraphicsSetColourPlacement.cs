using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Drawing;

/// <summary>
/// Where a room's loaded graphics set places the sprite colourings drawn under it (#1464): each member's colours in the
/// store its record loads them into, a shared role's in the store the role names, an encounter's colours in the store of
/// their colour run. Gameplay asks it which colourings hold the same colours wherever the cartridge compares palette
/// selections, and draws resolve their colours through it.
/// </summary>
/// <remarks>
/// It is taken when the graphics set loads and never changes after, so a published draw carrying it holds what that load
/// decided and none of the room's live enemy state; the next load takes a new one.
/// </remarks>
internal sealed class GraphicsSetColourPlacement : SpriteColourPlacement
{
    /// <summary>The name of the graphics set whose load this records, which a failed lookup names.</summary>
    private readonly string _loadedSetName;

    /// <summary>The store each member the set loads keeps its colours in.</summary>
    private readonly ImmutableDictionary<EnemyDefinitionId, SpriteColourStore> _memberStores;

    /// <summary>Records a load of <paramref name="loadedSetName"/> placing its members' colours in <paramref name="memberStores"/>.</summary>
    private GraphicsSetColourPlacement(string loadedSetName, ImmutableDictionary<EnemyDefinitionId, SpriteColourStore> memberStores)
    {
        _loadedSetName = loadedSetName;
        _memberStores = memberStores;
    }

    /// <summary>
    /// The placement a load of <paramref name="loadedSet"/> makes once it has loaded <paramref name="entries"/>, in load
    /// order. A member loaded twice keeps the store it was loaded into first, as the cartridge's search of the loaded
    /// records finds the first.
    /// </summary>
    internal static GraphicsSetColourPlacement Of(RoomEnemyGraphicsSetDefinition loadedSet, IEnumerable<RoomEnemyGraphicsSetEntry> entries)
    {
        Ensure.NotNull(loadedSet);
        var stores = ImmutableDictionary.CreateBuilder<EnemyDefinitionId, SpriteColourStore>();
        foreach (RoomEnemyGraphicsSetEntry entry in Ensure.NotNull(entries))
        {
            if (!stores.ContainsKey(entry.DefinitionId))
                stores.Add(entry.DefinitionId, entry.ColourStore);
        }
        return new GraphicsSetColourPlacement(loadedSet.Name, stores.ToImmutable());
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">The colouring names a member the graphics set does not load.</exception>
    internal override SpriteColourStore StoreOf(SpriteColouring colouring) =>
        Ensure.NotNull(colouring) is GraphicsSetMemberColouring member ? MemberStore(member.Enemy) : FixedStore(colouring);

    /// <summary>
    /// The store holding <paramref name="colouring"/> in every room: a shared role's in the store the role names, an
    /// encounter's colours in the store of their colour run.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The colouring is a graphics-set member's, whose store depends on the set loaded, or one no room places.
    /// </exception>
    internal static SpriteColourStore FixedStore(SpriteColouring colouring) => Ensure.NotNull(colouring) switch
    {
        SharedSpriteColouring shared => new SpriteColourStore(SharedSet(shared.Set)),
        EncounterSpriteColouring encounter => new SpriteColourStore(encounter.Colours.Set),
        GraphicsSetMemberColouring member => throw new InvalidOperationException(
            $"The colours of graphics-set member {member.Enemy} lie in the store the loaded graphics set gives them."),
        _ => throw new InvalidOperationException($"A room places no sprite colouring {colouring}."),
    };

    /// <summary>The store the graphics set loads <paramref name="enemy"/>'s colours into.</summary>
    /// <exception cref="InvalidOperationException">The set does not load the enemy.</exception>
    private SpriteColourStore MemberStore(EnemyDefinitionId enemy) =>
        _memberStores.TryGetValue(enemy, out SpriteColourStore store)
            ? store
            : throw new InvalidOperationException($"Graphics set {_loadedSetName} loads no colours for {enemy}.");

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
