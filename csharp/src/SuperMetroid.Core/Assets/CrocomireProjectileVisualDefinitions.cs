using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// The presentation boundary of Crocomire's projectiles (#641): the installed artwork identity of every typed
/// frame. Gameplay animates <see cref="CrocomireProjectileFrame"/> values; only this catalog knows the cartridge
/// operands that name their art.
/// </summary>
internal static class CrocomireProjectileVisualDefinitions
{
    /// <summary>Distinct projectile frames: six mouth, one bridge fragment, one spike piece and five shot frames.</summary>
    internal const int ProjectileFrameCount = 13;

    /// <summary><c>InstList_EnemyProjectile_CrocomiresProjectile</c> at $86:8FCF.</summary>
    private const ushort MouthProgram = 0x8fcf;
    /// <summary><c>InstList_EnemyProjectile_CrocomireBridgeCrumbling</c> at $86:8FEB.</summary>
    private const ushort BridgeFragmentProgram = 0x8feb;
    /// <summary><c>InstList_EnemyProjectile_CrocomireSpikeWallPieces</c> at $86:8FF3.</summary>
    private const ushort SpikeWallPieceProgram = 0x8ff3;
    /// <summary><c>InstList_EnemyProjectile_Shot_CrocomiresProjectile</c> at $86:9007.</summary>
    private const ushort ShotProgram = 0x9007;
    /// <summary>A frame's spritemap operand follows its two-byte duration.</summary>
    private const int OperandOffset = 2;
    /// <summary>Each timed frame is a duration and an operand.</summary>
    private const int FrameBytes = 4;

    /// <summary>Every projectile frame in installed-artwork order: mouth, bridge, spike, then shot.</summary>
    internal static IReadOnlyList<CrocomireProjectileFrame> ProjectileFrames { get; } =
    [
        .. Enumerable.Range(0, CrocomireAnimations.MouthPhaseCount).Select(phase => new CrocomireProjectileFrame(CrocomireProjectileFrameKind.Mouth, phase)),
        new CrocomireProjectileFrame(CrocomireProjectileFrameKind.BridgeFragment, 0),
        new CrocomireProjectileFrame(CrocomireProjectileFrameKind.SpikeWallPiece, 0),
        .. Enumerable.Range(0, CrocomireAnimations.ShotPhaseCount).Select(phase => new CrocomireProjectileFrame(CrocomireProjectileFrameKind.Shot, phase)),
    ];

    /// <summary>
    /// The installed presentation key of a projectile frame: the bank-$86 address of the spritemap operand that
    /// drew it, which names its artwork in installed content.
    /// </summary>
    internal static ushort PresentationKey(CrocomireProjectileFrame frame) => frame.Kind switch
    {
        CrocomireProjectileFrameKind.Mouth => Operand(MouthProgram, frame.Phase),
        CrocomireProjectileFrameKind.BridgeFragment => Operand(BridgeFragmentProgram, frame.Phase),
        CrocomireProjectileFrameKind.SpikeWallPiece => Operand(SpikeWallPieceProgram, frame.Phase),
        CrocomireProjectileFrameKind.Shot => Operand(ShotProgram, frame.Phase),
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Crocomire projectile frame."),
    };

    /// <summary>The operand of the <paramref name="phase"/>th frame of the program at <paramref name="program"/>.</summary>
    private static ushort Operand(ushort program, int phase) => unchecked((ushort)(program + FrameBytes * phase + OperandOffset));
}
