namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The artwork and animation family an ending sprite role belongs to.</summary>
internal enum EndingSpriteFamily
{
    /// <summary>Cloud animations advance with zoom-relative motion.</summary>
    Cloud,
    /// <summary>Planet explosion animations can spawn actors and palette effects.</summary>
    Explosion,
    /// <summary>Completion-text animations publish clear-time digits and credits transition.</summary>
    CompletionText,
    /// <summary>Reward animations animate the selected Samus pose.</summary>
    Reward,
    /// <summary>Animals' pod uses its own movement and deletion bounds.</summary>
    AnimalEscape,
}
