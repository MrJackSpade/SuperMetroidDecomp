namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>What a cue of the Wrecked Ship Spark's animations does: remove the Spark from collision or restore it.</summary>
internal enum SparkCueKind
{
    /// <summary>Removes the Spark from Samus, beam and grapple collision.</summary>
    BecomeIntangible,
    /// <summary>Restores the Spark to Samus, beam and grapple collision.</summary>
    BecomeTangible,
}
