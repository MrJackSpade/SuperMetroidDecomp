namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Carries out the cues her brain's playing animation raises at a draw (#2554), each catalog's with what its playback
/// carries: her head's own over her encounter, her late fight's volley at Samus over that late fight, her volley at the Baby
/// over the arrived Baby.
/// </summary>
internal interface IMotherBrainBrainCues
{
    /// <summary>Carries out a cue of her head's own animations.</summary>
    /// <returns>The segment whose animation a random hold or drool continues at; otherwise null.</returns>
    MotherBrainHeadSegment? RunHeadCue(MotherBrainBrainCue<MotherBrainHeadChanceCueKind> cue);

    /// <summary>Carries out a cue of <paramref name="lateFight"/>'s volley at Samus.</summary>
    /// <returns>The segment whose animation a random hold continues at; otherwise null.</returns>
    MotherBrainLateFightHeadSegment? RunLateFightCue(MotherBrainLateFightState lateFight, MotherBrainBrainCue<MotherBrainLateFightCueKind> cue);

    /// <summary>Carries out a cue of her volley at the arrived <paramref name="baby"/>.</summary>
    /// <returns>The segment whose animation a random hold continues at; otherwise null.</returns>
    MotherBrainBabyAttackSegment? RunBabyAttackCue(BabyMetroidCutsceneState baby, MotherBrainBrainCue<MotherBrainBabyAttackCueKind> cue);
}
