namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The phases of Mother Brain's final death (#2208): her explosions, fades and corpse, the escape text, timer and door. Its values are arbitrary.</summary>
public enum MotherBrainDeathPhase
{
    /// <summary>Phase-three death/escape: moves to the back of the room.</summary>
    MoveToBackOfRoom,

    /// <summary>Phase-three death/escape: idles while exploding.</summary>
    IdleWhilstExploding,

    /// <summary>Phase-three death/escape: stumbles to the middle of the room.</summary>
    StumbleToMiddleOfRoom,

    /// <summary>Phase-three death/escape: disables the brain's effects.</summary>
    DisableBrainEffects,

    /// <summary>Phase-three death/escape: sets up the body's fade-out.</summary>
    SetupBodyFadeOut,

    /// <summary>Phase-three death/escape: fades out the body.</summary>
    FadeOutBody,

    /// <summary>Phase-three death/escape: the final few explosions.</summary>
    FinalFewExplosions,

    /// <summary>Phase-three death/escape: the brain realizes its decapitation.</summary>
    RealizeDecapitation,

    /// <summary>Phase-three death/escape: the brain falls to the ground.</summary>
    BrainFallsToGround,

    /// <summary>Phase-three death/escape: readies her corpse one update at a time.</summary>
    ReadyCorpse,

    /// <summary>Phase-three death/escape: sets up the fade to grey.</summary>
    SetupFadeToGrey,

    /// <summary>Phase-three death/escape: fades to grey.</summary>
    FadeToGrey,

    /// <summary>Phase-three death/escape: the corpse tips over.</summary>
    CorpseTipsOver,

    /// <summary>Phase-three death/escape: the corpse rots away.</summary>
    CorpseRotsAway,

    /// <summary>Phase-three death/escape: waits twenty frames.</summary>
    PauseBeforeEscape,

    /// <summary>Phase-three death/escape: readies the escape timer and message one update at a time.</summary>
    ReadyEscapeWarning,

    /// <summary>Phase-three death/escape: starts the escape.</summary>
    StartEscape,

    /// <summary>Phase-three death/escape: spawns the time bomb and sets its subtitle.</summary>
    SpawnTimeBombSetSubtitle,

    /// <summary>Phase-three death/escape: types out the Zebes escape text.</summary>
    TypeOutZebesEscapeText,

    /// <summary>Phase-three death/escape: the door explodes and the timer starts.</summary>
    DoorExplodingStartTimer,

    /// <summary>Phase-three death/escape: blows up the escape door.</summary>
    BlowUpEscapeDoor,

    /// <summary>Phase-three death/escape: keeps the earthquake going.</summary>
    KeepEarthquakeGoing,
}
