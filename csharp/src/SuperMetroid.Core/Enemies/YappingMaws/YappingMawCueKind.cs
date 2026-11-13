namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>What a cue of the Yapping Maw's animations does: one per kind of step its lists use.</summary>
internal enum YappingMawCueKind
{
    /// <summary>Holds a grabbed Samus at the downward mouth offset.</summary>
    HoldSamusDown,
    /// <summary>Holds a grabbed Samus at the down-left mouth offset.</summary>
    HoldSamusDownLeft,
    /// <summary>Holds a grabbed Samus at the down-right mouth offset.</summary>
    HoldSamusDownRight,
    /// <summary>Holds a grabbed Samus at the upward mouth offset.</summary>
    HoldSamusUp,
    /// <summary>Holds a grabbed Samus at the up-left mouth offset.</summary>
    HoldSamusUpLeft,
    /// <summary>Holds a grabbed Samus at the up-right mouth offset.</summary>
    HoldSamusUpRight,
    /// <summary>Plays the bite sound when the Yapping Maw is on screen.</summary>
    PlayBiteSound,
}
