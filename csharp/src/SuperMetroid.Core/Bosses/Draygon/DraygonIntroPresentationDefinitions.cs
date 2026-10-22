using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Native Draygon opening Evir graphic transfer at $A5:871B.</summary>
internal static class DraygonIntroPresentationDefinitions
{
    /// <summary>Transfer size $0600 bytes, the Evir enemy graphics-set sheet.</summary>
    internal const int EvirTilesByteCount = 0x0600;
    /// <summary>VRAM word $6D00 expressed as the byte address accepted by SnesVram.</summary>
    internal const int EvirTilesVramByteAddress = 0xda00;
    /// <summary>The opening's upload of the Evir tiles over the OBJ characters.</summary>
    internal static RuntimeCharacterUpload EvirUpload { get; } = RuntimeCharacterUpload.To(BossVram.DraygonEvirTilesDma,
        EvirTilesByteCount, new EnemyTileCharacters(EnemyDefinitionId.Evir));
}
