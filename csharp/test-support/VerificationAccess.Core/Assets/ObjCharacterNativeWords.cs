using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>The VRAM words the cartridge places OBJ characters at, for checks against its native transfers.</summary>
[PpuLayoutBoundary("test fixture: OBJ characters' native VRAM words")]
internal static class ObjCharacterNativeWords
{
    /// <summary>The VRAM words one four-bit OBJ character occupies.</summary>
    private const int CharacterWords = 16;

    /// <summary>The VRAM word the cartridge places OBJ character <paramref name="character"/> at.</summary>
    internal static ushort Of(int character) => checked((ushort)(GameplayVramLayoutConstants.ObjectCharacters.Offset + character * CharacterWords));
}
