namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>The animations of the intro flashback's Rinkas and their spawner (#2197).</summary>
internal enum IntroRinkaAnimation
{
    /// <summary>
    /// A Rinka glowing up, starting to move, then glowing on as it flies; as a segment, one turn of its glow as it flies.
    /// </summary>
    Rinka,
    /// <summary>A segment of the Rinka (#2358): it glows up through its three pictures, then starts to move.</summary>
    RinkaAppearing,
    /// <summary>The invisible spawner's schedule: a wait, the striking pair, a longer wait, the missing pair.</summary>
    Spawner,
}
