namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>What a cue of the dead Sidehopper's and Tourian corpses' programs does: one per bank-$A9 instruction its lists use.</summary>
internal enum DeadTourianCueKind
{
    /// <summary>Hands the landed Sidehopper back to its post-animation main AI (<c>EndHop</c>, $A9:ECD0).</summary>
    EndHop,
}
