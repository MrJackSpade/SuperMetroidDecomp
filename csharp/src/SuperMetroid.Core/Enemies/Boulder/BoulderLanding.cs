namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>How far below its resting place a Boulder's landing is compensated (#1472).</summary>
internal enum BoulderLanding
{
    /// <summary>Two pixels, as every retail Boulder lands.</summary>
    Standard,

    /// <summary>Five pixels.</summary>
    Deep,
}
