
namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>One ordinary enemy's required installation identity and the bytes of its tile set.</summary>
/// <param name="DefinitionId">The enemy whose tile set the installation holds.</param>
/// <param name="ByteCount">The bytes of its tile set.</param>
public readonly record struct EnemyTileSourceDefinition(EnemyDefinitionId DefinitionId, int ByteCount);
