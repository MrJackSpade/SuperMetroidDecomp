namespace SuperMetroid.Core.Input;

/// <summary>
/// The title demonstrations' demo-input objects (#1300): each object's pre-instruction and program. No object does
/// anything when it is created.
/// </summary>
public static class DemoInputObjectDefinitions
{
    /// <summary>The LandingSite title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractLandingSite { get; } =
        new(DemoInputFrameBehavior.AttractCheckLeave, DemoInputProgram.AttractLandingSite);

    /// <summary>The PseudoScrewAttack title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractPseudoScrewAttack { get; } =
        new(DemoInputFrameBehavior.AttractCheckLeave, DemoInputProgram.AttractPseudoScrewAttack);

    /// <summary>The SpeedBooster title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractSpeedBooster { get; } =
        new(DemoInputFrameBehavior.AttractCheckLeave, DemoInputProgram.AttractSpeedBooster);

    /// <summary>The Dachora title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractDachora { get; } =
        new(DemoInputFrameBehavior.AttractCheckLeave, DemoInputProgram.AttractDachora);

    /// <summary>The RedBrinstarElevator title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractRedBrinstarElevator { get; } =
        new(DemoInputFrameBehavior.AttractCheckLeave, DemoInputProgram.AttractRedBrinstarElevator);

    /// <summary>The PrePhantoonHall title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractPrePhantoonHall { get; } =
        new(DemoInputFrameBehavior.AttractCheckLeave, DemoInputProgram.AttractPrePhantoonHall);

    /// <summary>The LowerNorfairEntrance title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractLowerNorfairEntrance { get; } =
        new(DemoInputFrameBehavior.AttractCheckLeave, DemoInputProgram.AttractLowerNorfairEntrance);

    /// <summary>The EyeDoor title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractEyeDoor { get; } =
        new(DemoInputFrameBehavior.AttractCheckLeave, DemoInputProgram.AttractEyeDoor);

    /// <summary>The DiagonalShinespark title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractDiagonalShinespark { get; } =
        new(DemoInputFrameBehavior.AttractShinespark, DemoInputProgram.AttractDiagonalShinespark);

    /// <summary>The MissileDoor title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractMissileDoor { get; } =
        new(DemoInputFrameBehavior.AttractCheckLeave, DemoInputProgram.AttractMissileDoor);

    /// <summary>The Kraid title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractKraid { get; } =
        new(DemoInputFrameBehavior.AttractCheckLeave, DemoInputProgram.AttractKraid);

    /// <summary>The FirefleaRoom title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractFirefleaRoom { get; } =
        new(DemoInputFrameBehavior.AttractCheckLeave, DemoInputProgram.AttractFirefleaRoom);

    /// <summary>The ScrewAttack title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractScrewAttack { get; } =
        new(DemoInputFrameBehavior.AttractCheckLeave, DemoInputProgram.AttractScrewAttack);

    /// <summary>The BrinstarDiagonalRoom title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractBrinstarDiagonalRoom { get; } =
        new(DemoInputFrameBehavior.AttractCheckLeave, DemoInputProgram.AttractBrinstarDiagonalRoom);

    /// <summary>The PreSporeSpawnHall title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractPreSporeSpawnHall { get; } =
        new(DemoInputFrameBehavior.AttractCheckLeave, DemoInputProgram.AttractPreSporeSpawnHall);

    /// <summary>The GrappleBeam title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractGrappleBeam { get; } =
        new(DemoInputFrameBehavior.AttractCheckLeave, DemoInputProgram.AttractGrappleBeam);

    /// <summary>The IceBeam title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractIceBeam { get; } =
        new(DemoInputFrameBehavior.AttractCheckLeave, DemoInputProgram.AttractIceBeam);

    /// <summary>The GauntletEntrance title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractGauntletEntrance { get; } =
        new(DemoInputFrameBehavior.AttractCheckLeave, DemoInputProgram.AttractGauntletEntrance);

    /// <summary>The AdvancedGrappleBeam title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractAdvancedGrappleBeam { get; } =
        new(DemoInputFrameBehavior.AttractCheckLeave, DemoInputProgram.AttractAdvancedGrappleBeam);

    /// <summary>The InfiniteBombJump title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractInfiniteBombJump { get; } =
        new(DemoInputFrameBehavior.AttractCheckLeave, DemoInputProgram.AttractInfiniteBombJump);

    /// <summary>The SpecialBeamAttack title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractSpecialBeamAttack { get; } =
        new(DemoInputFrameBehavior.AttractCheckLeave, DemoInputProgram.AttractSpecialBeamAttack);

    /// <summary>The TourianEntrance title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractTourianEntrance { get; } =
        new(DemoInputFrameBehavior.AttractCheckLeave, DemoInputProgram.AttractTourianEntrance);

    /// <summary>The CrystalFlash title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractCrystalFlash { get; } =
        new(DemoInputFrameBehavior.AttractCheckLeave, DemoInputProgram.AttractCrystalFlash);
}
