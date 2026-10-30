using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// Composes the memory image the cartridge's Ceres destruction leaves for a scene state (#1358, #1467): the floor's
/// characters and the views its shot loads, the Zebes reveal's tilemap once loaded, and the OBJ characters the scene's
/// sprite page holds. Checks compare the installed floor pictures and the scene's planes against it.
/// </summary>
internal static class CeresDestructionScreenMemory
{
    /// <summary>The image the scene leaves showing <paramref name="floor"/>, with the Zebes reveal loaded when <paramref name="zebesRevealed"/>.</summary>
    [WholeImageFixture("the Ceres destruction's memory image")]
    internal static byte[] Compose(IntroCinematicArtwork artwork, CeresFloorShot? floor, bool zebesRevealed)
    {
        var vram = new SnesVram();
        var mode7 = new Mode7Vram(vram);
        var ceres = new CeresVram(vram);
        var views = new byte[CeresFloorShotDefinitions.MapCells];
        CeresFlightCharacters.Of(artwork.CeresFlight).FloorMaps.Span.CopyTo(views);
        IntroCinematicSourceArtwork.Of(artwork).CeresDestructionMaps.Views.Span[..(views.Length - CeresFlightCharacters.Of(artwork.CeresFlight).FloorMaps.Length)].CopyTo(views.AsSpan(CeresFlightCharacters.Of(artwork.CeresFlight).FloorMaps.Length));
        if (floor is { } shot)
        {
            mode7.LoadCharacters(artwork.CeresFlight.FloorCharacters.Span[..CeresFlightArtworkFormat.FloorByteCount]);
            mode7.FillMap(CeresFloorShotDefinitions.BlankTile, FloorSheetComposer.CellCount);
            mode7.LoadMap(views.AsSpan(CeresFloorShotDefinitions.StationStart, CeresFloorShotDefinitions.StationCells));
            if (shot is CeresFloorShot.Explosion or CeresFloorShot.ExplosionWithoutGunship or CeresFloorShot.ZebesApproach)
            {
                int upper = shot == CeresFloorShot.ExplosionWithoutGunship ? CeresFloorShotDefinitions.BlankViewStart : CeresFloorShotDefinitions.FrontViewStart;
                mode7.LoadMap(views.AsSpan(upper, CeresFloorShotDefinitions.ViewCells));
                mode7.LoadMap(views.AsSpan(CeresFloorShotDefinitions.BlankViewStart, CeresFloorShotDefinitions.ViewCells),
                    cell: CeresFloorShotDefinitions.LowerViewFirstCell);
            }
            if (shot == CeresFloorShot.ZebesApproach)
                mode7.LoadMap(views.AsSpan(CeresFloorShotDefinitions.RearViewStart, CeresFloorShotDefinitions.ViewCells));
            ceres.ObjectCharacters.Load(CeresDestructionObjectPage.Characters(artwork, zebesRevealed));
        }
        if (zebesRevealed)
            ceres.ZebesTilemap.Load(IntroCinematicSourceArtwork.Of(artwork).ZebesReveal.Map.Bytes.Span[..ZebesRevealArtworkFormat.MapByteCount]);
        return vram.CaptureImage();
    }

    /// <summary>The image <paramref name="scene"/>'s state leaves over its bound artwork.</summary>
    [WholeImageFixture("the Ceres destruction's memory image")]
    internal static byte[] Compose(CeresDestructionCinematicState scene) =>
        Compose(scene.Artwork ?? throw new InvalidOperationException("Ceres destruction rendering requires installed cinematic artwork."),
            scene.FloorShot, scene.ZebesRevealed);
}
