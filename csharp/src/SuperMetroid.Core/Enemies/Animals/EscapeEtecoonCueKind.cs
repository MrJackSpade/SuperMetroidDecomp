namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>What a cue of the escaping Etecoons' programs does: one per bank-$B3 instruction its lists use.</summary>
internal enum EscapeEtecoonCueKind
{
    /// <summary>Clears the Etecoon's movement routine (<c>ClearPreInstruction</c>, $B3:8074).</summary>
    ClearPreInstruction,
}
