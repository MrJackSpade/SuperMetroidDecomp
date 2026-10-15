namespace SuperMetroid.Core.Enemies;

/// <summary>Bank-$86 falling-spark randomization data.</summary>
internal static class EnemyRomTablePointersFallingSpark
{
    /// <summary>Falling-spark horizontal whole-velocity words at $86:F3D4, four-byte stride; seven authored records plus an eighth native overread.</summary>
    public const int HorizontalWholeWords = 0x86f3d4;
    /// <summary>Falling-spark horizontal fractional-velocity words at $86:F3D6, four-byte stride.</summary>
    public const int HorizontalFractionWords = 0x86f3d6;
}
