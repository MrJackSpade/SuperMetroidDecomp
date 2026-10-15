
namespace SuperMetroid.Core.Bosses.Draygon.Artwork;

/// <summary>
/// The presentation boundary of Draygon's projectiles (#641): the installed artwork key of every typed frame, the
/// bank-$86 address of the spritemap operand that drew it.
/// </summary>
internal static class DraygonProjectileVisualDefinitions
{
    /// <summary>Distinct frames: six goop, two goop-burst, sixteen bloom and three flight frames.</summary>
    internal const int ProjectileFrameCount = 27;

    /// <summary><c>InstList_EnemyProjectile_DraygonGoop</c> at $86:8C3A.</summary>
    private const ushort GoopProgram = 0x8c3a;
    /// <summary><c>InstList_EnemyProjectile_DraygonGoop_Shot</c> at $86:8C58.</summary>
    private const ushort GoopShotProgram = 0x8c58;
    /// <summary><c>InstList_EnemyProjectile_DraygonsWallTurretProjectile_0</c> at $86:8CA4.</summary>
    private const ushort BloomProgram = 0x8ca4;
    /// <summary><c>InstList_EnemyProjectile_DraygonsWallTurretProjectile_1</c> at $86:8CE6.</summary>
    private const ushort FlightProgram = 0x8ce6;
    /// <summary>A frame's spritemap operand follows its two-byte duration.</summary>
    private const int OperandOffset = 2;
    /// <summary>Each timed frame is a duration and an operand.</summary>
    private const int FrameBytes = 4;

    /// <summary>Every frame in installed-artwork order: goop, burst, bloom, then flight.</summary>
    internal static IReadOnlyList<DraygonProjectileFrame> ProjectileFrames { get; } =
    [
        .. Frames(DraygonProjectileFrameKind.Goop, DraygonProjectileAnimations.GoopPhaseCount),
        .. Frames(DraygonProjectileFrameKind.GoopShot, DraygonProjectileAnimations.GoopShotPhaseCount),
        .. Frames(DraygonProjectileFrameKind.WallTurretBloom, DraygonProjectileAnimations.BloomPhaseCount),
        .. Frames(DraygonProjectileFrameKind.WallTurretFlight, DraygonProjectileAnimations.FlightPhaseCount),
    ];

    /// <summary>The installed presentation key of <paramref name="frame"/>.</summary>
    internal static ushort PresentationKey(DraygonProjectileFrame frame) => unchecked((ushort)(frame.Kind switch
    {
        DraygonProjectileFrameKind.Goop => GoopProgram,
        DraygonProjectileFrameKind.GoopShot => GoopShotProgram,
        DraygonProjectileFrameKind.WallTurretBloom => BloomProgram,
        DraygonProjectileFrameKind.WallTurretFlight => FlightProgram,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Draygon projectile frame."),
    } + FrameBytes * frame.Phase + OperandOffset));

    /// <summary>The <paramref name="count"/> frames of <paramref name="kind"/>.</summary>
    private static IEnumerable<DraygonProjectileFrame> Frames(DraygonProjectileFrameKind kind, int count) =>
        Enumerable.Range(0, count).Select(phase => new DraygonProjectileFrame(kind, phase));
}
