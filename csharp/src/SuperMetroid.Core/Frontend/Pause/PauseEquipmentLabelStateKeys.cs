namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// The installed key of each <see cref="PauseEquipmentLabelState"/> (#2520), written out so renaming a member never
/// changes a name the installation, an installed document or a save spells it by.
/// </summary>
internal static partial class PauseEquipmentLabelStateKeys
{
    /// <summary>Each PauseEquipmentLabelState member's installed key.</summary>
    internal static InstalledKeys<PauseEquipmentLabelState> Installed { get; } = new(new Dictionary<PauseEquipmentLabelState, string>
    {
        [PauseEquipmentLabelState.Blank] = "Blank",
        [PauseEquipmentLabelState.Enabled] = "Enabled",
        [PauseEquipmentLabelState.Disabled] = "Disabled",
    });
}
