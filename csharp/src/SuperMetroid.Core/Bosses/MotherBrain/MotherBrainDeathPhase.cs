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
    BrainComesAway,

    /// <summary>Mixed explosions burst before her body fades.</summary>
    ExplodesBeforeBodyFades,

    /// <summary>Her flickering body fades to black.</summary>
    FadeOutBody,

    /// <summary>The last mixed explosions burst where her body was.</summary>
    FinalFewExplosions,

    /// <summary>She shows herself decapitated and her brain starts to fall.</summary>
    RealizeDecapitation,

    /// <summary>Her brain falls to the floor.</summary>
    BrainFallsToGround,

    /// <summary>Her landed brain settles, the landing's quake shaking the room.</summary>
    BrainSettles,

    /// <summary>She lies still before her corpse greys.</summary>
    LiesStillBeforeGreying,

    /// <summary>Her corpse fades to grey.</summary>
    FadeToGrey,

    /// <summary>Her corpse tips over.</summary>
    CorpseTipsOver,

    /// <summary>Her corpse rots away.</summary>
    CorpseRotsAway,

    /// <summary>The pause before the escape.</summary>
    PauseBeforeEscape,

    /// <summary>The room holds, the brain parked, before the escape starts.</summary>
    HoldBeforeEscape,

    /// <summary>The escape's start waits, its first update the hold's last, then the escape's music, quake, flashing and text start.</summary>
    StartEscape,

    /// <summary>The escape text types on, then the Japanese time-bomb subtitle shows.</summary>
    ShowsEscapeSubtitle,

    /// <summary>The escape text types out.</summary>
    TypeOutZebesEscapeText,

    /// <summary>Dust bursts over the escape door, then the escape timer starts.</summary>
    EscapeDoorBursts,

    /// <summary>The escape door blows apart and opens.</summary>
    BlowUpEscapeDoor,

    /// <summary>The escape's earthquake keeps going.</summary>
    KeepEarthquakeGoing,
}
