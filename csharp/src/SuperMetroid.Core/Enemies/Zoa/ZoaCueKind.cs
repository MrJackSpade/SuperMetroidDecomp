namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>What a cue of Zoa's programs does: one per bank-$A3 instruction its lists use.</summary>
internal enum ZoaCueKind
{
    /// <summary>Selects horizontal speed table index 12 (<c>SetXSpeedTableIndexToC</c>, $A3:B43F).</summary>
    SetSpeedIndex12,
    /// <summary>Selects horizontal speed table index 4 (<c>SetXSpeedTableIndexTo4</c>, $A3:B429).</summary>
    SetSpeedIndex4,
    /// <summary>Selects horizontal speed table index 8 (<c>SetXSpeedTableIndexTo8</c>, $A3:B434).</summary>
    SetSpeedIndex8,
}
