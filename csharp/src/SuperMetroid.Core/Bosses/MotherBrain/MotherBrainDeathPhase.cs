namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The phases of Mother Brain's final death (#2208): her explosions, fades and corpse, the escape text, timer and door. Its values are arbitrary.</summary>
public enum MotherBrainDeathPhase
{
    /// <summary>She walks back, ignoring Samus, to the back of the room.</summary>
    MoveToBackOfRoom,

    /// <summary>She stands at the back of the room while smoky explosions burst over her.</summary>
    IdleWhilstExploding,

    /// <summary>She stumbles forward toward the middle of the room.</summary>
    StumbleToMiddleOfRoom,

    /// <summary>Her neck, drool and breath stop and her brain comes away from her neck.</summary>
    DisableBrainEffects,

    /// <summary>Mixed explosions burst before her body fades.</summary>
    SetupBodyFadeOut,

    /// <summary>Her flickering body fades to black.</summary>
    FadeOutBody,

    /// <summary>The last mixed explosions burst where her body was.</summary>
    FinalFewExplosions,

    /// <summary>She shows herself decapitated and her brain starts to fall.</summary>
    RealizeDecapitation,

    /// <summary>Her brain falls to the floor.</summary>
    BrainFallsToGround,

    /// <summary>Her corpse is readied one update at a time.</summary>
    ReadyCorpse,

    /// <summary>She lies still before her corpse greys.</summary>
    SetupFadeToGrey,

    /// <summary>Her corpse fades to grey.</summary>
    FadeToGrey,

    /// <summary>Her corpse tips over.</summary>
    CorpseTipsOver,

    /// <summary>Her corpse rots away.</summary>
    CorpseRotsAway,

    /// <summary>The pause before the escape.</summary>
    PauseBeforeEscape,

    /// <summary>The escape timer and message are readied one update at a time.</summary>
    ReadyEscapeWarning,

    /// <summary>The escape door's explosion is readied, then the escape's music, quake, flashing and text start.</summary>
    StartEscape,

    /// <summary>The escape text types on, then the Japanese time-bomb subtitle shows.</summary>
    SpawnTimeBombSetSubtitle,

    /// <summary>The escape text types out.</summary>
    TypeOutZebesEscapeText,

    /// <summary>Dust bursts over the escape door, then the escape timer starts.</summary>
    DoorExplodingStartTimer,

    /// <summary>The escape door blows apart and opens.</summary>
    BlowUpEscapeDoor,

    /// <summary>The escape's earthquake keeps going.</summary>
    KeepEarthquakeGoing,
}
