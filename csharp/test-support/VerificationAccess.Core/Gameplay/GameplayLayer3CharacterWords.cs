using SuperMetroid.AssetExtraction.Video;

namespace SuperMetroid.Core.Gameplay;

/// <summary>The native BG3 character base words of the layer-3 characters gameplay reads, which only verification reads (#1406).</summary>
internal static class GameplayLayer3CharacterWords
{
    /// <summary>The display state's BG3 character base as the cartridge's BG34NBA selects it.</summary>
    extension(GameplayDisplayState self)
    {
        /// <summary>The VRAM word of the layer-3 characters BG3 reads.</summary>
        internal int GameplayHudCharacterBaseWord => BgCharacterWordsConstants.WordOf(self.Layer3Characters, 0);
    }
}
