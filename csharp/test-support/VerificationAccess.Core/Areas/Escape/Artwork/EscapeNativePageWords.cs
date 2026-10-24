using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>The VRAM words the cartridge copies the escape timer's and text's pages to, for checks against its native transfers.</summary>
[PpuLayoutBoundary("test fixture: the escape pages' native OBJ VRAM words")]
internal static class EscapeNativePageWords
{
    /// <summary>The VRAM words one four-bit OBJ character occupies.</summary>
    private const int CharacterWords = 16;

    /// <summary>The VRAM word the cartridge places OBJ character <paramref name="character"/> at.</summary>
    internal static ushort Of(int character) => checked((ushort)(GameplayVramLayoutConstants.ObjectCharacters.Offset + character * CharacterWords));

    /// <summary>The VRAM word the cartridge copies the first timer page to.</summary>
    internal static ushort TimerFirst => Of(EscapeTimerTileAtlasFormat.FirstCharacter);

    /// <summary>The VRAM word the cartridge copies the second timer page to.</summary>
    internal static ushort TimerSecond => Of(EscapeTimerTileAtlasFormat.SecondCharacter);
}
