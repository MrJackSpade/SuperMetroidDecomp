namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>What a cue of Zoa's programs does: one per bank-$A3 instruction its lists use.</summary>
internal enum ZoaCueKind
{
    /// <summary>Selects the third, two-pixel launch speed (<c>SetXSpeedTableIndexToC</c>, $A3:B43F).</summary>
    SetSpeedStage3,
    /// <summary>Selects the first, half-pixel launch speed (<c>SetXSpeedTableIndexTo4</c>, $A3:B429).</summary>
    SetSpeedStage1,
    /// <summary>Selects the second, five-eighths-pixel launch speed (<c>SetXSpeedTableIndexTo8</c>, $A3:B434).</summary>
    SetSpeedStage2,
}
