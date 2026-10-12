using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Botwoon's presentation boundary (#641): the installed artwork identity of every typed head and
/// projectile frame. Gameplay animates <see cref="BotwoonHeadFrame"/> and <see cref="BotwoonProjectileFrame"/>
/// values; only this catalog knows the cartridge spritemaps and program operands that name their art.
/// </summary>
internal static class BotwoonVisualDefinitions
{
    /// <summary>Botwoon's head OAM bank, $B3.</summary>
    internal const byte Bank = 0xb3;

    /// <summary>Distinct projectile frames: eight four-phase body links, eight tails, the hidden segment and five spit phases.</summary>
    internal const int ProjectileFrameCount = 46;

    /// <summary>
    /// The facings in the cartridge's physical program order, from up-left counterclockwise to up.
    /// Installed artwork is named and numbered in this order.
    /// </summary>
    private static readonly BotwoonFacing[] PhysicalOrder =
    [
        BotwoonFacing.UpLeft, BotwoonFacing.Left, BotwoonFacing.DownLeft, BotwoonFacing.Down,
        BotwoonFacing.DownRight, BotwoonFacing.Right, BotwoonFacing.UpRight, BotwoonFacing.Up,
    ];

    /// <summary>The tail facings in the cartridge's tail program order, from up counterclockwise to up-right.</summary>
    private static readonly BotwoonFacing[] TailOrder =
    [
        BotwoonFacing.Up, BotwoonFacing.UpLeft, BotwoonFacing.Left, BotwoonFacing.DownLeft,
        BotwoonFacing.Down, BotwoonFacing.DownRight, BotwoonFacing.Right, BotwoonFacing.UpRight,
    ];

    /// <summary><c>InstList_EnemyProjectile_BotwoonsBody_UpLeft</c> at $86:E80F; nine twenty-byte body programs follow, the fourth unused.</summary>
    private const ushort BodyProgramsStart = 0xe80f;
    /// <summary><c>InstList_EnemyProjectile_BotwoonsTail_Up_FacingRight</c> at $86:E8C3; eight six-byte tail programs follow.</summary>
    private const ushort TailProgramsStart = 0xe8c3;
    /// <summary><c>InstList_EnemyProjectile_BotwoonsBodyTail_Hidden</c> at $86:E8F3.</summary>
    private const ushort HiddenSegmentProgram = 0xe8f3;
    /// <summary><c>InstList_EnemyProjectile_BotwoonsSpit</c> at $86:EBAE.</summary>
    private const ushort SpitProgram = 0xebae;

    /// <summary>The installed head spritemap for <paramref name="frame"/>; the hidden frame is the shared empty map.</summary>
    internal static ushort HeadSpritemap(BotwoonHeadFrame frame)
    {
        if (!frame.Visible)
            return CommonEnemyEmptyExtendedFrameDefinitions.EmptySpritemap;
        return frame.MouthOpen ? OpenMouth(frame.Facing) : ClosedMouth(frame.Facing);
    }

    /// <summary>The sixteen visible head frames as installed artwork, closed then open, in physical order.</summary>
    internal static EnemySpritemapDefinition[] Frames() =>
    [
        .. PhysicalOrder.Select(BotwoonHeadFrame.Closed).Concat(PhysicalOrder.Select(BotwoonHeadFrame.Open))
            .Select((frame, index) => new EnemySpritemapDefinition(Bank, HeadSpritemap(frame), $"botwoon_head_{index:D2}")),
    ];

    /// <summary>
    /// Every projectile frame in installed-artwork order: body links by physical facing and phase,
    /// then the tails, the hidden segment and the spit phases.
    /// </summary>
    internal static IReadOnlyList<BotwoonProjectileFrame> ProjectileFrames { get; } =
    [
        .. PhysicalOrder.SelectMany(facing => Enumerable.Range(0, BotwoonAnimations.BodyPhaseCount)
            .Select(phase => new BotwoonProjectileFrame(BotwoonProjectileFrameKind.Body, facing, phase))),
        .. TailOrder.Select(facing => new BotwoonProjectileFrame(BotwoonProjectileFrameKind.Tail, facing, 0)),
        new BotwoonProjectileFrame(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.Up, 0),
        .. Enumerable.Range(0, BotwoonAnimations.SpitPhaseCount)
            .Select(phase => new BotwoonProjectileFrame(BotwoonProjectileFrameKind.Spit, BotwoonFacing.Up, phase)),
    ];

    /// <summary>
    /// The installed presentation key of a projectile frame: the bank-$86 address of the spritemap
    /// operand that drew it, which names its artwork in installed content.
    /// </summary>
    internal static ushort ProjectilePresentationKey(BotwoonProjectileFrame frame) => frame.Kind switch
    {
        BotwoonProjectileFrameKind.Body => unchecked((ushort)(BodyProgramsStart + 20 * BodyPhysicalSlot(frame.Facing) + 4 * frame.Phase + 2)),
        BotwoonProjectileFrameKind.Tail => unchecked((ushort)(TailProgramsStart + 6 * Array.IndexOf(TailOrder, frame.Facing) + 2)),
        BotwoonProjectileFrameKind.Hidden => HiddenSegmentProgram + 2,
        BotwoonProjectileFrameKind.Spit => unchecked((ushort)(SpitProgram + 4 * frame.Phase + 2)),
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Botwoon projectile frame."),
    };

    // The body and head programs share one layout: up-left, left, down-left, an unused
    // down-facing-left slot, then down through up.
    /// <summary>Body physical slot.</summary>
    private static int BodyPhysicalSlot(BotwoonFacing facing)
    {
        int index = Array.IndexOf(PhysicalOrder, facing);
        return index < 3 ? index : index + 1;
    }

    /// <summary>Closed mouth.</summary>
    private static ushort ClosedMouth(BotwoonFacing facing) => facing switch
    {
        // Spritemaps_Botwoon_MouthClosed_Priority2_Aiming* at $B3:E329-$E389; the unused down-facing-left map is skipped.
        BotwoonFacing.UpLeft => 0xe329,
        BotwoonFacing.Left => 0xe335,
        BotwoonFacing.DownLeft => 0xe341,
        BotwoonFacing.Down => 0xe359,
        BotwoonFacing.DownRight => 0xe365,
        BotwoonFacing.Right => 0xe371,
        BotwoonFacing.UpRight => 0xe37d,
        BotwoonFacing.Up => 0xe389,
        _ => throw new ArgumentOutOfRangeException(nameof(facing), facing, "Undefined Botwoon facing."),
    };

    /// <summary>Open mouth.</summary>
    private static ushort OpenMouth(BotwoonFacing facing) => facing switch
    {
        // Spritemaps_Botwoon_MouthOpen_Priority2_Aiming* at $B3:E3A1-$E415; diagonal maps carry a third piece.
        BotwoonFacing.UpLeft => 0xe3a1,
        BotwoonFacing.Left => 0xe3b2,
        BotwoonFacing.DownLeft => 0xe3be,
        BotwoonFacing.Down => 0xe3db,
        BotwoonFacing.DownRight => 0xe3e7,
        BotwoonFacing.Right => 0xe3f8,
        BotwoonFacing.UpRight => 0xe404,
        BotwoonFacing.Up => 0xe415,
        _ => throw new ArgumentOutOfRangeException(nameof(facing), facing, "Undefined Botwoon facing."),
    };
}
