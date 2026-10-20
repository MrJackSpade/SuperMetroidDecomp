namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>What a cue of the Yard's programs does: one per bank-$A3 instruction its lists use.</summary>
internal enum YardCueKind
{
    /// <summary>While hiding, or on a 50% chance, shows the previous pose again; otherwise runs on (<c>GoBack4BytesIfHidingOr50PercentChance</c>, $A3:CC78).</summary>
    RepeatPreviousPoseUnlessContinuing,
}
