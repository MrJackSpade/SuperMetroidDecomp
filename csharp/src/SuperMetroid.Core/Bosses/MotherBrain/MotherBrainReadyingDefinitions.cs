namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// How many updates each of Mother Brain's readying phases waits before it moves on (#1406, #1538). Each phase readies
/// one update at a time, as the encounter readies one part a frame, and the phase that follows starts on
/// the update the last one runs; her frames show their finished pictures throughout.
/// </summary>
internal static class MotherBrainReadyingDefinitions
{
    /// <summary>
    /// The updates her rise from the fake death readies her standing body, her legs and the attacks they covered, before her
    /// pause before rising runs on in the same update as the last.
    /// </summary>
    internal const int StandingBodyDuration = 12;

    /// <summary>The updates the finish-off readies the Baby Metroid before it spawns, in the same update as the last.</summary>
    internal const int BabyArrivalDuration = 4;

    /// <summary>
    /// The updates the Baby's cutscene readies her attacks again once the Baby has died, before Samus's rainbow runs on in
    /// the same update as the last.
    /// </summary>
    internal const int AttacksAgainDuration = 4;

    /// <summary>The updates her death readies her corpse once her head has fallen, before the corpse's wait starts.</summary>
    internal const int CorpseDuration = 6;

    /// <summary>The updates her death readies the escape timer and message, before the escape door's explosion readies.</summary>
    internal const int EscapeWarningDuration = 7;

    /// <summary>The updates her death readies the escape door's explosion, before the door explodes.</summary>
    internal const int ExplodedDoorDuration = 2;
}
