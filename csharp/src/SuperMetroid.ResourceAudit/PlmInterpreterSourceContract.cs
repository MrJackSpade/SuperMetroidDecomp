namespace SuperMetroid.ResourceAudit;

/// <summary>
/// The PLM interpreter methods that consume records or route typed owners. A guarded
/// method missing from this set, or a listed method that no longer exists, is a finding.
/// </summary>
internal static class PlmInterpreterSourceContract
{
    internal static readonly IReadOnlySet<string> Methods = new HashSet<string>
    {
        "TryIdentifyPermanentCollectible/3",
        // #628 types PLM block coordinates and the layer-1 camera as spatial values; no program reads or operand widths change.
        "DrawPlmInstruction/7",
        // #1269 adds the compiled escape-passage and shaft-wall clear draws; no program reads.
        // #142 reads the installed visuals through auto-properties; no program reads.
        // #628 types PLM block coordinates and the layer-1 camera as spatial values; no program reads or operand widths change.
        "DrawPlmInstruction/6",
        // #628 types PLM block coordinates and the layer-1 camera as spatial values; no program reads or operand widths change.
        "ConvertEyeToBlueDoor/3",
        // #1269 adds $84:BB25, a two-byte record with no operands (PlmInstructionFormats).
        // #627 decodes the word into RoomPlmInstruction once; handlers take the typed instruction. No program reads or operand widths change.
        // #628 $84:AB51 names its two scroll cells as typed screen positions; same writes, no program reads.
        // #628 $84:AB51 names its two scroll cells as typed screen positions; same writes, no program reads.
        // #628 types PLM block coordinates and the layer-1 camera as spatial values; no program reads or operand widths change.
        "ExecuteInstructionStream/7",
        // #1273 drops the verification-only instruction source; the remaining sources are unchanged.
        "ReadProgramByte/2",
        "ReadProgramWord/2",
        // Saved item-load counter increment adds no program reads or operand-width changes;
        // #1269 Chozo orb phases change collectible state only.
        "SetupCollectibleSlot/5",
        // #1273 makes the door setups static; no reads or widths change.
        "SetupColoredDoorSlot/6",
        // #627 selects the grey-door condition through GreyDoorConditions.AtTableIndex; no program reads change.
        "SetupGreyDoorSlot/5",
        // Calculated scroll programs retain the same completion mutation.
        "FinishScrollMutation/2",
        "SetupScrollSlot/5",
        // #627 decodes station animation list words into StationAnimationList; no program reads change.
        "SetupStation/6",
        // #627 types the eye-door projectile definition as EyeDoorProjectileDefinition; no program reads change.
        "SpawnEyeDoorProjectile/3",
        "SpawnEyeDoorProjectile/4",
        // #627 typed handlers test membership, then throw on an unhandled member; no program reads or operand widths change.
        // #628 types PLM block coordinates and the layer-1 camera as spatial values; no program reads or operand widths change.
        "TryExecuteBombTorizoHandInstruction/3",
        "TryExecuteChozoStatueInstruction/4",
        "TryExecuteDraygonCannonInstruction/4",
        // #627 passes EyeDoorProjectileDefinition members of equal value; no program reads change.
        // #628 types PLM block coordinates and the layer-1 camera as spatial values; no program reads or operand widths change.
        "TryExecuteEyeDoorInstruction/4",
        // #628 types PLM block coordinates and the layer-1 camera as spatial values; no program reads or operand widths change.
        "TryExecuteMotherBrainGlassInstruction/3",
        "TryExecuteNoobTubeInstruction/3",
        // #142 drops the unused Samus accessor; no program reads or operand widths change.
        "TryRunRoomPopulationSetup/5",
        // #1255 suspends before the existing Empty draw/delete continuation; no new program reads or operand widths;
        // #1269 releases an emptied Chozo slot without reading its program.
        // #628 types PLM block coordinates and the layer-1 camera as spatial values; no program reads or operand widths change.
        "TryStepCollectible/5",
        // #628 types PLM block coordinates and the layer-1 camera as spatial values; no program reads or operand widths change.
        "TryStepColoredDoor/5",
        // #1266 resets LoopTimer for a rejected shot; no program reads or operand widths change.
        // #628 types PLM block coordinates and the layer-1 camera as spatial values; no program reads or operand widths change.
        "TryStepGreyDoor/7",
        "TryStepScrollPlm/3",
        // #1269 station lock timing; the unpause-release change drops the second lock; no
        // program reads or operand widths change.
        // #628 types PLM block coordinates and the layer-1 camera as spatial values; no program reads or operand widths change.
        "TryStepStation/5",
        "TryStepWreckedShipTreadmill/3",
    };
}
