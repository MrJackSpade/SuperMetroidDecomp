namespace SuperMetroid.Core.Frontend;

/// <summary>The artwork and instruction-list family an ending sprite role belongs to.</summary>
internal enum EndingSpriteFamily
{
    /// <summary>Cloud lists advance with zoom-relative motion.</summary>
    Cloud,
    /// <summary>Planet explosion lists can spawn actors and palette effects.</summary>
    Explosion,
    /// <summary>Completion-text lists publish clear-time digits and credits transition.</summary>
    CompletionText,
    /// <summary>Reward lists animate the selected Samus pose.</summary>
    Reward,
    /// <summary>Animals' pod uses its own movement and deletion bounds.</summary>
    AnimalEscape,
}
