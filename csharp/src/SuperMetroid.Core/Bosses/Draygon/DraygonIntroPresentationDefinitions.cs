using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Native Draygon opening Evir graphic transfer at $A5:871B.</summary>
internal static class DraygonIntroPresentationDefinitions
{
    /// <summary>Transfer size $0600 bytes, the Evir enemy graphics-set sheet.</summary>
    internal const int EvirTilesByteCount = 0x0600;
    /// <summary>The OBJ character table byte the Evir tiles start at, $1A00: the start of the enemy tile area plus $200.</summary>
    private const int EvirTilesFirstByte = EnemyGraphicsSet.EnemyTileAreaFirstByte + 0x0200;
    /// <summary>The opening's Evir tiles over the OBJ characters.</summary>
    internal static RuntimeCharacterUpload EvirUpload { get; } = new(new EnemyTileCharacters(EnemyDefinitionId.Evir),
        EvirTilesFirstByte, EvirTilesByteCount);
}
