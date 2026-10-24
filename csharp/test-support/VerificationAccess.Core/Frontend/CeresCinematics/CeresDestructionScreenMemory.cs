using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// Replays the Ceres destruction scene's graphics transfers over the bound artwork into the memory image the backends
/// draw (#1358), and names the OBJ characters they leave as the scene's sprite page.
/// </summary>
[PpuLayoutBoundary("exact hardware test: the Ceres destruction transfers at their native words")]
internal static class CeresDestructionScreenMemory
{
    /// <summary>The image <paramref name="uploads"/> leave over <paramref name="artwork"/>, in order.</summary>
    internal static byte[] Compose(IntroCinematicArtworkCatalog artwork, IReadOnlyList<CeresDestructionUpload> uploads, bool withSamus)
    {
        var vram = new SnesVram();
        var mode7 = new Mode7Vram(vram);
        var ceres = new CeresVram(vram);
        byte[] maps = CeresDestructionScenery.Tilemaps(artwork);
        foreach (CeresDestructionUpload upload in uploads)
        {
            switch (upload)
            {
                case CeresDestructionUpload.CeresScene:
                    // C11B selects bytes $600-$BFF: the third/fourth 24-row Ceres map pair.
                    mode7.LoadCharacters(artwork.CeresFlight.FloorCharacters.Span[..CeresDestructionRomData.Transfers.FloorCharacterBytes]);
                    mode7.FillMap(CeresDestructionRomData.Transfers.InitialFloorMapCharacter, CeresDestructionRomData.Transfers.FloorMapCapacityBytes);
                    mode7.LoadMap(maps.AsSpan(CeresDestructionRomData.Transfers.CeresSceneTilemapOffset, CeresDestructionRomData.Transfers.CeresSceneTilemapBytes));
                    break;
                case CeresDestructionUpload.FinalExplosionMaps:
                    // State $22 shows the front gunship in the upper rows; state $25 has no gunship and clears them too.
                    int upperSource = withSamus ? CeresDestructionRomData.Transfers.ClearMapSourceOffset : 0;
                    mode7.LoadMap(maps.AsSpan(upperSource, CeresDestructionRomData.Transfers.MapHalfBytes));
                    mode7.LoadMap(maps.AsSpan(CeresDestructionRomData.Transfers.ClearMapSourceOffset, CeresDestructionRomData.Transfers.MapHalfBytes),
                        cell: CeresDestructionRomData.Transfers.ClearMapFirstCell);
                    break;
                case CeresDestructionUpload.ZebesReveal:
                    mode7.LoadMap(maps.AsSpan(CeresDestructionRomData.Transfers.MapHalfBytes, CeresDestructionRomData.Transfers.MapHalfBytes));
                    ceres.ZebesTilemap.Load(artwork.CeresDestruction.ZebesMap.Bytes.Span[..CeresDestructionRomData.Transfers.ZebesTilemapMinimumBytes]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(uploads), upload, "Undefined Ceres destruction upload.");
            }
        }
        // The OBJ characters are independent of the maps; extraction composes the scene's sprites over the same page.
        ceres.ObjectCharacters.Load(CeresDestructionObjectPage.Characters(artwork, uploads));
        return vram.CaptureImage();
    }

    /// <summary>The image <paramref name="scene"/>'s uploads leave over its bound artwork.</summary>
    internal static byte[] Compose(CeresDestructionCinematicState scene) =>
        Compose(scene.Artwork ?? throw new InvalidOperationException("Ceres destruction rendering requires installed cinematic artwork."),
            scene.Uploads, scene.WithSamus);
}
