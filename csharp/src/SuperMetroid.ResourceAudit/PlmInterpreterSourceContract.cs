namespace SuperMetroid.ResourceAudit;

/// <summary>
/// Reviewed token fingerprints for record consumers and typed-owner routes.
/// Whitespace/comments are irrelevant. A behavioral edit invalidates its proof,
/// so the audit cannot keep succeeding with stale widths or exemptions.
/// </summary>
internal static class PlmInterpreterSourceContract
{
    internal static readonly IReadOnlyDictionary<string, string> Methods = new Dictionary<string, string>
    {
        ["TryIdentifyPermanentCollectible/3"] = "3AF5296E2EC554B6736764E7ABD9323557E34CA991464F8336E6C7214B737D4F",
        // #628 types PLM block coordinates and the layer-1 camera as spatial values; no program reads or operand widths change.
        ["DrawPlmInstruction/7"] = "2C892713DAF89E6312EB7AA6D0E81288260712F6784EC7FFA9262487B8DFACEF",
        // #1269 adds the compiled escape-passage and shaft-wall clear draws; no program reads.
        // #142 reads the installed visuals through auto-properties; no program reads.
        // #628 types PLM block coordinates and the layer-1 camera as spatial values; no program reads or operand widths change.
        ["DrawPlmInstruction/8"] = "4F2D88A249AD470383CADB738C44FD6B5AC3C45D8C467A0925C6B523F948C028",
        // #628 types PLM block coordinates and the layer-1 camera as spatial values; no program reads or operand widths change.
        ["ConvertEyeToBlueDoor/3"] = "CBAFD5F7484FA8FA379B53164C7D6DCEB17F1D45700378E88A036959DDC7F145",
        // #1269 adds $84:BB25, a two-byte record with no operands (PlmInstructionFormats).
        // #627 decodes the word into RoomPlmInstruction once; handlers take the typed instruction. No program reads or operand widths change.
        // #628 types PLM block coordinates and the layer-1 camera as spatial values; no program reads or operand widths change.
        ["ExecuteInstructionStream/8"] = "E3C9CAA89BC4200D3E53BEDE936E8691DB0B68BACD5419AD2CBE9073EE1D3182",
        // #1273 drops the verification-only instruction source; the remaining sources are unchanged.
        ["ReadProgramByte/2"] = "22D5078ACAA43B18D75BD7EF992352EB6F635BBC377C35E7EBDDBF42FCEB644E",
        ["ReadProgramWord/2"] = "C8D33F4B5EC309B3DBA0DC288A08F3DBF37522B91CDD00D3194ED7AE7BEF95C6",
        // Saved item-load counter increment adds no program reads or operand-width changes;
        // #1269 Chozo orb phases change collectible state only.
        ["SetupCollectibleSlot/8"] = "FC1B045565189F852BAD4D102346D3190B42D9B005542CC2B541AE89ABE0496A",
        // #1273 makes the door setups static; no reads or widths change.
        ["SetupColoredDoorSlot/6"] = "C878BE26BCD5544C3AC520C798FE826E3A023140B701E6E514ECADECF57B5715",
        // #627 selects the grey-door condition through GreyDoorConditions.AtTableIndex; no program reads change.
        ["SetupGreyDoorSlot/5"] = "A7996BE432E3613E6E5803F0CC795CE36CC6981CD909C5EF1ACD96D601854BC7",
        // Calculated scroll programs retain the same completion mutation.
        ["FinishScrollMutation/2"] = "56ED242C6D1033089B7D352FB6BD23A5F1F4425DA1C583962035AE345C819DB3",
        ["SetupScrollSlot/5"] = "90427E5947D2E103127996ADD0748C112C8F7139E365F6EF4AC20EEE7656AFDD",
        // #627 decodes station animation list words into StationAnimationList; no program reads change.
        ["SetupStation/6"] = "BCB7F88A91D291BBC12B18C2E2441E9B415F725B0595A1E7CB232EE63896B944",
        // #627 types the eye-door projectile definition as EyeDoorProjectileDefinition; no program reads change.
        ["SpawnEyeDoorProjectile/3"] = "0CFBE0699B18063775D047C16BD1F16F32C516BC7A9FBE158592A8C7C19CD2F3",
        ["SpawnEyeDoorProjectile/4"] = "2E19C737A5E889783FC733015EF4262F4B028B45BF9E24E76EA5FF91BD9634F5",
        // #627 typed handlers test membership, then throw on an unhandled member; no program reads or operand widths change.
        // #628 types PLM block coordinates and the layer-1 camera as spatial values; no program reads or operand widths change.
        ["TryExecuteBombTorizoHandInstruction/3"] = "A666786B67E88404DC86ED74284AFE118DC26042AB78B025DB79E6DD73B9AF2D",
        ["TryExecuteChozoStatueInstruction/4"] = "5372E7E6D95723C4EC346537A1F49DA015A16AB4B8FF8452336DA5D4E0CC979F",
        ["TryExecuteDraygonCannonInstruction/4"] = "260A37C2A8B309CE9DDDCF395BCE3A49B81DC158CBB6B1641ED9B06B757976D0",
        // #627 passes EyeDoorProjectileDefinition members of equal value; no program reads change.
        // #628 types PLM block coordinates and the layer-1 camera as spatial values; no program reads or operand widths change.
        ["TryExecuteEyeDoorInstruction/4"] = "A6DB1B4417364ABF328F79DDC34D6DE7BDF28BA01A42440A3009DB73EC7605CE",
        // #628 types PLM block coordinates and the layer-1 camera as spatial values; no program reads or operand widths change.
        ["TryExecuteMotherBrainGlassInstruction/3"] = "E466C19D1E39415E1BF82DB0E34E5B9AB84A29D7B79BD0D6E6CBE9D1B6D9EF33",
        // #627 re-pin: spawned projectile definitions became NoobTubeProjectileDefinition members of equal value; no program reads change.
        ["TryExecuteNoobTubeInstruction/3"] = "8D68636858EAF7CBD39F30AF108E157B9D3C6903EEF8B3DE012DECCC6F833986",
        // #142 drops the unused Samus accessor; no program reads or operand widths change.
        // #627 re-pin: PLM header aliases became PlmHeaderId members of equal value; routing unchanged.
        ["TryRunRoomPopulationSetup/10"] = "16FFB7D9F7852FAB1370230C9C3D9C97A1B11B816F23DAA07C669B603596914C",
        // #1255 suspends before the existing Empty draw/delete continuation; no new program reads or operand widths;
        // #1269 releases an emptied Chozo slot without reading its program.
        // #628 types PLM block coordinates and the layer-1 camera as spatial values; no program reads or operand widths change.
        ["TryStepCollectible/6"] = "4B4119293CF57BA02C13D99A609C2B86742CAC96579ADAD341ED5D48342B6C3D",
        // #628 types PLM block coordinates and the layer-1 camera as spatial values; no program reads or operand widths change.
        ["TryStepColoredDoor/6"] = "A1804CBE701F9A57CCAC0C674A6AF5D83D387947C4D0EB4F3A442CB80B8E9F93",
        // #1266 resets LoopTimer for a rejected shot; no program reads or operand widths change.
        // #628 types PLM block coordinates and the layer-1 camera as spatial values; no program reads or operand widths change.
        ["TryStepGreyDoor/8"] = "1E4F9BD5F4A1D1918DDFE1CB71DDE0F0FA0418FEE27E7FCEE8CBC01F211A0F44",
        ["TryStepScrollPlm/3"] = "3E7FD154C88BC9644F274C7B75B77C61B7F1CFC8452913C988415733FB168E2D",
        // #1269 station lock timing; the unpause-release change drops the second lock; no
        // program reads or operand widths change.
        // #628 types PLM block coordinates and the layer-1 camera as spatial values; no program reads or operand widths change.
        ["TryStepStation/6"] = "E505AED8773B391F24A255F0748524F2E277F6BB74669F9584E99D7CCF899FA3",
        ["TryStepWreckedShipTreadmill/3"] = "ED6CDE07E6C1DEF7DB2187EA0DD41010CFAA5FB9961A34EB61C57893E8486420",
    };
}
