namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// An effect one update of Mother Brain's late fight and death starts in her room (#2562). The update's phases ask for
/// each as they run (<see cref="MotherBrainRainbowBeamStepRequests"/>), and the step reports the effects it asked for
/// (<see cref="MotherBrainRainbowBeamAttackStepResult.Effects"/>), which the encounter and the room then start.
/// </summary>
public enum MotherBrainStepEffect
{
    /// <summary>The beam's sound: its start or its stop, as the sequence's choice says.</summary>
    RainbowBeamSound,
    /// <summary>Her rainbow colours step, which change only on their phase of the body's update count.</summary>
    RainbowColours,
    /// <summary>The beam narrowed below its narrowest and let Samus go, so her colours return and the drain's sound stops.</summary>
    BeamEnded,
    /// <summary>The drained beam ran out at the painful walk's sixth step, leaving her colours on the drained frame.</summary>
    DrainedBeamRanOut,
    /// <summary>Disabling her brain's effects detached her brain, which keeps the colours it shows.</summary>
    BrainDetached,
    /// <summary>Her body's fade ran, which flickers her body with the background on every update.</summary>
    BodyFlickers,
    /// <summary>The beam's charge silences her body's sound.</summary>
    ChargeSound,
    /// <summary>Her faded body has just gone, so her body's picture on the background is cleared (#1466).</summary>
    BodyCleared,
    /// <summary>The Baby Metroid spawns.</summary>
    BabySpawn,
    /// <summary>The final beam fired: its sound plays and her rainbow colours start again from the first.</summary>
    FinalBeamFired,
    /// <summary>The music stops.</summary>
    MusicStop,
    /// <summary>The escape's music data loads.</summary>
    EscapeMusic,
    /// <summary>The escape's music track plays.</summary>
    EscapeMusicTrack,
    /// <summary>The escape's warning text starts typing over the background.</summary>
    EscapeTypewriterSetup,
    /// <summary>The escape's subtitle spawns.</summary>
    EscapeSubtitle,
    /// <summary>Samus's drained get-up ends, because the escape countdown takes its place.</summary>
    GetUpRelease,
    /// <summary>The escape countdown starts.</summary>
    EscapeTimerStart,
    /// <summary>Mother Brain's defeat is recorded as her area's mini-boss.</summary>
    DefeatRecord,
    /// <summary>The Zebes time bomb's story event is set.</summary>
    ZebesTimebombEvent,
}
