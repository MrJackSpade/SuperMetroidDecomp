using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Where the cartridge lays one graphics-set member's tiles out among the sprite characters (#1513): the import's
/// knowledge, from which it composes the member's sheets.
/// </summary>
/// <param name="Member">The record: an enemy, or artwork the set loads for no enemy (#1686).</param>
/// <param name="TileIndexBase">The sheet character the member's graphics start at, advancing by each earlier member's characters.</param>
/// <param name="StagingOffset">Byte offset into the enemy tile area, following the previous member or at the member's fixed block.</param>
/// <param name="TileByteCount">The bytes of the member's tile set.</param>
internal readonly record struct GraphicsSetMemberCharacters(
    RoomEnemyGraphicsSetMember Member,
    int TileIndexBase,
    int StagingOffset,
    int TileByteCount)
{
    /// <summary>The member enemy; null for a record loading artwork for no enemy.</summary>
    internal EnemyDefinitionId? DefinitionId => Member.DefinitionId;
}
