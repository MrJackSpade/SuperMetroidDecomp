using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

using SuperMetroid.AssetExtraction.Video;
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
    internal static SnesVram Of(SpriteArtworkPlacements uploads, IInstalledArtworkContent resolver)
    {
        var vram = new SnesVram();
        vram.LoadBytes(GameplayVramLayoutConstants.ObjectCharacters.Offset * 2,
            new ObjCharacterTablePage().Page(uploads, resolver).Characters[..SpriteArtworkPlacements.TableByteCount]);
        return vram;
    }
}
