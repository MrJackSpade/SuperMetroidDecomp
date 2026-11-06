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
        [33] = ArmCannonDescriptorDefinitions.Default,
        [34] = ArmCannonDescriptorDefinitions.Default,
        [35] = ArmCannonDescriptorDefinitions.Default,
        [36] = ArmCannonDescriptorDefinitions.Default,
        [69] = ArmCannonDescriptorDefinitions.Default,
        [70] = ArmCannonDescriptorDefinitions.Default,
        [71] = ArmCannonDescriptorDefinitions.FacingRight,
        [72] = ArmCannonDescriptorDefinitions.FacingLeft,
        [91] = ArmCannonDescriptorDefinitions.Default,
        [92] = ArmCannonDescriptorDefinitions.Default,
        [93] = ArmCannonDescriptorDefinitions.Default,
        [94] = ArmCannonDescriptorDefinitions.Default,
        [95] = ArmCannonDescriptorDefinitions.Default,
        [96] = ArmCannonDescriptorDefinitions.Default,
        [97] = ArmCannonDescriptorDefinitions.Default,
        [98] = ArmCannonDescriptorDefinitions.Default,
        [173] = ArmCannonDescriptorDefinitions.FacingLeftNormalJumpNotMovingGunExt,
        [175] = ArmCannonDescriptorDefinitions.FacingLeftNormalJumpAimingDown,
        [177] = ArmCannonDescriptorDefinitions.FacingLeftNormalJumpAimingDownLeft,
        [197] = ArmCannonDescriptorDefinitions.Default,
    };
}
