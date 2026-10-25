namespace SuperMetroid.Core.Input;

/// <summary>
/// The title demonstrations' demo-input objects (#1300): each object's pre-instruction and program. No object does
/// anything when it is created.
/// </summary>
public static class DemoInputObjectDefinitions
{
    /// <summary>The LandingSite title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractLandingSite { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractLandingSite);

    /// <summary>The PseudoScrewAttack title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractPseudoScrewAttack { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractPseudoScrewAttack);

    /// <summary>The SpeedBooster title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractSpeedBooster { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractSpeedBooster);

    /// <summary>The Dachora title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractDachora { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractDachora);

    /// <summary>The RedBrinstarElevator title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractRedBrinstarElevator { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractRedBrinstarElevator);

    /// <summary>The PrePhantoonHall title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractPrePhantoonHall { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractPrePhantoonHall);

    /// <summary>The LowerNorfairEntrance title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractLowerNorfairEntrance { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractLowerNorfairEntrance);

    /// <summary>The EyeDoor title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractEyeDoor { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractEyeDoor);

    /// <summary>The DiagonalShinespark title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractDiagonalShinespark { get; } =
        new(DemoInputPreInstruction.AttractShinespark, DemoInputProgram.AttractDiagonalShinespark);

    /// <summary>The MissileDoor title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractMissileDoor { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractMissileDoor);

    /// <summary>The Kraid title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractKraid { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractKraid);

    /// <summary>The FirefleaRoom title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractFirefleaRoom { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractFirefleaRoom);

    /// <summary>The ScrewAttack title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractScrewAttack { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractScrewAttack);

    /// <summary>The BrinstarDiagonalRoom title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractBrinstarDiagonalRoom { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractBrinstarDiagonalRoom);

    /// <summary>The PreSporeSpawnHall title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractPreSporeSpawnHall { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractPreSporeSpawnHall);

    /// <summary>The GrappleBeam title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractGrappleBeam { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractGrappleBeam);

    /// <summary>The IceBeam title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractIceBeam { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractIceBeam);

    /// <summary>The GauntletEntrance title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractGauntletEntrance { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractGauntletEntrance);

    /// <summary>The AdvancedGrappleBeam title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractAdvancedGrappleBeam { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractAdvancedGrappleBeam);

    /// <summary>The InfiniteBombJump title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractInfiniteBombJump { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractInfiniteBombJump);

    /// <summary>The SpecialBeamAttack title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractSpecialBeamAttack { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractSpecialBeamAttack);

    /// <summary>The TourianEntrance title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractTourianEntrance { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractTourianEntrance);

    /// <summary>The CrystalFlash title demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractCrystalFlash { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractCrystalFlash);
}
