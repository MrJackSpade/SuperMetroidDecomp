namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>What a cue of the Rinkas' animations does: show the Rinka and start its aim delay, or hide it, keeping it processing off screen or not.</summary>
internal enum RinkaCueKind
{
    /// <summary>Makes the Rinka visible and tangible and starts its aim delay.</summary>
    Fire,
    /// <summary>Makes the Rinka intangible and invisible.</summary>
    Hide,
    /// <summary>Makes the Rinka intangible and invisible and keeps it processing off screen.</summary>
    HideAndProcessOffScreen,
}
