using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Runtime.Attract;

/// <summary>
/// The title demonstrations' inputs (#1300, #1554): the recording each plays, and the diagonal shinespark's continuation.
/// Creating an input does nothing until its playback first steps.
/// </summary>
public static class AttractDemoInputDefinitions
{
    /// <summary>The LandingSite title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractLandingSite { get; } =
        new(DemoInputRecordingId.AttractLandingSite);

    /// <summary>The PseudoScrewAttack title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractPseudoScrewAttack { get; } =
        new(DemoInputRecordingId.AttractPseudoScrewAttack);

    /// <summary>The SpeedBooster title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractSpeedBooster { get; } =
        new(DemoInputRecordingId.AttractSpeedBooster);

    /// <summary>The Dachora title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractDachora { get; } =
        new(DemoInputRecordingId.AttractDachora);

    /// <summary>The RedBrinstarElevator title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractRedBrinstarElevator { get; } =
        new(DemoInputRecordingId.AttractRedBrinstarElevator);

    /// <summary>The PrePhantoonHall title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractPrePhantoonHall { get; } =
        new(DemoInputRecordingId.AttractPrePhantoonHall);

    /// <summary>The LowerNorfairEntrance title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractLowerNorfairEntrance { get; } =
        new(DemoInputRecordingId.AttractLowerNorfairEntrance);

    /// <summary>The EyeDoor title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractEyeDoor { get; } =
        new(DemoInputRecordingId.AttractEyeDoor);

    /// <summary>
    /// The DiagonalShinespark title demonstration's input: the shinespark's launch, released into its continuation on the
    /// first step that plays.
    /// </summary>
    public static AttractDemoInputDefinition AttractDiagonalShinespark { get; } =
        new(DemoInputRecordingId.AttractDiagonalShinespark, Continuation: DemoInputRecordingId.AttractDiagonalShinesparkContinuation);

    /// <summary>The MissileDoor title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractMissileDoor { get; } =
        new(DemoInputRecordingId.AttractMissileDoor);

    /// <summary>The Kraid title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractKraid { get; } =
        new(DemoInputRecordingId.AttractKraid);

    /// <summary>The FirefleaRoom title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractFirefleaRoom { get; } =
        new(DemoInputRecordingId.AttractFirefleaRoom);

    /// <summary>The ScrewAttack title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractScrewAttack { get; } =
        new(DemoInputRecordingId.AttractScrewAttack);

    /// <summary>The BrinstarDiagonalRoom title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractBrinstarDiagonalRoom { get; } =
        new(DemoInputRecordingId.AttractBrinstarDiagonalRoom);

    /// <summary>The PreSporeSpawnHall title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractPreSporeSpawnHall { get; } =
        new(DemoInputRecordingId.AttractPreSporeSpawnHall);

    /// <summary>The GrappleBeam title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractGrappleBeam { get; } =
        new(DemoInputRecordingId.AttractGrappleBeam);

    /// <summary>The IceBeam title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractIceBeam { get; } =
        new(DemoInputRecordingId.AttractIceBeam);

    /// <summary>The GauntletEntrance title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractGauntletEntrance { get; } =
        new(DemoInputRecordingId.AttractGauntletEntrance);

    /// <summary>The AdvancedGrappleBeam title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractAdvancedGrappleBeam { get; } =
        new(DemoInputRecordingId.AttractAdvancedGrappleBeam);

    /// <summary>The InfiniteBombJump title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractInfiniteBombJump { get; } =
        new(DemoInputRecordingId.AttractInfiniteBombJump);

    /// <summary>The SpecialBeamAttack title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractSpecialBeamAttack { get; } =
        new(DemoInputRecordingId.AttractSpecialBeamAttack);

    /// <summary>The TourianEntrance title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractTourianEntrance { get; } =
        new(DemoInputRecordingId.AttractTourianEntrance);

    /// <summary>The CrystalFlash title demonstration's input.</summary>
    public static AttractDemoInputDefinition AttractCrystalFlash { get; } =
        new(DemoInputRecordingId.AttractCrystalFlash);
}
