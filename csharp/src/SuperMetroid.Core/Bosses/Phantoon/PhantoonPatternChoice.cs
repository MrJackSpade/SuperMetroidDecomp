namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// The pattern his eye-close animation's pattern choice picks next (#1472, #2562): his figure-eight, or the first flame
/// rain, which a vulnerable window that closes his eye, or one that ends in a swoop during the flame rain, chooses.
/// </summary>
public enum PhantoonPatternChoice
{
    /// <summary>He moves on in his figure-eight and opens his eye again.</summary>
    FigureEight,
    /// <summary>He fades out before his first flame rain.</summary>
    FlameRain,
}
