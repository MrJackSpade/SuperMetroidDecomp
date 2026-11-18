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
    /// The Baby Metroid being delivered: its pulse, played ten times before the next page starts, then on; as a segment,
    /// one pulse.
    /// </summary>
    DeliveredBaby,
    /// <summary>The delivered Baby's eleventh pulse (#2489), at whose start it starts the page the delivery leads into.</summary>
    DeliveredBabyTurningPage,
    /// <summary>
    /// The Baby Metroid being examined: its pulse, played ten times before the next page starts, then on; as a segment, one
    /// pulse.
    /// </summary>
    ExaminedBaby,
    /// <summary>The examined Baby's eleventh pulse (#2489), at whose start it starts the page the examination leads into.</summary>
    ExaminedBabyTurningPage,
}
