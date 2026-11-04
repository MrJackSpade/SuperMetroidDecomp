using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Where the cartridge lays one graphics-set member's tiles out among the sprite characters (#1513): the import's
/// knowledge, from which it composes the member's sheets.
/// </summary>
/// <param name="DefinitionId">The member enemy.</param>
/// <param name="TileIndexBase">The sheet character the member's graphics start at, advancing by each earlier member's characters.</param>
/// <param name="StagingOffset">Byte offset into the enemy tile area, following the previous member or at the member's fixed block.</param>
/// <param name="TileByteCount">The bytes of the member's tile set.</param>
internal readonly record struct GraphicsSetMemberCharacters(
    EnemyDefinitionId DefinitionId,
    int TileIndexBase,
    int StagingOffset,
    int TileByteCount);
