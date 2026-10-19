namespace SuperMetroid.Core.Input;

/// <summary>
/// The title demonstrations' bank-$91 demo-input objects (#1300): each object's pre-instruction and program. Every
/// object's initializer is the return-only <c>$91:83BF</c>.
/// </summary>
public static class DemoInputObjectDefinitions
{
    /// <summary><c>$91:9E52</c>: the LandingSite demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractLandingSite { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractLandingSite);

    /// <summary><c>$91:9E58</c>: the PseudoScrewAttack demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractPseudoScrewAttack { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractPseudoScrewAttack);

    /// <summary><c>$91:9E5E</c>: the SpeedBooster demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractSpeedBooster { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractSpeedBooster);

    /// <summary><c>$91:9E64</c>: the Dachora demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractDachora { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractDachora);

    /// <summary><c>$91:9E6A</c>: the RedBrinstarElevator demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractRedBrinstarElevator { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractRedBrinstarElevator);

    /// <summary><c>$91:9E70</c>: the PrePhantoonHall demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractPrePhantoonHall { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractPrePhantoonHall);

    /// <summary><c>$91:9E76</c>: the LowerNorfairEntrance demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractLowerNorfairEntrance { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractLowerNorfairEntrance);

    /// <summary><c>$91:9E7C</c>: the EyeDoor demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractEyeDoor { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractEyeDoor);

    /// <summary><c>$91:9E82</c>: the DiagonalShinespark demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractDiagonalShinespark { get; } =
        new(DemoInputPreInstruction.AttractShinespark, DemoInputProgram.AttractDiagonalShinespark);

    /// <summary><c>$91:9E88</c>: the MissileDoor demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractMissileDoor { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractMissileDoor);

    /// <summary><c>$91:9E8E</c>: the Kraid demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractKraid { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractKraid);

    /// <summary><c>$91:9E94</c>: the FirefleaRoom demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractFirefleaRoom { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractFirefleaRoom);

    /// <summary><c>$91:9E9A</c>: the ScrewAttack demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractScrewAttack { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractScrewAttack);

    /// <summary><c>$91:9EA0</c>: the BrinstarDiagonalRoom demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractBrinstarDiagonalRoom { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractBrinstarDiagonalRoom);

    /// <summary><c>$91:9EAC</c>: the PreSporeSpawnHall demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractPreSporeSpawnHall { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractPreSporeSpawnHall);

    /// <summary><c>$91:9EB2</c>: the GrappleBeam demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractGrappleBeam { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractGrappleBeam);

    /// <summary><c>$91:9EB8</c>: the IceBeam demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractIceBeam { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractIceBeam);

    /// <summary><c>$91:9EBE</c>: the GauntletEntrance demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractGauntletEntrance { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractGauntletEntrance);

    /// <summary><c>$91:9EC4</c>: the AdvancedGrappleBeam demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractAdvancedGrappleBeam { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractAdvancedGrappleBeam);

    /// <summary><c>$91:9ECA</c>: the InfiniteBombJump demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractInfiniteBombJump { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractInfiniteBombJump);

    /// <summary><c>$91:9ED0</c>: the SpecialBeamAttack demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractSpecialBeamAttack { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractSpecialBeamAttack);

    /// <summary><c>$91:9ED6</c>: the TourianEntrance demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractTourianEntrance { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractTourianEntrance);

    /// <summary><c>$91:9EDC</c>: the CrystalFlash demonstration's object.</summary>
    public static DemoInputObjectDefinition AttractCrystalFlash { get; } =
        new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractCrystalFlash);
}
