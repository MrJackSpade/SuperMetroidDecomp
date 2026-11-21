namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// The installed key of each <see cref="SamusAnimationPlaceName"/> (#2520), written out so renaming a member never
/// changes a name the installation, an installed document or a save spells it by.
/// </summary>
internal static partial class SamusAnimationPlaceNameKeys
{
    /// <summary>Each SamusAnimationPlaceName member's installed key.</summary>
    internal static InstalledKeys<SamusAnimationPlaceName> Installed { get; } = new(new Dictionary<SamusAnimationPlaceName, string>
    {
        [SamusAnimationPlaceName.NeutralJumpRise] = "NeutralJumpRise",
        [SamusAnimationPlaceName.SpaceJumpSpinStart] = "SpaceJumpSpinStart",
        [SamusAnimationPlaceName.SpaceJumpSpinMiddle] = "SpaceJumpSpinMiddle",
        [SamusAnimationPlaceName.WallJumpOrdinarySpinStart] = "WallJumpOrdinarySpinStart",
        [SamusAnimationPlaceName.WallJumpSpaceJumpSpinStart] = "WallJumpSpaceJumpSpinStart",
        [SamusAnimationPlaceName.WallJumpScrewAttackSpinStart] = "WallJumpScrewAttackSpinStart",
        [SamusAnimationPlaceName.SpinJumpWallJumpReady] = "SpinJumpWallJumpReady",
        [SamusAnimationPlaceName.ScrewAttackWallJumpReady] = "ScrewAttackWallJumpReady",
        [SamusAnimationPlaceName.ScrewAttackSpinStart] = "ScrewAttackSpinStart",
        [SamusAnimationPlaceName.FastFallStart] = "FastFallStart",
        [SamusAnimationPlaceName.DrainedAbleToStandFirst] = "DrainedAbleToStandFirst",
        [SamusAnimationPlaceName.DrainedAbleToStand] = "DrainedAbleToStand",
        [SamusAnimationPlaceName.DrainedUnableToStandFirst] = "DrainedUnableToStandFirst",
        [SamusAnimationPlaceName.DrainedUnableToStandLast] = "DrainedUnableToStandLast",
        [SamusAnimationPlaceName.DrainedUnableToStand] = "DrainedUnableToStand",
        [SamusAnimationPlaceName.DrainedHyperBeamAcquisition] = "DrainedHyperBeamAcquisition",
        [SamusAnimationPlaceName.DrainedRainbowStop] = "DrainedRainbowStop",
        [SamusAnimationPlaceName.DrainedLanding] = "DrainedLanding",
        [SamusAnimationPlaceName.DrainedReleaseCrouching] = "DrainedReleaseCrouching",
        [SamusAnimationPlaceName.DrainedReleaseStanding] = "DrainedReleaseStanding",
        [SamusAnimationPlaceName.DrainedFallInitial] = "DrainedFallInitial",
        [SamusAnimationPlaceName.DrainedPutCrouching] = "DrainedPutCrouching",
        [SamusAnimationPlaceName.ScrewAttackWallContactRewind] = "ScrewAttackWallContactRewind",
        [SamusAnimationPlaceName.SpinJumpWallContactRewind] = "SpinJumpWallContactRewind",
    });
}
