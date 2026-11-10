using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Runtime.Attract;

/// <summary>
/// The title demonstrations' inputs (#1300, #1554): the recording each plays and what its playback first watches for.
/// Creating an input does nothing until its playback first steps.
/// </summary>
public static class AttractDemoInputDefinitions
{
    /// <summary>The LandingSite title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractLandingSite { get; } =
        new(AttractDemoInputWatch.TitleLeaving, DemoInputRecordingId.AttractLandingSite);

    /// <summary>The PseudoScrewAttack title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractPseudoScrewAttack { get; } =
        new(AttractDemoInputWatch.TitleLeaving, DemoInputRecordingId.AttractPseudoScrewAttack);

    /// <summary>The SpeedBooster title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractSpeedBooster { get; } =
        new(AttractDemoInputWatch.TitleLeaving, DemoInputRecordingId.AttractSpeedBooster);

    /// <summary>The Dachora title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractDachora { get; } =
        new(AttractDemoInputWatch.TitleLeaving, DemoInputRecordingId.AttractDachora);

    /// <summary>The RedBrinstarElevator title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractRedBrinstarElevator { get; } =
        new(AttractDemoInputWatch.TitleLeaving, DemoInputRecordingId.AttractRedBrinstarElevator);

    /// <summary>The PrePhantoonHall title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractPrePhantoonHall { get; } =
        new(AttractDemoInputWatch.TitleLeaving, DemoInputRecordingId.AttractPrePhantoonHall);

    /// <summary>The LowerNorfairEntrance title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractLowerNorfairEntrance { get; } =
        new(AttractDemoInputWatch.TitleLeaving, DemoInputRecordingId.AttractLowerNorfairEntrance);

    /// <summary>The EyeDoor title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractEyeDoor { get; } =
        new(AttractDemoInputWatch.TitleLeaving, DemoInputRecordingId.AttractEyeDoor);

    /// <summary>The DiagonalShinespark title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractDiagonalShinespark { get; } =
        new(AttractDemoInputWatch.ShinesparkRelease, DemoInputRecordingId.AttractDiagonalShinespark);

    /// <summary>The MissileDoor title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractMissileDoor { get; } =
        new(AttractDemoInputWatch.TitleLeaving, DemoInputRecordingId.AttractMissileDoor);

    /// <summary>The Kraid title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractKraid { get; } =
        new(AttractDemoInputWatch.TitleLeaving, DemoInputRecordingId.AttractKraid);

    /// <summary>The FirefleaRoom title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractFirefleaRoom { get; } =
        new(AttractDemoInputWatch.TitleLeaving, DemoInputRecordingId.AttractFirefleaRoom);

    /// <summary>The ScrewAttack title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractScrewAttack { get; } =
        new(AttractDemoInputWatch.TitleLeaving, DemoInputRecordingId.AttractScrewAttack);

    /// <summary>The BrinstarDiagonalRoom title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractBrinstarDiagonalRoom { get; } =
        new(AttractDemoInputWatch.TitleLeaving, DemoInputRecordingId.AttractBrinstarDiagonalRoom);

    /// <summary>The PreSporeSpawnHall title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractPreSporeSpawnHall { get; } =
        new(AttractDemoInputWatch.TitleLeaving, DemoInputRecordingId.AttractPreSporeSpawnHall);

    /// <summary>The GrappleBeam title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractGrappleBeam { get; } =
        new(AttractDemoInputWatch.TitleLeaving, DemoInputRecordingId.AttractGrappleBeam);

    /// <summary>The IceBeam title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractIceBeam { get; } =
        new(AttractDemoInputWatch.TitleLeaving, DemoInputRecordingId.AttractIceBeam);

    /// <summary>The GauntletEntrance title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractGauntletEntrance { get; } =
        new(AttractDemoInputWatch.TitleLeaving, DemoInputRecordingId.AttractGauntletEntrance);

    /// <summary>The AdvancedGrappleBeam title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractAdvancedGrappleBeam { get; } =
        new(AttractDemoInputWatch.TitleLeaving, DemoInputRecordingId.AttractAdvancedGrappleBeam);

    /// <summary>The InfiniteBombJump title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractInfiniteBombJump { get; } =
        new(AttractDemoInputWatch.TitleLeaving, DemoInputRecordingId.AttractInfiniteBombJump);

    /// <summary>The SpecialBeamAttack title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractSpecialBeamAttack { get; } =
        new(AttractDemoInputWatch.TitleLeaving, DemoInputRecordingId.AttractSpecialBeamAttack);

    /// <summary>The TourianEntrance title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractTourianEntrance { get; } =
        new(AttractDemoInputWatch.TitleLeaving, DemoInputRecordingId.AttractTourianEntrance);

    /// <summary>The CrystalFlash title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractCrystalFlash { get; } =
        new(AttractDemoInputWatch.TitleLeaving, DemoInputRecordingId.AttractCrystalFlash);
}
