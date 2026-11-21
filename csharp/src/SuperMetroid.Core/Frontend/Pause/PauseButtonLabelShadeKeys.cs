namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// The installed key of each <see cref="PauseButtonLabelShade"/> (#2520), written out so renaming a member never
/// changes a name the installation, an installed document or a save spells it by.
/// </summary>
internal static partial class PauseButtonLabelShadeKeys
{
    /// <summary>Each PauseButtonLabelShade member's installed key.</summary>
    internal static InstalledKeys<PauseButtonLabelShade> Installed { get; } = new(new Dictionary<PauseButtonLabelShade, string>
    {
        [PauseButtonLabelShade.Lit] = "Lit",
        [PauseButtonLabelShade.Dim] = "Dim",
    });
}
