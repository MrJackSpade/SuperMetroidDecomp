namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>What a cue of the Wrecked Ship Spark's programs does: one per bank-$A8 instruction its lists use.</summary>
internal enum SparkCueKind
{
    /// <summary>Removes the Spark from Samus, beam and grapple collision (<c>SetAsIntangible</c>, $A8:E61D).</summary>
    BecomeIntangible,
    /// <summary>Restores the Spark to Samus, beam and grapple collision (<c>SetAsTangible</c>, $A8:E62A).</summary>
    BecomeTangible,
}
