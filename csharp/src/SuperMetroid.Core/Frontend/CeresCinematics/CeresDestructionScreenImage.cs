using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// Replays the Ceres destruction scene's graphics transfers over the bound artwork into the memory image the backends
/// draw (#1358), and names the OBJ characters they leave as the scene's sprite page.
/// </summary>
[PpuLayoutBoundary("codec: the Ceres destruction transfers at their native words")]
internal static class CeresDestructionScreenImage
{
    /// <summary>The image <paramref name="uploads"/> leave over <paramref name="artwork"/>, in order.</summary>
    internal static byte[] Compose(IntroCinematicArtworkCatalog artwork, IReadOnlyList<CeresDestructionUpload> uploads, bool withSamus)
    {
        var vram = new SnesVram();
        var mode7 = new Mode7Vram(vram);
        var ceres = new CeresVram(vram);
        byte[] maps = Tilemaps(artwork);
        foreach (CeresDestructionUpload upload in uploads)
        {
            switch (upload)
            {
                case CeresDestructionUpload.CeresScene:
                    // C11B selects bytes $600-$BFF: the third/fourth 24-row Ceres map pair.
                    mode7.LoadCharacters(artwork.CeresFlight.Mode7Characters.Span[..CeresDestructionRomData.Vram.Mode7CharacterBytes]);
                    mode7.FillMap(CeresDestructionRomData.Vram.InitialMode7MapCharacter, CeresDestructionRomData.Vram.Mode7MapCapacityBytes);
                    mode7.LoadMap(maps.AsSpan(CeresDestructionRomData.Vram.CeresSceneTilemapOffset, CeresDestructionRomData.Vram.CeresSceneTilemapBytes));
                    ceres.ObjectCharacters.Load(artwork.CeresFlight.ObjectCharacters.Span[..CeresDestructionRomData.Vram.Mode7CharacterBytes]);
                    // The final C11B DMA overwrites the first $1A00 OBJ bytes with the standard cinematic sheet at $9A:D200.
                    ceres.ObjectCharacters.Load(artwork.IntroObjectCharacters.Transfer.Span[..CeresDestructionRomData.Vram.SharedObjectCharacterBytes]);
                    break;
                case CeresDestructionUpload.FinalExplosionMaps:
                    // State $22 shows the front gunship in the upper rows; state $25 has no gunship and clears them too.
                    int upperSource = withSamus ? CeresDestructionRomData.Vram.ClearMapSourceOffset : 0;
                    mode7.LoadMap(maps.AsSpan(upperSource, CeresDestructionRomData.Vram.MapHalfBytes));
                    mode7.LoadMap(maps.AsSpan(CeresDestructionRomData.Vram.ClearMapSourceOffset, CeresDestructionRomData.Vram.MapHalfBytes),
                        cell: CeresDestructionRomData.Vram.ClearMapDestinationWord);
                    break;
                case CeresDestructionUpload.ZebesReveal:
                    mode7.LoadMap(maps.AsSpan(CeresDestructionRomData.Vram.MapHalfBytes, CeresDestructionRomData.Vram.MapHalfBytes));
                    ceres.ZebesTilemap.Load(artwork.CeresDestruction.ZebesMap.Transfer.Span[..CeresDestructionRomData.Vram.ZebesTilemapMinimumBytes]);
                    ceres.ObjectCharacters.Load(artwork.CeresDestruction.ZebesCharacters.Transfer.Span[..CeresDestructionRomData.Vram.Mode7CharacterBytes]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(uploads), upload, "Undefined Ceres destruction upload.");
            }
        }
        return vram.CaptureImage();
    }

    /// <summary>The scene's sprite page: the OBJ characters <paramref name="image"/> holds under OBSEL $03.</summary>
    internal static SpriteArtwork ObjectPage(ReadOnlySpan<byte> image) =>
        new(new SpriteArtworkId(SpriteArtworkKind.CeresDestructionObjects),
            image.Slice(CeresVramLayout.ObjectCharacters.Offset * sizeof(ushort), CeresVramLayout.ObjectCharacters.Count * sizeof(ushort)));

    /// <summary>The installed Ceres flight and destruction maps joined into the native contiguous transfer source.</summary>
    private static byte[] Tilemaps(IntroCinematicArtworkCatalog artwork)
    {
        var maps = new byte[CeresDestructionRomData.Vram.CeresMinimumTilemapBytes];
        artwork.CeresFlight.Mode7Maps.Span.CopyTo(maps);
        artwork.CeresDestruction.CeresMaps.Span.CopyTo(maps.AsSpan(artwork.CeresFlight.Mode7Maps.Length));
        return maps;
    }
}
