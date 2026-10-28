namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Morph-ball bounce state at WRAM <c>$0B20</c>. The low byte counts rebounds; the Spring Ball
/// landing handler (<c>$91:F25E</c>) additionally stores high byte <c>$06</c> as its bounce
/// family. Bank <c>$91</c> stores only these five words.
/// </summary>
public enum SamusBallBounceState
{
    /// <summary>Zero: not bouncing.</summary>
    None = 0x0000,

    /// <summary>Morph Ball first rebound (<c>$91:EFxx</c> ordinary landing).</summary>
    FirstRebound = 0x0001,

    /// <summary>Morph Ball second rebound.</summary>
    SecondRebound = 0x0002,

    /// <summary>Spring Ball first rebound: family <c>$06</c>, rebound one.</summary>
    SpringBallFirstRebound = 0x0601,

    /// <summary>Spring Ball second rebound: family <c>$06</c>, rebound two.</summary>
    SpringBallSecondRebound = 0x0602,
}
