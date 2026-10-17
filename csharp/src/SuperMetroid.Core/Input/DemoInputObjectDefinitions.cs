namespace SuperMetroid.Core.Input;

/// <summary>
/// The title demonstrations' bank-$91 demo-input objects (#1300): each object's pre-instruction and program. Every
/// object's initializer is the return-only <c>$91:83BF</c>.
/// </summary>
public static class DemoInputObjectDefinitions
{
    /// <summary>A title demonstration's object, by its native identity (the scene's input object).</summary>
    public static DemoInputObjectDefinition StockAttract(ushort inputObject) => inputObject switch
    {
        // $91:9E52
        0x9e52 => new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractLandingSite),
        // $91:9E58
        0x9e58 => new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractPseudoScrewAttack),
        // $91:9E5E
        0x9e5e => new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractSpeedBooster),
        // $91:9E64
        0x9e64 => new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractDachora),
        // $91:9E6A
        0x9e6a => new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractRedBrinstarElevator),
        // $91:9E70
        0x9e70 => new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractPrePhantoonHall),
        // $91:9E76
        0x9e76 => new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractLowerNorfairEntrance),
        // $91:9E7C
        0x9e7c => new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractEyeDoor),
        // $91:9E82
        0x9e82 => new(DemoInputPreInstruction.AttractShinespark, DemoInputProgram.AttractDiagonalShinespark),
        // $91:9E88
        0x9e88 => new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractMissileDoor),
        // $91:9E8E
        0x9e8e => new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractKraid),
        // $91:9E94
        0x9e94 => new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractFirefleaRoom),
        // $91:9E9A
        0x9e9a => new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractScrewAttack),
        // $91:9EA0
        0x9ea0 => new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractBrinstarDiagonalRoom),
        // $91:9EAC
        0x9eac => new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractPreSporeSpawnHall),
        // $91:9EB2
        0x9eb2 => new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractGrappleBeam),
        // $91:9EB8
        0x9eb8 => new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractIceBeam),
        // $91:9EBE
        0x9ebe => new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractGauntletEntrance),
        // $91:9EC4
        0x9ec4 => new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractAdvancedGrappleBeam),
        // $91:9ECA
        0x9eca => new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractInfiniteBombJump),
        // $91:9ED0
        0x9ed0 => new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractSpecialBeamAttack),
        // $91:9ED6
        0x9ed6 => new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractTourianEntrance),
        // $91:9EDC
        0x9edc => new(DemoInputPreInstruction.AttractCheckLeave, DemoInputProgram.AttractCrystalFlash),
        _ => throw new InvalidDataException($"Unknown stock attract object $91:{inputObject:X4}."),
    };
}
