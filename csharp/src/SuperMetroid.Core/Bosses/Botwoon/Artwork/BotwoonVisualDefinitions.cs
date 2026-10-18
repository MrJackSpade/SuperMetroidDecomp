using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Botwoon.Artwork;

/// <summary>
/// Botwoon's presentation boundary (#641): the installed artwork identity of every typed head and
/// projectile frame. Gameplay animates <see cref="BotwoonHeadFrame"/> and <see cref="BotwoonProjectileFrame"/>
/// values; only this catalog knows the frames that draw them.
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

    /// <summary>The installed head spritemap for <paramref name="frame"/>; the hidden frame is the shared empty map.</summary>
    internal static ushort HeadSpritemap(BotwoonHeadFrame frame)
    {
        if (!frame.Visible)
            return CommonEnemyEmptyExtendedFrameDefinitions.EmptySpritemap;
        return frame.MouthOpen ? OpenMouth(frame.Facing) : ClosedMouth(frame.Facing);
    }

    /// <summary>
    /// The installed frame each head pose shows: the visible frames are named in the physical order of
    /// <see cref="Frames"/>, closed then open; the hidden head shows the shared empty frame.
    /// </summary>
    internal static EnemyFrameTable<BotwoonHeadFrame> HeadFrames { get; } = new()
    {
        { BotwoonHeadFrame.Hidden, EnemyFrameId.Empty },
        { BotwoonHeadFrame.Closed(BotwoonFacing.UpLeft), EnemyFrameId.BotwoonHead00 },
        { BotwoonHeadFrame.Closed(BotwoonFacing.Left), EnemyFrameId.BotwoonHead01 },
        { BotwoonHeadFrame.Closed(BotwoonFacing.DownLeft), EnemyFrameId.BotwoonHead02 },
        { BotwoonHeadFrame.Closed(BotwoonFacing.Down), EnemyFrameId.BotwoonHead03 },
        { BotwoonHeadFrame.Closed(BotwoonFacing.DownRight), EnemyFrameId.BotwoonHead04 },
        { BotwoonHeadFrame.Closed(BotwoonFacing.Right), EnemyFrameId.BotwoonHead05 },
        { BotwoonHeadFrame.Closed(BotwoonFacing.UpRight), EnemyFrameId.BotwoonHead06 },
        { BotwoonHeadFrame.Closed(BotwoonFacing.Up), EnemyFrameId.BotwoonHead07 },
        { BotwoonHeadFrame.Open(BotwoonFacing.UpLeft), EnemyFrameId.BotwoonHead08 },
        { BotwoonHeadFrame.Open(BotwoonFacing.Left), EnemyFrameId.BotwoonHead09 },
        { BotwoonHeadFrame.Open(BotwoonFacing.DownLeft), EnemyFrameId.BotwoonHead10 },
        { BotwoonHeadFrame.Open(BotwoonFacing.Down), EnemyFrameId.BotwoonHead11 },
        { BotwoonHeadFrame.Open(BotwoonFacing.DownRight), EnemyFrameId.BotwoonHead12 },
        { BotwoonHeadFrame.Open(BotwoonFacing.Right), EnemyFrameId.BotwoonHead13 },
        { BotwoonHeadFrame.Open(BotwoonFacing.UpRight), EnemyFrameId.BotwoonHead14 },
        { BotwoonHeadFrame.Open(BotwoonFacing.Up), EnemyFrameId.BotwoonHead15 },
    };

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

    /// <summary>Selects the bank-$B3 closed-mouth spritemap, skipping the unused down-facing-left map.</summary>
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

    /// <summary>Selects the bank-$B3 open-mouth spritemap; diagonal facings carry a third OAM piece.</summary>
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

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<BotwoonProjectileFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<BotwoonProjectileFrame, EnemyProjectileFrameId>
        {
            [new(BotwoonProjectileFrameKind.Body, BotwoonFacing.Up, 0)] = EnemyProjectileFrameId.BotwoonFrame28,
            [new(BotwoonProjectileFrameKind.Body, BotwoonFacing.Up, 1)] = EnemyProjectileFrameId.BotwoonFrame29,
            [new(BotwoonProjectileFrameKind.Body, BotwoonFacing.Up, 2)] = EnemyProjectileFrameId.BotwoonFrame30,
            [new(BotwoonProjectileFrameKind.Body, BotwoonFacing.Up, 3)] = EnemyProjectileFrameId.BotwoonFrame31,
            [new(BotwoonProjectileFrameKind.Body, BotwoonFacing.UpRight, 0)] = EnemyProjectileFrameId.BotwoonFrame24,
            [new(BotwoonProjectileFrameKind.Body, BotwoonFacing.UpRight, 1)] = EnemyProjectileFrameId.BotwoonFrame25,
            [new(BotwoonProjectileFrameKind.Body, BotwoonFacing.UpRight, 2)] = EnemyProjectileFrameId.BotwoonFrame26,
            [new(BotwoonProjectileFrameKind.Body, BotwoonFacing.UpRight, 3)] = EnemyProjectileFrameId.BotwoonFrame27,
            [new(BotwoonProjectileFrameKind.Body, BotwoonFacing.Right, 0)] = EnemyProjectileFrameId.BotwoonFrame20,
            [new(BotwoonProjectileFrameKind.Body, BotwoonFacing.Right, 1)] = EnemyProjectileFrameId.BotwoonFrame21,
            [new(BotwoonProjectileFrameKind.Body, BotwoonFacing.Right, 2)] = EnemyProjectileFrameId.BotwoonFrame22,
            [new(BotwoonProjectileFrameKind.Body, BotwoonFacing.Right, 3)] = EnemyProjectileFrameId.BotwoonFrame23,
            [new(BotwoonProjectileFrameKind.Body, BotwoonFacing.DownRight, 0)] = EnemyProjectileFrameId.BotwoonFrame16,
            [new(BotwoonProjectileFrameKind.Body, BotwoonFacing.DownRight, 1)] = EnemyProjectileFrameId.BotwoonFrame17,
            [new(BotwoonProjectileFrameKind.Body, BotwoonFacing.DownRight, 2)] = EnemyProjectileFrameId.BotwoonFrame18,
            [new(BotwoonProjectileFrameKind.Body, BotwoonFacing.DownRight, 3)] = EnemyProjectileFrameId.BotwoonFrame19,
            [new(BotwoonProjectileFrameKind.Body, BotwoonFacing.Down, 0)] = EnemyProjectileFrameId.BotwoonFrame12,
            [new(BotwoonProjectileFrameKind.Body, BotwoonFacing.Down, 1)] = EnemyProjectileFrameId.BotwoonFrame13,
            [new(BotwoonProjectileFrameKind.Body, BotwoonFacing.Down, 2)] = EnemyProjectileFrameId.BotwoonFrame14,
            [new(BotwoonProjectileFrameKind.Body, BotwoonFacing.Down, 3)] = EnemyProjectileFrameId.BotwoonFrame15,
            [new(BotwoonProjectileFrameKind.Body, BotwoonFacing.DownLeft, 0)] = EnemyProjectileFrameId.BotwoonFrame08,
            [new(BotwoonProjectileFrameKind.Body, BotwoonFacing.DownLeft, 1)] = EnemyProjectileFrameId.BotwoonFrame09,
            [new(BotwoonProjectileFrameKind.Body, BotwoonFacing.DownLeft, 2)] = EnemyProjectileFrameId.BotwoonFrame10,
            [new(BotwoonProjectileFrameKind.Body, BotwoonFacing.DownLeft, 3)] = EnemyProjectileFrameId.BotwoonFrame11,
            [new(BotwoonProjectileFrameKind.Body, BotwoonFacing.Left, 0)] = EnemyProjectileFrameId.BotwoonFrame04,
            [new(BotwoonProjectileFrameKind.Body, BotwoonFacing.Left, 1)] = EnemyProjectileFrameId.BotwoonFrame05,
            [new(BotwoonProjectileFrameKind.Body, BotwoonFacing.Left, 2)] = EnemyProjectileFrameId.BotwoonFrame06,
            [new(BotwoonProjectileFrameKind.Body, BotwoonFacing.Left, 3)] = EnemyProjectileFrameId.BotwoonFrame07,
            [new(BotwoonProjectileFrameKind.Body, BotwoonFacing.UpLeft, 0)] = EnemyProjectileFrameId.BotwoonFrame00,
            [new(BotwoonProjectileFrameKind.Body, BotwoonFacing.UpLeft, 1)] = EnemyProjectileFrameId.BotwoonFrame01,
            [new(BotwoonProjectileFrameKind.Body, BotwoonFacing.UpLeft, 2)] = EnemyProjectileFrameId.BotwoonFrame02,
            [new(BotwoonProjectileFrameKind.Body, BotwoonFacing.UpLeft, 3)] = EnemyProjectileFrameId.BotwoonFrame03,
            [new(BotwoonProjectileFrameKind.Tail, BotwoonFacing.Up, 0)] = EnemyProjectileFrameId.BotwoonFrame32,
            [new(BotwoonProjectileFrameKind.Tail, BotwoonFacing.Up, 1)] = EnemyProjectileFrameId.BotwoonFrame32,
            [new(BotwoonProjectileFrameKind.Tail, BotwoonFacing.Up, 2)] = EnemyProjectileFrameId.BotwoonFrame32,
            [new(BotwoonProjectileFrameKind.Tail, BotwoonFacing.Up, 3)] = EnemyProjectileFrameId.BotwoonFrame32,
            [new(BotwoonProjectileFrameKind.Tail, BotwoonFacing.Up, 4)] = EnemyProjectileFrameId.BotwoonFrame32,
            [new(BotwoonProjectileFrameKind.Tail, BotwoonFacing.UpRight, 0)] = EnemyProjectileFrameId.BotwoonFrame39,
            [new(BotwoonProjectileFrameKind.Tail, BotwoonFacing.UpRight, 1)] = EnemyProjectileFrameId.BotwoonFrame39,
            [new(BotwoonProjectileFrameKind.Tail, BotwoonFacing.UpRight, 2)] = EnemyProjectileFrameId.BotwoonFrame39,
            [new(BotwoonProjectileFrameKind.Tail, BotwoonFacing.UpRight, 3)] = EnemyProjectileFrameId.BotwoonFrame39,
            [new(BotwoonProjectileFrameKind.Tail, BotwoonFacing.UpRight, 4)] = EnemyProjectileFrameId.BotwoonFrame39,
            [new(BotwoonProjectileFrameKind.Tail, BotwoonFacing.Right, 0)] = EnemyProjectileFrameId.BotwoonFrame38,
            [new(BotwoonProjectileFrameKind.Tail, BotwoonFacing.Right, 1)] = EnemyProjectileFrameId.BotwoonFrame38,
            [new(BotwoonProjectileFrameKind.Tail, BotwoonFacing.Right, 2)] = EnemyProjectileFrameId.BotwoonFrame38,
            [new(BotwoonProjectileFrameKind.Tail, BotwoonFacing.Right, 3)] = EnemyProjectileFrameId.BotwoonFrame38,
            [new(BotwoonProjectileFrameKind.Tail, BotwoonFacing.Right, 4)] = EnemyProjectileFrameId.BotwoonFrame38,
            [new(BotwoonProjectileFrameKind.Tail, BotwoonFacing.DownRight, 0)] = EnemyProjectileFrameId.BotwoonFrame37,
            [new(BotwoonProjectileFrameKind.Tail, BotwoonFacing.DownRight, 1)] = EnemyProjectileFrameId.BotwoonFrame37,
            [new(BotwoonProjectileFrameKind.Tail, BotwoonFacing.DownRight, 2)] = EnemyProjectileFrameId.BotwoonFrame37,
            [new(BotwoonProjectileFrameKind.Tail, BotwoonFacing.DownRight, 3)] = EnemyProjectileFrameId.BotwoonFrame37,
            [new(BotwoonProjectileFrameKind.Tail, BotwoonFacing.DownRight, 4)] = EnemyProjectileFrameId.BotwoonFrame37,
            [new(BotwoonProjectileFrameKind.Tail, BotwoonFacing.Down, 0)] = EnemyProjectileFrameId.BotwoonFrame36,
            [new(BotwoonProjectileFrameKind.Tail, BotwoonFacing.Down, 1)] = EnemyProjectileFrameId.BotwoonFrame36,
            [new(BotwoonProjectileFrameKind.Tail, BotwoonFacing.Down, 2)] = EnemyProjectileFrameId.BotwoonFrame36,
            [new(BotwoonProjectileFrameKind.Tail, BotwoonFacing.Down, 3)] = EnemyProjectileFrameId.BotwoonFrame36,
            [new(BotwoonProjectileFrameKind.Tail, BotwoonFacing.Down, 4)] = EnemyProjectileFrameId.BotwoonFrame36,
            [new(BotwoonProjectileFrameKind.Tail, BotwoonFacing.DownLeft, 0)] = EnemyProjectileFrameId.BotwoonFrame35,
            [new(BotwoonProjectileFrameKind.Tail, BotwoonFacing.DownLeft, 1)] = EnemyProjectileFrameId.BotwoonFrame35,
            [new(BotwoonProjectileFrameKind.Tail, BotwoonFacing.DownLeft, 2)] = EnemyProjectileFrameId.BotwoonFrame35,
            [new(BotwoonProjectileFrameKind.Tail, BotwoonFacing.DownLeft, 3)] = EnemyProjectileFrameId.BotwoonFrame35,
            [new(BotwoonProjectileFrameKind.Tail, BotwoonFacing.DownLeft, 4)] = EnemyProjectileFrameId.BotwoonFrame35,
            [new(BotwoonProjectileFrameKind.Tail, BotwoonFacing.Left, 0)] = EnemyProjectileFrameId.BotwoonFrame34,
            [new(BotwoonProjectileFrameKind.Tail, BotwoonFacing.Left, 1)] = EnemyProjectileFrameId.BotwoonFrame34,
            [new(BotwoonProjectileFrameKind.Tail, BotwoonFacing.Left, 2)] = EnemyProjectileFrameId.BotwoonFrame34,
            [new(BotwoonProjectileFrameKind.Tail, BotwoonFacing.Left, 3)] = EnemyProjectileFrameId.BotwoonFrame34,
            [new(BotwoonProjectileFrameKind.Tail, BotwoonFacing.Left, 4)] = EnemyProjectileFrameId.BotwoonFrame34,
            [new(BotwoonProjectileFrameKind.Tail, BotwoonFacing.UpLeft, 0)] = EnemyProjectileFrameId.BotwoonFrame33,
            [new(BotwoonProjectileFrameKind.Tail, BotwoonFacing.UpLeft, 1)] = EnemyProjectileFrameId.BotwoonFrame33,
            [new(BotwoonProjectileFrameKind.Tail, BotwoonFacing.UpLeft, 2)] = EnemyProjectileFrameId.BotwoonFrame33,
            [new(BotwoonProjectileFrameKind.Tail, BotwoonFacing.UpLeft, 3)] = EnemyProjectileFrameId.BotwoonFrame33,
            [new(BotwoonProjectileFrameKind.Tail, BotwoonFacing.UpLeft, 4)] = EnemyProjectileFrameId.BotwoonFrame33,
            [new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.Up, 0)] = EnemyProjectileFrameId.BotwoonFrame40,
            [new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.Up, 1)] = EnemyProjectileFrameId.BotwoonFrame40,
            [new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.Up, 2)] = EnemyProjectileFrameId.BotwoonFrame40,
            [new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.Up, 3)] = EnemyProjectileFrameId.BotwoonFrame40,
            [new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.Up, 4)] = EnemyProjectileFrameId.BotwoonFrame40,
            [new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.UpRight, 0)] = EnemyProjectileFrameId.BotwoonFrame40,
            [new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.UpRight, 1)] = EnemyProjectileFrameId.BotwoonFrame40,
            [new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.UpRight, 2)] = EnemyProjectileFrameId.BotwoonFrame40,
            [new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.UpRight, 3)] = EnemyProjectileFrameId.BotwoonFrame40,
            [new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.UpRight, 4)] = EnemyProjectileFrameId.BotwoonFrame40,
            [new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.Right, 0)] = EnemyProjectileFrameId.BotwoonFrame40,
            [new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.Right, 1)] = EnemyProjectileFrameId.BotwoonFrame40,
            [new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.Right, 2)] = EnemyProjectileFrameId.BotwoonFrame40,
            [new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.Right, 3)] = EnemyProjectileFrameId.BotwoonFrame40,
            [new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.Right, 4)] = EnemyProjectileFrameId.BotwoonFrame40,
            [new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.DownRight, 0)] = EnemyProjectileFrameId.BotwoonFrame40,
            [new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.DownRight, 1)] = EnemyProjectileFrameId.BotwoonFrame40,
            [new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.DownRight, 2)] = EnemyProjectileFrameId.BotwoonFrame40,
            [new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.DownRight, 3)] = EnemyProjectileFrameId.BotwoonFrame40,
            [new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.DownRight, 4)] = EnemyProjectileFrameId.BotwoonFrame40,
            [new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.Down, 0)] = EnemyProjectileFrameId.BotwoonFrame40,
            [new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.Down, 1)] = EnemyProjectileFrameId.BotwoonFrame40,
            [new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.Down, 2)] = EnemyProjectileFrameId.BotwoonFrame40,
            [new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.Down, 3)] = EnemyProjectileFrameId.BotwoonFrame40,
            [new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.Down, 4)] = EnemyProjectileFrameId.BotwoonFrame40,
            [new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.DownLeft, 0)] = EnemyProjectileFrameId.BotwoonFrame40,
            [new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.DownLeft, 1)] = EnemyProjectileFrameId.BotwoonFrame40,
            [new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.DownLeft, 2)] = EnemyProjectileFrameId.BotwoonFrame40,
            [new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.DownLeft, 3)] = EnemyProjectileFrameId.BotwoonFrame40,
            [new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.DownLeft, 4)] = EnemyProjectileFrameId.BotwoonFrame40,
            [new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.Left, 0)] = EnemyProjectileFrameId.BotwoonFrame40,
            [new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.Left, 1)] = EnemyProjectileFrameId.BotwoonFrame40,
            [new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.Left, 2)] = EnemyProjectileFrameId.BotwoonFrame40,
            [new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.Left, 3)] = EnemyProjectileFrameId.BotwoonFrame40,
            [new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.Left, 4)] = EnemyProjectileFrameId.BotwoonFrame40,
            [new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.UpLeft, 0)] = EnemyProjectileFrameId.BotwoonFrame40,
            [new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.UpLeft, 1)] = EnemyProjectileFrameId.BotwoonFrame40,
            [new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.UpLeft, 2)] = EnemyProjectileFrameId.BotwoonFrame40,
            [new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.UpLeft, 3)] = EnemyProjectileFrameId.BotwoonFrame40,
            [new(BotwoonProjectileFrameKind.Hidden, BotwoonFacing.UpLeft, 4)] = EnemyProjectileFrameId.BotwoonFrame40,
            [new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.Up, 0)] = EnemyProjectileFrameId.BotwoonFrame41,
            [new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.Up, 1)] = EnemyProjectileFrameId.BotwoonFrame42,
            [new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.Up, 2)] = EnemyProjectileFrameId.BotwoonFrame43,
            [new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.Up, 3)] = EnemyProjectileFrameId.BotwoonFrame44,
            [new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.Up, 4)] = EnemyProjectileFrameId.BotwoonFrame45,
            [new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.UpRight, 0)] = EnemyProjectileFrameId.BotwoonFrame41,
            [new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.UpRight, 1)] = EnemyProjectileFrameId.BotwoonFrame42,
            [new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.UpRight, 2)] = EnemyProjectileFrameId.BotwoonFrame43,
            [new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.UpRight, 3)] = EnemyProjectileFrameId.BotwoonFrame44,
            [new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.UpRight, 4)] = EnemyProjectileFrameId.BotwoonFrame45,
            [new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.Right, 0)] = EnemyProjectileFrameId.BotwoonFrame41,
            [new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.Right, 1)] = EnemyProjectileFrameId.BotwoonFrame42,
            [new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.Right, 2)] = EnemyProjectileFrameId.BotwoonFrame43,
            [new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.Right, 3)] = EnemyProjectileFrameId.BotwoonFrame44,
            [new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.Right, 4)] = EnemyProjectileFrameId.BotwoonFrame45,
            [new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.DownRight, 0)] = EnemyProjectileFrameId.BotwoonFrame41,
            [new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.DownRight, 1)] = EnemyProjectileFrameId.BotwoonFrame42,
            [new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.DownRight, 2)] = EnemyProjectileFrameId.BotwoonFrame43,
            [new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.DownRight, 3)] = EnemyProjectileFrameId.BotwoonFrame44,
            [new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.DownRight, 4)] = EnemyProjectileFrameId.BotwoonFrame45,
            [new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.Down, 0)] = EnemyProjectileFrameId.BotwoonFrame41,
            [new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.Down, 1)] = EnemyProjectileFrameId.BotwoonFrame42,
            [new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.Down, 2)] = EnemyProjectileFrameId.BotwoonFrame43,
            [new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.Down, 3)] = EnemyProjectileFrameId.BotwoonFrame44,
            [new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.Down, 4)] = EnemyProjectileFrameId.BotwoonFrame45,
            [new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.DownLeft, 0)] = EnemyProjectileFrameId.BotwoonFrame41,
            [new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.DownLeft, 1)] = EnemyProjectileFrameId.BotwoonFrame42,
            [new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.DownLeft, 2)] = EnemyProjectileFrameId.BotwoonFrame43,
            [new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.DownLeft, 3)] = EnemyProjectileFrameId.BotwoonFrame44,
            [new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.DownLeft, 4)] = EnemyProjectileFrameId.BotwoonFrame45,
            [new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.Left, 0)] = EnemyProjectileFrameId.BotwoonFrame41,
            [new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.Left, 1)] = EnemyProjectileFrameId.BotwoonFrame42,
            [new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.Left, 2)] = EnemyProjectileFrameId.BotwoonFrame43,
            [new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.Left, 3)] = EnemyProjectileFrameId.BotwoonFrame44,
            [new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.Left, 4)] = EnemyProjectileFrameId.BotwoonFrame45,
            [new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.UpLeft, 0)] = EnemyProjectileFrameId.BotwoonFrame41,
            [new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.UpLeft, 1)] = EnemyProjectileFrameId.BotwoonFrame42,
            [new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.UpLeft, 2)] = EnemyProjectileFrameId.BotwoonFrame43,
            [new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.UpLeft, 3)] = EnemyProjectileFrameId.BotwoonFrame44,
            [new(BotwoonProjectileFrameKind.Spit, BotwoonFacing.UpLeft, 4)] = EnemyProjectileFrameId.BotwoonFrame45,
        };
}
