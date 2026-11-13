namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>What a cue of the Wrecked Ship Spark's programs does: one per kind of step its lists use.</summary>
internal enum SparkCueKind
{
    /// <summary>Removes the Spark from Samus, beam and grapple collision.</summary>
    BecomeIntangible,
    /// <summary>Restores the Spark to Samus, beam and grapple collision.</summary>
    BecomeTangible,
}
