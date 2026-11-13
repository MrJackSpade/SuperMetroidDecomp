using SuperMetroid.Core.Hardware;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Frontend.Intro;
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
    /// <summary>
    /// The image the scene leaves showing <paramref name="floor"/>, with the Zebes reveal loaded when
    /// <paramref name="zebesRevealed"/>; with <paramref name="clearedWithSamusAboard"/>, the image the final explosion
    /// leaves with Samus still aboard, which clears both views and so shows no floor.
    /// </summary>
    [WholeImageFixture("the Ceres destruction's memory image")]
    internal static byte[] Compose(ImportIntroCinematicArtwork artwork, CeresFloorShot? floor, bool zebesRevealed, bool clearedWithSamusAboard = false)
    {
        if (clearedWithSamusAboard && floor is not null)
            throw new ArgumentException("The explosion with Samus aboard leaves no floor shot.", nameof(floor));
        var vram = new SnesVram();
        var mode7 = new Mode7Vram(vram);
        var ceres = new CeresVram(vram);
        var views = new byte[CeresFloorShotDefinitions.MapCells];
        artwork.FlightCharacters.FloorMaps.Span.CopyTo(views);
        artwork.CeresDestructionMaps.Views.Span[..(views.Length - artwork.FlightCharacters.FloorMaps.Length)].CopyTo(views.AsSpan(artwork.FlightCharacters.FloorMaps.Length));
        if (floor is not null || clearedWithSamusAboard)
        {
            mode7.LoadCharacters(artwork.FlightCharacters.FloorCharacters.Span[..CeresFlightArtworkFileFormat.FloorByteCount]);
            mode7.FillMap(CeresFloorShotDefinitions.BlankTile, FloorSheetComposer.CellCount);
            mode7.LoadMap(views.AsSpan(CeresFloorShotDefinitions.StationStart, CeresFloorShotDefinitions.StationCells));
            if (clearedWithSamusAboard || floor is CeresFloorShot.Explosion or CeresFloorShot.ZebesApproach)
            {
                int upper = clearedWithSamusAboard ? CeresFloorShotDefinitions.BlankViewStart : CeresFloorShotDefinitions.FrontViewStart;
                mode7.LoadMap(views.AsSpan(upper, CeresFloorShotDefinitions.ViewCells));
                mode7.LoadMap(views.AsSpan(CeresFloorShotDefinitions.BlankViewStart, CeresFloorShotDefinitions.ViewCells),
                    cell: CeresFloorShotDefinitions.LowerViewFirstCell);
            }
            if (floor == CeresFloorShot.ZebesApproach)
                mode7.LoadMap(views.AsSpan(CeresFloorShotDefinitions.RearViewStart, CeresFloorShotDefinitions.ViewCells));
            ceres.ObjectCharacters.Load(CeresDestructionObjectPage.Characters(artwork, zebesRevealed));
        }
        if (zebesRevealed)
            ceres.ZebesTilemap.Load(artwork.ZebesReveal.Map.Bytes.Span[..ZebesRevealArtworkFormat.MapByteCount]);
        return vram.CaptureImage();
    }

    /// <summary>
    /// The image <paramref name="scene"/>'s state leaves over the import's artwork its bound catalog came from (#1616). A scene with Samus aboard that shows no
    /// floor after its final explosion has cleared it.
    /// </summary>
    [WholeImageFixture("the Ceres destruction's memory image")]
    internal static byte[] Compose(CeresDestructionCinematicState scene) =>
        Compose((scene.Artwork ?? throw new InvalidOperationException("Ceres destruction rendering requires installed cinematic artwork.")).ImportSource,
            scene.FloorShot, scene.ZebesRevealed, scene.FloorShot is null && scene.WithSamus && scene.Phase is CeresDestructionPhase.FlyingAwayFromExplosion
                or CeresDestructionPhase.HoldAfterExplosion or CeresDestructionPhase.FadeOutCeres or CeresDestructionPhase.Finished);
}
