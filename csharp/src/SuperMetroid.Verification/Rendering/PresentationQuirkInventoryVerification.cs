using System.Reflection;
using SuperMetroid.Core.Samus.Projectiles;
using F = SuperMetroid.Core.Samus.Projectiles.SamusProjectileFrameId;
using L = SuperMetroid.Core.Samus.Projectiles.ProjectileTrailPlacementList;

namespace SuperMetroid.Verification.Rendering;

/// <summary>
/// The presentation-quirk inventory of #2573 (#1423 R1): Core states what is shown, so the members that kept the
/// original's draw order, layer row offsets, drawing owners and display artifacts are gone, and the trail placements that
/// lagged their pictures show their own stage. The check reports every member still present and every lagging placement,
/// and passes only with none.
/// </summary>
internal static class PresentationQuirkInventoryVerification
{
    /// <summary>
    /// The members and types the quirks lived in, by declaring type's full name and member name (a null member names the
    /// type itself), each with the quirk it kept.
    /// </summary>
    private static readonly (string Type, string? Member, string Quirk)[] Removed =
    [
        ("SuperMetroid.Core.Spatial.ScreenGeometry", "LayerRowsHiddenAboveScreen", "the hardware's one-row offset between layers and sprites"),
        ("SuperMetroid.Core.Rendering.GameplaySceneView", "LayerPixelAtScreenTop", "a layer shown one row below its camera"),
        ("SuperMetroid.Core.Rendering.GameplaySceneView", "LayerRowAtScreenTop", "a line scroll shown one row below its camera"),
        ("SuperMetroid.Core.Bosses.Crocomire.CrocomireMeltingDefinitions", "SecondPassBandCount", "the second melt's band read past its image"),
        ("SuperMetroid.Core.Bosses.Crocomire.CrocomireMeltingDefinitions", "FirstPassBandCount", "a melt image's bands counted per pass"),
        ("SuperMetroid.Core.Fx.FirefleaFxDefinitions", "SixthDeathFaintDarknessLevel", "a sixth Fireflea death's faint darkness"),
        ("SuperMetroid.Core.Rooms.Doors.DoorOpeningScrollDefinitions", "UpwardUpdatesBeforeScrollRecalculation", "an upward door's delayed layer scrolls"),
        ("SuperMetroid.Core.Rooms.Doors.DoorOpeningScrollState", "RecalculatesScrollAfterAdvance", "an upward door's delayed layer scrolls"),
        ("SuperMetroid.Core.Frontend.Intro.IntroSamusDisplay", "ActorsFirst", "Samus's layering named by the sprite order"),
        ("SuperMetroid.Core.Frontend.Intro.IntroSamusDisplay", "SamusFirst", "Samus's layering named by the sprite order"),
        ("SuperMetroid.Core.Frontend.Pause.PauseMenuState", "mapLabelsBeforeIcons", "the pause map's per-phase sprite order"),
        ("SuperMetroid.Core.Enemies.ICeresEncounter", "DrawCeresRidleyImmediateBabyAndDoor", "the Baby and door drawn where the sprite order put them"),
        ("SuperMetroid.Core.Enemies.ICeresEncounter", "DrawCeresRidleyBabyAndDoorOverEnemies", "the Baby and door drawn where the sprite order put them"),
        ("SuperMetroid.Core.Areas.Ceres.IDrawnByCeresRidleyScene", null, "Ridley's scene drawing his room's door"),
        ("SuperMetroid.Core.Areas.Ceres.ICeresRidleyScene", null, "Ridley's scene drawing his room's door"),
        ("SuperMetroid.Core.Areas.Ceres.CeresRidleyRoomDoorDrawing", null, "the drawing owner of Ridley's room's door"),
        ("SuperMetroid.Core.Areas.Ceres.CeresDoorCueKind", "HandDrawingToRidley", "the drawing owner of Ridley's room's door"),
        ("SuperMetroid.Core.Samus.Projectiles.ProjectileTrailPlacementDefinitions", "PictureInset", "a sparkle stored at its picture's corner"),
        ("SuperMetroid.Core.Samus.Grapple.SamusGrappleBeamDrawing", "RefreshFiringDrawOrigins", "a draw that rewrote the rope start"),
    ];

    /// <summary>
    /// The down-left Spazer-Wave frames whose trails lagged their pictures by a stage, each with the up frame of the stage
    /// it shows, whose placement every other direction gives that stage.
    /// </summary>
    private static readonly (F DownLeft, F Up)[] DownLeftStages =
    [
        (F.SpazerWaveDownLeftParting, F.SpazerWaveUpParting),
        (F.SpazerWaveDownLeftSpreading, F.SpazerWaveUpSpreading),
        (F.SpazerWaveDownLeftNearlySpread, F.SpazerWaveUpNearlySpread),
        (F.SpazerWaveDownLeftSpread, F.SpazerWaveUpSpread),
        (F.SpazerWaveDownLeftSpreadingWithoutDefaultTrail, F.SpazerWaveUpSpreading),
        (F.SpazerWaveDownLeftPartingWithoutDefaultTrail, F.SpazerWaveUpParting),
    ];

    /// <summary>The lists of the uncharged Wave, Ice and Spazer trails, one for each direction of travel.</summary>
    private static readonly L[] WaveIceSpazerLists =
    [
        L.UnchargedWaveIceSpazerUp, L.UnchargedWaveIceSpazerUpRight, L.UnchargedWaveIceSpazerRight, L.UnchargedWaveIceSpazerDownRight,
        L.UnchargedWaveIceSpazerDown, L.UnchargedWaveIceSpazerDownLeft, L.UnchargedWaveIceSpazerLeft, L.UnchargedWaveIceSpazerUpLeft,
    ];

    /// <summary>Checks that no listed quirk member remains in Core and that no trail placement lags its picture.</summary>
    internal static void VerifyPresentationQuirkInventory()
    {
        Assembly core = typeof(ProjectileTrailPlacements).Assembly;
        var findings = new List<string>();
        const BindingFlags members = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        foreach ((string typeName, string? member, string quirk) in Removed)
        {
            Type? type = core.GetType(typeName);
            if (member is null)
            {
                if (type is not null)
                    findings.Add($"{typeName} remains: {quirk}");
                continue;
            }
            if (type is not null && type.GetMember(member, members).Length != 0)
                findings.Add($"{typeName}.{member} remains: {quirk}");
        }
        foreach (L list in WaveIceSpazerLists)
        {
            foreach ((F downLeft, F up) in DownLeftStages)
            {
                ProjectileTrailPlacement? shown = ProjectileTrailPlacements.Start(list, downLeft);
                ProjectileTrailPlacement? stage = ProjectileTrailPlacements.Start(list, up);
                if (shown != stage)
                    findings.Add($"{list} places {downLeft}'s trail at {shown}, not at its stage's {stage}");
            }
        }
        if (findings.Count != 0)
            throw new InvalidOperationException($"{findings.Count} presentation quirk(s) remain in Core:{Environment.NewLine}{string.Join(Environment.NewLine, findings)}");
    }
}
