using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies;

/// <summary>One member of a room's enemy graphics set, with where the set places its graphics and colours.</summary>
/// <param name="DefinitionId">The member's enemy identity, which selects its character and colour artwork.</param>
/// <param name="ColourStore">The shared sprite colour store the room loads the member's colours into.</param>
/// <param name="TileIndexBase">The sheet character the member's graphics start at, advancing by each earlier member's characters; it becomes the enemy's <see cref="RoomEnemySlot.TileIndexBase"/>.</param>
/// <param name="StagingOffset">Byte offset into the enemy tile area, following the previous member or at the record's fixed block.</param>
/// <param name="TileByteCount">The bytes of the member's tile set.</param>
public readonly record struct RoomEnemyGraphicsSetEntry(
    EnemyDefinitionId DefinitionId,
    SpriteColourStore ColourStore,
    int TileIndexBase,
    int StagingOffset,
    int TileByteCount);
