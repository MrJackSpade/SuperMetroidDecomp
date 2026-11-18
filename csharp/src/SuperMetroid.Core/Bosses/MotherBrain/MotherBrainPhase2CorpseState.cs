namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Whether Mother Brain lies as a corpse at the end of the second phase, which the Baby Metroid watches for. The Baby stops
/// draining once Mother Brain is a corpse, and the health colours resume only once she has revived and walked back
/// to her target.
/// </summary>
public enum MotherBrainPhase2CorpseState
{
    /// <summary>Mother Brain has not yet greyed out into a corpse; the Baby keeps draining her.</summary>
    Living,

    /// <summary>The grey fade has finished and her brain health has been restored; the Baby stops draining.</summary>
    Corpse,

    /// <summary>She has revived and reached her walk target; health-based colour handling resumes.</summary>
    Revived,
}
