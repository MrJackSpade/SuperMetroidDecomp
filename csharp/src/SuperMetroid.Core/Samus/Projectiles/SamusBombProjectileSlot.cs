using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Artwork;
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// One of Samus's bombs: a placed bomb, a Power Bomb, or a bomb-spread bomb. Each is created fresh when it takes its
/// place among the five (<see cref="SamusBombProjectileSystem"/>) and ends when it is deleted or Samus's bombs are
/// cleared; nothing it holds reaches the next bomb in its place. A place no bomb holds reads as the bomb that last
/// ended there, or one never placed: no type, no damage, at rest at the room's origin.
/// </summary>
public sealed class SamusBombProjectileSlot
{
    /// <summary>Creates a bomb, not yet placed, in <paramref name="index"/> among the five places.</summary>
    internal SamusBombProjectileSlot(int index) => Index = index;

    /// <summary>The bomb's place among the five, zero through four: bombs take the lowest free one, and a spread fills all five.</summary>
    public int Index { get; }

    /// <summary>Room position in 1/65536 pixels.</summary>
    public FixedPosition<RoomPixels> Position { get; internal set; }

    /// <summary>
    /// Whether the bomb has already struck an enemy: its explosion keeps running, but it no longer launches Samus and
    /// later enemy collision skips it.
    /// </summary>
    public bool IsSpent { get; internal set; }

    /// <summary>
    /// The direction the shared projectile reactions read for a bomb. Bombs are placed without an aim, so they read as
    /// fired upward facing right.
    /// </summary>
    public static SamusProjectileDirection Direction => SamusProjectileDirection.UpFacingRight;

    /// <summary>What the bomb is; <see cref="SamusProjectileType.None"/> before it is placed and once it ends.</summary>
    public SamusProjectileType Type { get; internal set; }

    /// <summary>
    /// The beam combination whose pictures the bomb and its explosion show (#1586): the beams Samus had equipped when
    /// she placed it, whatever she equips while it burns.
    /// </summary>
    public SamusBeamCombination PicturedBeams { get; internal set; }

    /// <summary>Whether a placed bomb's explosion has reacted with the five-block cross of terrain around it; it does so once.</summary>
    public bool HasReactedToTerrain { get; internal set; }

    /// <summary>Damage from the bomb's projectile data, which every bomb carries; zero before it is placed and once it ends.</summary>
    public int Damage
    {
        get;
        internal set => field = Ensure.AtLeastZero(value);
    }
    /// <summary>The slot's projectile animation and where it stands in it (#1600); null marks an inactive slot, and a consumed timed frame plays it past that record.</summary>
    internal AnimationPlayback<SamusProjectileSegment>? Playback { get; set; }
    /// <summary>Wrapping duration countdown, decremented once per unfrozen projectile update; reaching zero runs the next animation cue.</summary>
    public WrappingCountdown FrameHold { get; internal set; }
    /// <summary>The composition the bomb shows, selected by the timed-frame animation or installed frame binding; none before its first frame.</summary>
    public ProjectileSpriteId? Sprite { get; internal set; }
    /// <summary>Collision half-extents in whole pixels, loaded from the timed frames; not the global Power Bomb blast radius.</summary>
    public Extent16<RoomPixels> Radius { get; internal set; }

    /// <summary>Whether the bomb is one of a bomb spread's five.</summary>
    public bool IsBombSpread { get; internal set; }

    /// <summary>The spread bomb's horizontal travel; a wall reverses its direction.</summary>
    public BombSpreadHorizontalVelocity BombSpreadHorizontal { get; internal set; }

    /// <summary>The spread bomb's signed 1/65536-pixel vertical velocity; Samus's gravity accelerates it every frame.</summary>
    public SignedSixteenSixteen BombSpreadVerticalVelocity { get; internal set; }

    /// <summary>The upward launch velocity, signed and in 1/65536 pixels, that a floor bounce restores.</summary>
    public SignedSixteenSixteen BombSpreadLaunchVelocity { get; internal set; }

    /// <summary>The bomb's fuse, and after detonation its explosion phase counter.</summary>
    public WrappingCountdown BombTimer { get; internal set; }

    /// <summary>Whether the bomb is live: a placed bomb runs its animation until it ends.</summary>
    public bool IsActive => Playback is not null;

    /// <summary>
    /// Ends the bomb: it reads as no bomb, at rest at the room's origin, to whatever still holds it and to anything that
    /// reads its place before the next bomb takes it.
    /// </summary>
    internal void End()
    {
        Position = FixedPosition<RoomPixels>.Zero;
        IsSpent = false;
        Type = SamusProjectileType.None;
        PicturedBeams = SamusBeamCombination.Power;
        HasReactedToTerrain = false;
        Damage = 0;
        Playback = null;
        FrameHold = WrappingCountdown.Start(0);
        Sprite = null;
        Radius = Extent16<RoomPixels>.Zero;
        BombTimer = WrappingCountdown.Start(0);
        IsBombSpread = false;
        BombSpreadHorizontal = default;
        BombSpreadVerticalVelocity = SignedSixteenSixteen.Zero;
        BombSpreadLaunchVelocity = SignedSixteenSixteen.Zero;
    }
}
