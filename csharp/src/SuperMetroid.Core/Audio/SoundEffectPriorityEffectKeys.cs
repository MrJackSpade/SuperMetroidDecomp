namespace SuperMetroid.Core.Audio;

/// <summary>
/// The installed key of each <see cref="SoundEffectPriorityEffect"/> (#2520), written out so renaming a member never
/// changes a name the installation, an installed document or a save spells it by.
/// </summary>
internal static class SoundEffectPriorityEffectKeys
{
    /// <summary>Each SoundEffectPriorityEffect member's installed key.</summary>
    internal static InstalledKeys<SoundEffectPriorityEffect> Installed { get; } = new(new Dictionary<SoundEffectPriorityEffect, string>
    {
        [SoundEffectPriorityEffect.Low] = "Low",
        [SoundEffectPriorityEffect.High] = "High",
        [SoundEffectPriorityEffect.Unchanged] = "Unchanged",
    });
}
