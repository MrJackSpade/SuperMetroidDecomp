
namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>One ordinary enemy's required installation identity and complete DMA source.</summary>
public readonly record struct EnemyTileSourceDefinition(EnemyDefinitionId DefinitionPointer, int SourceAddress, int ByteCount);
