using SuperMetroid.Core.Samus.Projectiles;
using F = SuperMetroid.Core.Samus.Projectiles.SamusProjectileFrameId;
using L = SuperMetroid.Core.Samus.Projectiles.ProjectileTrailPlacementList;
using T = SuperMetroid.Core.Samus.Projectiles.ProjectileTrailFamily;
using C = SuperMetroid.Core.Samus.Projectiles.ProjectileTrailCourse;

namespace SuperMetroid.Verification.Rendering;

/// <summary>
/// The presentation-quirk inventory of #2573 (#1423 R1): Core states what is shown, so the members that kept the
/// original's draw order, layer row offsets, drawing owners and display artifacts are gone (the compiler holds them gone:
/// nothing names them), and the trail placements that lagged their pictures show their own stage. The check reports every
/// lagging placement and passes only with none.
/// </summary>
internal static class PresentationQuirkInventoryVerification
{
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
        new L(T.UnchargedWaveIceSpazer, C.Up), new L(T.UnchargedWaveIceSpazer, C.UpRight), new L(T.UnchargedWaveIceSpazer, C.Right), new L(T.UnchargedWaveIceSpazer, C.DownRight),
        new L(T.UnchargedWaveIceSpazer, C.Down), new L(T.UnchargedWaveIceSpazer, C.DownLeft), new L(T.UnchargedWaveIceSpazer, C.Left), new L(T.UnchargedWaveIceSpazer, C.UpLeft),
    ];

    /// <summary>Checks that no trail placement lags its picture.</summary>
    internal static void VerifyPresentationQuirkInventory()
    {
        var findings = new List<string>();
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
