using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.TestSupport.VerificationOracles;

/// <summary>
/// The VRAM image an OBJ character page stands for: the page's characters at the OBJ character table and every other
/// word zero, so checks written against native VRAM keep comparing the same bytes at the same addresses.
/// </summary>
internal static class ObjCharacterVramImage
{
    /// <summary>The VRAM image of the page <paramref name="uploads"/> show as <paramref name="resolver"/> resolves them.</summary>
    /// <param name="uploads">The OBJ character table's uploads.</param>
    /// <param name="resolver">Resolves the uploads' named artwork.</param>
    [PpuLayoutBoundary("exact hardware test: places an OBJ page at the native OBJ character table")]
    internal static SnesVram Of(SpriteCharacters uploads, IObjCharacterSourceResolver resolver)
    {
        var vram = new SnesVram();
        vram.LoadBytes(GameplayVramLayoutConstants.ObjectCharacters.Offset * 2,
            new ObjCharacterTablePage().Page(uploads, resolver).Characters[..SpriteCharacters.TableByteCount]);
        return vram;
    }
}
