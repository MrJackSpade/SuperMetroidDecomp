namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>What a cue of the Rinkas' programs does: one per kind of step its lists use.</summary>
internal enum RinkaCueKind
{
    /// <summary>Makes the Rinka visible and tangible and starts its aim delay.</summary>
    Fire,
    /// <summary>Makes the Rinka intangible and invisible.</summary>
    Hide,
    /// <summary>Makes the Rinka intangible and invisible and keeps it processing off screen.</summary>
    HideAndProcessOffScreen,
}
