using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Native Draygon opening Evir graphic transfer at $A5:871B.</summary>
internal static class DraygonIntroPresentationDefinitions
{
    /// <summary>Transfer size $0600 bytes, the Evir enemy graphics-set sheet.</summary>
    internal const int EvirTilesByteCount = 0x0600;
    /// <summary>The Evir tiles the opening shows.</summary>
    internal static ObjCharacterSource EvirTiles { get; } = new EnemyTileCharacters(EnemyDefinitionId.Evir);
}
