namespace SuperMetroid.ResourceAudit;

/// <summary>
/// Reviewed token fingerprints for record consumers and typed-owner routes.
/// Whitespace/comments are irrelevant. A behavioral edit invalidates its proof,
/// so the audit cannot keep succeeding with stale widths or exemptions.
/// </summary>
internal static class PlmInterpreterSourceContract
{
    /// <summary>Gets the approved normalized-source fingerprints keyed by PLM interpreter method and arity.</summary>
    internal static readonly IReadOnlyDictionary<string, string> Methods = new Dictionary<string, string>
    {
        ["TryIdentifyPermanentCollectible/3"] = "6A6300F5B6F6DA8B5D1080735D67D69F395CEC3CE759F5B4FAD3DF8448BD9212",
        ["DrawPlmInstruction/8"] = "73FCC41B9A16992DC0D0D6850B0C46C5B195C2FFE93B78CFFE0039813E2CFD09",
        // #1269 adds the compiled escape-passage and shaft-wall clear draws; no program reads.
        ["DrawPlmInstruction/9"] = "860DFEC3F7FD370E1C5891C79F23F569EA0429E1A7DE7E23187CD44D44EE9986",
        ["ConvertEyeToBlueDoor/3"] = "C7280ECD02DA2D9EA7C5E36E10FBB3C2613829BE806663CA6E8DAD5E900BD929",
        // #1269 adds $84:BB25, a two-byte record with no operands (PlmInstructionFormats).
        ["ExecuteInstructionStream/9"] = "3806E413E6018A448104D24BD2659AA68D13301178E6D3C0C280E9A2702761CB",
        // #1273 drops the verification-only instruction source; the remaining sources are unchanged.
        ["ReadProgramByte/2"] = "22D5078ACAA43B18D75BD7EF992352EB6F635BBC377C35E7EBDDBF42FCEB644E",
        ["ReadProgramWord/2"] = "C8D33F4B5EC309B3DBA0DC288A08F3DBF37522B91CDD00D3194ED7AE7BEF95C6",
        // Saved item-load counter increment adds no program reads or operand-width changes;
        // #1269 Chozo orb phases change collectible state only.
        ["SetupCollectibleSlot/8"] = "FC1B045565189F852BAD4D102346D3190B42D9B005542CC2B541AE89ABE0496A",
        // #1273 makes the door setups static; no reads or widths change.
        ["SetupColoredDoorSlot/6"] = "C878BE26BCD5544C3AC520C798FE826E3A023140B701E6E514ECADECF57B5715",
        ["SetupGreyDoorSlot/5"] = "CDF84F67BF87B2752B3BC5A5799419EE54221D98D8672EAD0F9F20B4B1460D55",
        // Calculated scroll programs retain the same completion mutation.
        ["FinishScrollMutation/2"] = "56ED242C6D1033089B7D352FB6BD23A5F1F4425DA1C583962035AE345C819DB3",
        ["SetupScrollSlot/5"] = "307FC2CA13E00C1D0B3F178DD9820499BC88ADF4390FB3BA9ECD72F342D2AD9C",
        ["SetupStation/6"] = "4666424900C476A3E92A52EFA09DEF447C57A12278F7ACA95ACCF21824334035",
        ["SpawnEyeDoorProjectile/3"] = "528A979BBCEFD1FD759D8507E71C4D7B88503590E40577116BC0843C98FD0F7E",
        ["SpawnEyeDoorProjectile/4"] = "2CFF585079A6B3D8D6FB70A2C1C2DC00C74F46B7A5541F070CFB765AE6215232",
        ["TryExecuteBombTorizoHandInstruction/3"] = "316A4A4670F79C644405AAFA0439046E3D58E2C2FFF1C4D2CFAF4F6901E52C72",
        ["TryExecuteChozoStatueInstruction/4"] = "35C3960BD729C81C4B1A246125ED89D843701CB447583727706AF627F8B004A9",
        ["TryExecuteDraygonCannonInstruction/4"] = "54FD73CC670DE12E30842185942A9051B27C7F7DF1C7F58F0145192B2597FB88",
        ["TryExecuteEyeDoorInstruction/4"] = "5B2F9ACE369601FC1F8BCBBD28E85D064F7048EA835A3C0FB64FE592C3BC81C9",
        ["TryExecuteMotherBrainGlassInstruction/3"] = "0D2B48AF9CCBAF2656929520850ACDB252B4098C40141846456F8C6B36F00515",
        ["TryExecuteNoobTubeInstruction/3"] = "7CF84A4E7392E1E439C51C2C28177DA7D00C452F7C234FBE01E4841A702D1783",
        ["TryRunRoomPopulationSetup/11"] = "FDB4AC00475D5189549DFE4EC2B1E3E3E4F58BD86EABB30D5B75E28F25241E3B",
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
