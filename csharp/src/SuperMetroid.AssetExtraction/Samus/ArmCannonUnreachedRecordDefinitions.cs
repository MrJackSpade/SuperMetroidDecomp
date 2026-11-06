namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The stock arm-cannon drawing descriptor of each pose-table record no gameplay path gives Samus (#1517). Those records
/// name no pose, but the installed drawing window keeps one descriptor per record, so the import decodes theirs by record
/// number.
/// </summary>
internal static class ArmCannonUnreachedRecordDefinitions
{
    /// <summary>Each unreached record's number and its stock descriptor.</summary>
    internal static readonly Dictionary<int, int> StockDescriptors = new()
    {
        [0x21] = ArmCannonDescriptorDefinitions.Default,
        [0x22] = ArmCannonDescriptorDefinitions.Default,
        [0x23] = ArmCannonDescriptorDefinitions.Default,
        [0x24] = ArmCannonDescriptorDefinitions.Default,
        [0x45] = ArmCannonDescriptorDefinitions.Default,
        [0x46] = ArmCannonDescriptorDefinitions.Default,
        [0x47] = ArmCannonDescriptorDefinitions.FacingRight,
        [0x48] = ArmCannonDescriptorDefinitions.FacingLeft,
        [0x5B] = ArmCannonDescriptorDefinitions.Default,
        [0x5C] = ArmCannonDescriptorDefinitions.Default,
        [0x5D] = ArmCannonDescriptorDefinitions.Default,
        [0x5E] = ArmCannonDescriptorDefinitions.Default,
        [0x5F] = ArmCannonDescriptorDefinitions.Default,
        [0x60] = ArmCannonDescriptorDefinitions.Default,
        [0x61] = ArmCannonDescriptorDefinitions.Default,
        [0x62] = ArmCannonDescriptorDefinitions.Default,
        [0xAD] = ArmCannonDescriptorDefinitions.FacingLeftNormalJumpNotMovingGunExt,
        [0xAF] = ArmCannonDescriptorDefinitions.FacingLeftNormalJumpAimingDown,
        [0xB1] = ArmCannonDescriptorDefinitions.FacingLeftNormalJumpAimingDownLeft,
        [0xC5] = ArmCannonDescriptorDefinitions.Default,
    };
}
