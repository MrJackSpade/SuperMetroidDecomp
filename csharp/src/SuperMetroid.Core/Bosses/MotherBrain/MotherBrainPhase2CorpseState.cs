namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The corpse handshake between Mother Brain and the Baby Metroid at the end of the second phase. The Baby stops
/// draining once Mother Brain is a corpse, and the health palette resumes only once she has revived and walked back
/// to her target. The backing values are the snapshot word the verification import decodes.
/// </summary>
public enum MotherBrainPhase2CorpseState : ushort
{
    /// <summary>Mother Brain has not yet greyed out into a corpse; the Baby keeps draining her.</summary>
    Living = 0,

    /// <summary>The grey fade has finished and her brain health has been restored; the Baby stops draining.</summary>
    Corpse = 1,

    /// <summary>She has revived and reached her walk target; health-based palette handling resumes.</summary>
    Revived = 2,
}
