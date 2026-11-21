namespace SuperMetroid.Core.Input;

/// <summary>
/// The installed key of each <see cref="ControllerAction"/> (#2520, #2565), written out so renaming a member never changes
/// a name the installation, an installed document or a save spells it by.
/// </summary>
internal static class ControllerActionKeys
{
    /// <summary>Each ControllerAction member's installed key.</summary>
    internal static InstalledKeys<ControllerAction> Installed { get; } = new(new Dictionary<ControllerAction, string>
    {
        [ControllerAction.Shoot] = "Shoot",
        [ControllerAction.Jump] = "Jump",
        [ControllerAction.Dash] = "Dash",
        [ControllerAction.ItemSelect] = "ItemSelect",
        [ControllerAction.ItemCancel] = "ItemCancel",
        [ControllerAction.AimUp] = "AimUp",
        [ControllerAction.AimDown] = "AimDown",
    });
}
