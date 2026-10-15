using K = SuperMetroid.Core.Bosses.Ridley.RidleyFireballFrameKind;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>
/// The presentation boundary of Ridley's fireballs and afterburns (#641): the bank-$86 list entries the projectile
/// definitions name, and the installed artwork key of each typed frame, which is its native spritemap operand's
/// address.
/// </summary>
internal static class RidleyFireballVisualDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_RidleysFireball_0</c> at $86:9552.</summary>
    internal const ushort FireballProgram = 0x9552;
    /// <summary><c>InstList_EnemyProjectile_HorizontalAfterburn_Center</c> at $86:95A0.</summary>
    internal const ushort HorizontalCenterProgram = 0x95a0;
    /// <summary><c>InstList_EnemyProjectile_VerticalAfterburn_Center</c> at $86:95D3.</summary>
    internal const ushort VerticalCenterProgram = 0x95d3;
    /// <summary><c>InstList_EnemyProjectile_Afterburn</c> at $86:9606.</summary>
    internal const ushort DirectionalAfterburnProgram = 0x9606;

    /// <summary>The first launch pose's operand ($86:9556).</summary>
    private const ushort LaunchOperand = 0x9556;
    /// <summary>The second launch pose's operand, past the pre-instruction install ($86:955E).</summary>
    private const ushort SecondLaunchOperand = 0x955e;
    /// <summary>The first flight pose's operand ($86:9562).</summary>
    private const ushort FlightOperand = 0x9562;
    /// <summary>The first dying-afterburn pose's operand ($86:9578).</summary>
    private const ushort AfterburnFinalOperand = 0x9578;
    /// <summary>A four-byte frame record.</summary>
    private const int FrameBytes = 4;
    /// <summary>The spawn command a spawning run carries after its first pose.</summary>
    private const int SpawnCommandBytes = 2;
    /// <summary>The first operand's offset in a spawning run, past its clear-pre-instruction command and duration.</summary>
    private const int FirstOperandOffset = 4;

    /// <summary>The installed fireball and afterburn frames.</summary>
    internal const int ProjectileFrameCount = 26;

    /// <summary>Every frame in installed order.</summary>
    internal static IReadOnlyList<RidleyFireballFrame> ProjectileFrames { get; } =
    [
        .. Run(K.Launch, RidleyFireballAnimations.LaunchPoseCount),
        .. Run(K.Flight, RidleyFireballAnimations.FlightPoseCount),
        .. Run(K.AfterburnFinal, RidleyFireballAnimations.AfterburnPoseCount),
        .. Run(K.HorizontalCenter, RidleyFireballAnimations.AfterburnPoseCount),
        .. Run(K.VerticalCenter, RidleyFireballAnimations.AfterburnPoseCount),
        .. Run(K.Directional, RidleyFireballAnimations.AfterburnPoseCount),
    ];

    /// <summary>The installed artwork key of <paramref name="frame"/>: its native operand's address.</summary>
    internal static ushort PresentationKey(RidleyFireballFrame frame) => frame.Kind switch
    {
        K.Launch when frame.Phase is 0 => LaunchOperand,
        K.Launch when frame.Phase is 1 => SecondLaunchOperand,
        K.Flight when (uint)frame.Phase < RidleyFireballAnimations.FlightPoseCount => (ushort)(FlightOperand + FrameBytes * frame.Phase),
        K.AfterburnFinal when (uint)frame.Phase < RidleyFireballAnimations.AfterburnPoseCount =>
            (ushort)(AfterburnFinalOperand + FrameBytes * frame.Phase),
        K.HorizontalCenter => SpawningRunOperand(HorizontalCenterProgram, frame.Phase),
        K.VerticalCenter => SpawningRunOperand(VerticalCenterProgram, frame.Phase),
        K.Directional => SpawningRunOperand(DirectionalAfterburnProgram, frame.Phase),
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Ridley fireball frame."),
    };

    /// <summary>The operand of pose <paramref name="phase"/> in a spawning run, whose spawn command follows its first pose.</summary>
    private static ushort SpawningRunOperand(ushort program, int phase) =>
        (uint)phase < RidleyFireballAnimations.AfterburnPoseCount
            ? (ushort)(program + FirstOperandOffset + FrameBytes * phase + (phase == 0 ? 0 : SpawnCommandBytes))
            : throw new ArgumentOutOfRangeException(nameof(phase), phase, "An afterburn run has five poses.");

    /// <summary>The <paramref name="count"/> poses of <paramref name="kind"/>.</summary>
    private static IEnumerable<RidleyFireballFrame> Run(K kind, int count) =>
        Enumerable.Range(0, count).Select(phase => new RidleyFireballFrame(kind, phase));
}
