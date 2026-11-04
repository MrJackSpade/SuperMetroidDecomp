using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Where a room graphics set's members land among the sprite characters, as the cartridge lays them out (#1513): each
/// member's tiles, where in the enemy tile area they upload to and the character index its pictures count from. The
/// import composes every sheet from this layout; the game draws each enemy from its own sheet and never sees it.
/// </summary>
internal sealed class GraphicsSetCharacterLayout
{
    /// <summary>The most members a room's enemy graphics can hold.</summary>
    internal const int MaximumMembers = 4;

    /// <summary>The bytes one fixed staging block spans.</summary>
    private const int StagingBlockByteCount = 512;

    /// <summary>The bytes one enemy tile holds; a member's tile index base advances one per tile.</summary>
    private const int TileByteCount = SpriteCharacterTableLayout.CharacterByteCount;

    /// <summary>The bytes of the enemy tile area: 320 tiles, through the end of the sprites' tile memory.</summary>
    private const int EnemyTileAreaByteCount = 320 * TileByteCount;

    /// <summary>Where the set's first member that follows the previous one stages its tiles: 64 tiles into the enemy tile area.</summary>
    private const int OrdinaryTilesStart = 64 * TileByteCount;

    /// <summary>The layout of <paramref name="set"/>.</summary>
    private GraphicsSetCharacterLayout(RoomEnemyGraphicsSetDefinition set) => Members = LayOut(set);

    /// <summary>The set's members in record order, each where the set lays its tiles out.</summary>
    internal IReadOnlyList<GraphicsSetMemberCharacters> Members { get; }

    /// <summary>The layout of graphics set <paramref name="set"/>.</summary>
    /// <exception cref="InvalidDataException">The set cannot be laid out (see <see cref="LayOut"/>).</exception>
    internal static GraphicsSetCharacterLayout Of(RoomEnemyGraphicsSetDefinition set) => new(Ensure.NotNull(set));

    /// <summary>
    /// The character index <paramref name="enemy"/>'s pictures count from under this set: its record's, or character zero
    /// for an enemy the set does not load, with the index its initialization copies from the enemy it follows, or fixes,
    /// in place of the set's where it does.
    /// </summary>
    internal int TileBaseOf(EnemyDefinitionId enemy)
    {
        int tileBase = LoadedTileBaseOf(enemy);
        if (EnemyCharacterBaseDefinitions.DrawsAtTilesOf.TryGetValue(enemy, out EnemyDefinitionId tileOwner))
            tileBase = LoadedTileBaseOf(tileOwner);
        return EnemyCharacterBaseDefinitions.Fixed.GetValueOrDefault(enemy, tileBase);
    }

    /// <summary>
    /// The character index the set's record for <paramref name="definitionId"/> gives, or character zero when the set
    /// does not load it, as the cartridge falls back for an actor without a graphics-set record.
    /// </summary>
    private int LoadedTileBaseOf(EnemyDefinitionId definitionId)
    {
        foreach (GraphicsSetMemberCharacters member in Members)
        {
            if (member.DefinitionId == definitionId)
                return member.TileIndexBase;
        }
        return 0;
    }

    /// <summary>
    /// The members <paramref name="set"/> loads, in record order: each enemy's tiles, where in the enemy tile area they
    /// upload to and the character index they start at.
    /// </summary>
    /// <remarks>
    /// A member shown at a fixed block ends every retail set. The game adds the header's fixed-block flag into the
    /// counters the following members would start from; no retail set has a following member, so that sum is never
    /// observed, and a set that would observe it is rejected rather than reproduced (#1412).
    /// </remarks>
    /// <exception cref="InvalidDataException">
    /// The set has more than four members, places a member after one shown at a fixed block, names an enemy whose tile set
    /// the import does not catalogue, or a member's tiles leave the enemy tile area.
    /// </exception>
    private static IReadOnlyList<GraphicsSetMemberCharacters> LayOut(RoomEnemyGraphicsSetDefinition set)
    {
        var members = new List<GraphicsSetMemberCharacters>();
        int nextEnemyTileIndex = 0;
        int nextStagingOffset = OrdinaryTilesStart;
        bool followsFixedBlock = false;

        foreach (RoomEnemyGraphicsSetMember source in set.Records.Span)
        {
            EnemyDefinitionId enemy = source.DefinitionId;
            if (members.Count == MaximumMembers)
                throw new InvalidDataException($"Enemy graphics set {set.Name} has more than the four members a room's enemy graphics can hold.");
            if (followsFixedBlock)
                throw new InvalidDataException($"Enemy graphics set {set.Name} places {enemy} after a member shown at a fixed block.");

            int byteCount = EnemyTileSetDefinitions.ByteCounts.TryGetValue(enemy, out int catalogued) ? catalogued
                : throw new InvalidDataException($"Enemy graphics set {set.Name} loads {enemy}, whose tile set the import does not catalogue.");
            bool atFixedBlock = EnemyTileSetDefinitions.FixedBlocks.TryGetValue(enemy, out int block);
            int stagingOffset = atFixedBlock ? block * StagingBlockByteCount : nextStagingOffset;
            if (stagingOffset + byteCount > EnemyTileAreaByteCount)
                throw new InvalidDataException($"Enemy {enemy}'s tiles would leave the enemy tile area: offset {stagingOffset}, size {byteCount}.");

            members.Add(new GraphicsSetMemberCharacters(enemy, nextEnemyTileIndex, stagingOffset, byteCount));
            followsFixedBlock = atFixedBlock;
            nextEnemyTileIndex += byteCount / TileByteCount;
            nextStagingOffset += byteCount;
        }
        return members;
    }
}
