using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>One ordinary enemy's required installation identity and complete DMA source.</summary>
public readonly record struct EnemyTileSourceDefinition(EnemyDefinitionId DefinitionPointer, int SourceAddress, int ByteCount);
