namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// The installed key of each <see cref="PauseButtonLabel"/> (#2520), written out so renaming a member never changes a
/// name the installation, an installed document or a save spells it by.
/// </summary>
internal static partial class PauseButtonLabelKeys
{
    /// <summary>Each PauseButtonLabel member's installed key.</summary>
    internal static InstalledKeys<PauseButtonLabel> Installed { get; } = new(new Dictionary<PauseButtonLabel, string>
    {
        [PauseButtonLabel.Map] = "Map",
        [PauseButtonLabel.Exit] = "Exit",
        [PauseButtonLabel.Samus] = "Samus",
    });
}
