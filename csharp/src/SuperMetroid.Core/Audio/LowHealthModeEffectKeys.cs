namespace SuperMetroid.Core.Audio;

/// <summary>
/// The installed key of each <see cref="LowHealthModeEffect"/> (#2520), written out so renaming a member never changes
/// a name the installation, an installed document or a save spells it by.
/// </summary>
internal static class LowHealthModeEffectKeys
{
    /// <summary>Each LowHealthModeEffect member's installed key.</summary>
    internal static InstalledKeys<LowHealthModeEffect> Installed { get; } = new(new Dictionary<LowHealthModeEffect, string>
    {
        [LowHealthModeEffect.Unchanged] = "Unchanged",
        [LowHealthModeEffect.Clear] = "Clear",
        [LowHealthModeEffect.Enter] = "Enter",
    });
}
