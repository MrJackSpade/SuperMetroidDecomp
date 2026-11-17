namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The animations of the intro's Metroid egg and Baby Metroid (#2197), and the segments they play (#2358): an animation an
/// actor starts plays its own segment, after the segments named here that lead into it.
/// </summary>
internal enum IntroBabyAnimation
{
    /// <summary>The Metroid egg at rest.</summary>
    Egg,
    /// <summary>
    /// The Metroid egg resting, rocking four times, cracking and hatching, then its shell settling until the third page
    /// starts; as a segment, its last still moment, its cracking and hatching and its shell settling.
    /// </summary>
    EggHatching,
    /// <summary>A segment of the hatching egg (#2358): the egg resting intact before it starts to rock.</summary>
    EggResting,
    /// <summary>A segment of the hatching egg (#2358): one rock of the egg, left and back, then right and back.</summary>
    EggRocking,
    /// <summary>The confused Baby Metroid's loop.</summary>
    ConfusedBaby,
    /// <summary>
    /// The Baby Metroid being delivered: its pulse, played ten times before the fourth page starts, then on; as a segment,
    /// one pulse.
    /// </summary>
    DeliveredBaby,
    /// <summary>A segment of the delivered Baby (#2358): the moment its tenth pulse ends and the fourth page starts.</summary>
    DeliveredBabyStartingPageFour,
    /// <summary>
    /// The Baby Metroid being examined: its pulse, played ten times before the fifth page starts, then on; as a segment, one
    /// pulse.
    /// </summary>
    ExaminedBaby,
    /// <summary>A segment of the examined Baby (#2358): the moment its tenth pulse ends and the fifth page starts.</summary>
    ExaminedBabyStartingPageFive,
}
