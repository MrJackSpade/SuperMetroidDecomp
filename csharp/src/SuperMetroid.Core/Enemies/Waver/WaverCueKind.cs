namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>What a cue of Waver's programs does: one per bank-$A3 instruction its lists use.</summary>
internal enum WaverCueKind
{
    /// <summary>Reports the spin finished to main AI (<c>SetSpinFinishedFlag</c>, $A3:86E3).</summary>
    FinishSpin,
}
