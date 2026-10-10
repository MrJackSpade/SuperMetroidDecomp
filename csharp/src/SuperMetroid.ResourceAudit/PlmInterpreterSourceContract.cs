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
        ["DrawPlmInstruction/8"] = "73FCC41B9A16992DC0D0D6850B0C46C5B195C2FFE93B78CFFE0039813E2CFD09",
        // #1269 adds the compiled escape-passage and shaft-wall clear draws; no program reads.
        // #142 reads the installed visuals through auto-properties; no program reads.
        ["DrawPlmInstruction/9"] = "B0207746653551A75D5E292F9657536D9A9C6AC40795BBFE0CEC752F6FB69EA0",
        ["ConvertEyeToBlueDoor/3"] = "C7280ECD02DA2D9EA7C5E36E10FBB3C2613829BE806663CA6E8DAD5E900BD929",
        // #1269 adds $84:BB25, a two-byte record with no operands (PlmInstructionFormats).
        // #627 decodes the word into RoomPlmInstruction once; handlers take the typed instruction. No program reads or operand widths change.
        ["ExecuteInstructionStream/9"] = "59A75859D9B638CD91518A5A3A259F3DB73634744FDC069155854E995443A0A4",
        // #1273 drops the verification-only instruction source; the remaining sources are unchanged.
        ["ReadProgramByte/2"] = "22D5078ACAA43B18D75BD7EF992352EB6F635BBC377C35E7EBDDBF42FCEB644E",
        ["ReadProgramWord/2"] = "C8D33F4B5EC309B3DBA0DC288A08F3DBF37522B91CDD00D3194ED7AE7BEF95C6",
        // Saved item-load counter increment adds no program reads or operand-width changes;
        // #1269 Chozo orb phases change collectible state only.
        ["SetupCollectibleSlot/8"] = "FC1B045565189F852BAD4D102346D3190B42D9B005542CC2B541AE89ABE0496A",
        // #1273 makes the door setups static; no reads or widths change.
        ["SetupColoredDoorSlot/6"] = "C878BE26BCD5544C3AC520C798FE826E3A023140B701E6E514ECADECF57B5715",
        ["SetupGreyDoorSlot/5"] = "EBF4AA11BF6DFC350FE23A8DE13C007A4D4FFE8131C8E83E513D373BB553E8A7",
        // Calculated scroll programs retain the same completion mutation.
        ["FinishScrollMutation/2"] = "56ED242C6D1033089B7D352FB6BD23A5F1F4425DA1C583962035AE345C819DB3",
        ["SetupScrollSlot/5"] = "90427E5947D2E103127996ADD0748C112C8F7139E365F6EF4AC20EEE7656AFDD",
        ["SetupStation/6"] = "4666424900C476A3E92A52EFA09DEF447C57A12278F7ACA95ACCF21824334035",
        ["SpawnEyeDoorProjectile/3"] = "528A979BBCEFD1FD759D8507E71C4D7B88503590E40577116BC0843C98FD0F7E",
        ["SpawnEyeDoorProjectile/4"] = "2CFF585079A6B3D8D6FB70A2C1C2DC00C74F46B7A5541F070CFB765AE6215232",
        // #627 typed handlers test membership, then throw on an unhandled member; no program reads or operand widths change.
        ["TryExecuteBombTorizoHandInstruction/3"] = "38039FAFA28040DB31AC5E080842B0146B50DB494C750E87B7341D11FCD1ECE2",
        ["TryExecuteChozoStatueInstruction/4"] = "5372E7E6D95723C4EC346537A1F49DA015A16AB4B8FF8452336DA5D4E0CC979F",
        ["TryExecuteDraygonCannonInstruction/4"] = "260A37C2A8B309CE9DDDCF395BCE3A49B81DC158CBB6B1641ED9B06B757976D0",
        ["TryExecuteEyeDoorInstruction/4"] = "E4C0169DC63624F56434E356DE9A9DC96303343C5AD3B16F4012537CD0B038B0",
        ["TryExecuteMotherBrainGlassInstruction/3"] = "8B7C27BE9344830E619C0AB4336C5D248C539753B0C9356DB15AEB2518FCA29A",
        ["TryExecuteNoobTubeInstruction/3"] = "5F1BFD668AF959AF88DA99DEFA7698F85D03CD00E80CCF716F9129F8A930A10D",
        // #142 drops the unused Samus accessor; no program reads or operand widths change.
        // #627 re-pin: PLM header aliases became PlmHeaderId members of equal value; routing unchanged.
        ["TryRunRoomPopulationSetup/10"] = "16FFB7D9F7852FAB1370230C9C3D9C97A1B11B816F23DAA07C669B603596914C",
        // #1255 suspends before the existing Empty draw/delete continuation; no new program reads or operand widths;
        // #1269 releases an emptied Chozo slot without reading its program.
        ["TryStepCollectible/7"] = "E0E560003D363D333D84DF81E009EA6764A487CB0BAC8D58E4B30BB59E37AF28",
        ["TryStepColoredDoor/7"] = "14CD1F847751E3E06E51CB8F2FC0FC374F56A36CDBFCFDCFF6D241D95637FE50",
        // #1266 resets LoopTimer for a rejected shot; no program reads or operand widths change.
        ["TryStepGreyDoor/9"] = "1D39DE376A24C293CBCEA04F7BD7B26563277F20143E268129C5F7067D86BCD8",
        ["TryStepScrollPlm/3"] = "3E7FD154C88BC9644F274C7B75B77C61B7F1CFC8452913C988415733FB168E2D",
        // #1269 station lock timing; the unpause-release change drops the second lock; no
        // program reads or operand widths change.
        ["TryStepStation/7"] = "8CED1EABD72AF7F2C5B7269DC3A57EDD3AECF55485BC4DB1E93C2AC1BB030B0F",
        ["TryStepWreckedShipTreadmill/3"] = "ED6CDE07E6C1DEF7DB2187EA0DD41010CFAA5FB9961A34EB61C57893E8486420",
    };
}
