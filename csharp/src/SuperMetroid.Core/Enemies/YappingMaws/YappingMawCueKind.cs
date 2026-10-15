namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>What a cue of the Yapping Maw's programs does: one per bank-$A8 instruction its lists use.</summary>
internal enum YappingMawCueKind
{
    /// <summary>Holds a grabbed Samus at the downward mouth offset (<c>OffsetSamusDown</c>, $A8:A121).</summary>
    HoldSamusDown,
    /// <summary>Holds a grabbed Samus at the down-left mouth offset (<c>OffsetSamusDownLeft</c>, $A8:A0FD).</summary>
    HoldSamusDownLeft,
    /// <summary>Holds a grabbed Samus at the down-right mouth offset (<c>OffsetSamusDownRight</c>, $A8:A0EB).</summary>
    HoldSamusDownRight,
    /// <summary>Holds a grabbed Samus at the upward mouth offset (<c>OffsetSamusUp</c>, $A8:A10F).</summary>
    HoldSamusUp,
    /// <summary>Holds a grabbed Samus at the up-left mouth offset (<c>OffsetSamusUpLeft</c>, $A8:A0D9).</summary>
    HoldSamusUpLeft,
    /// <summary>Holds a grabbed Samus at the up-right mouth offset (<c>OffsetSamusUpRight</c>, $A8:A0C7).</summary>
    HoldSamusUpRight,
    /// <summary>Plays the bite sound when the Yapping Maw is on screen (<c>QueueSFXIfOnScreen</c>, $A8:A133).</summary>
    PlayBiteSound,
}
