using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Artwork;
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// One of Samus's bombs: a placed bomb, a Power Bomb, or a bomb-spread bomb. Each is created fresh when it takes its
/// place among the five (<see cref="SamusBombProjectileSystem"/>) and ends when it is deleted or Samus's bombs are
/// cleared; nothing it holds reaches the next bomb in its place. Once it ends it reads as no bomb, with no type, no
/// damage and at rest at the room's origin, until the next bomb takes its place.
/// </summary>
public sealed class SamusBomb
{
    /// <summary>Creates a bomb, not yet placed, in <paramref name="place"/> among the five places.</summary>
    internal SamusBomb(int place) => Place = place;

    /// <summary>The bomb's place among the five, zero through four: bombs take the lowest free one, and a spread fills all five.</summary>
    public int Place { get; }

    /// <summary>Room position, to a fraction of a pixel.</summary>
    public FixedPosition<RoomPixels> Position { get; internal set; }

    /// <summary>
    /// Whether the bomb has already struck an enemy: its explosion keeps running, but it no longer launches Samus and
    /// later enemy collision skips it.
    /// </summary>
    public bool IsSpent { get; private set; }

    /// <summary>The bomb strikes an enemy (#2562): its explosion runs on, but it no longer launches Samus or meets enemies.</summary>
    internal void Spend() => IsSpent = true;

    /// <summary>The bomb's explosion passes through an enemy as though it had struck nothing (#2562).</summary>
    internal void FlyOn() => IsSpent = false;

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
    /// <summary>
    /// The bomb's animation (#1600, #2458): the frame it shows, the updates that frame has left and where its play stands;
    /// null while no bomb is placed here.
    /// </summary>
    internal AnimationPlayer<SamusProjectileSegment, SamusProjectileFrameId, NoAnimationCue>? Animation { get; set; }
    /// <summary>The bomb's animation and where it stands in it; null while no bomb is placed here.</summary>
    internal AnimationPlayback<SamusProjectileSegment>? Playback => Animation?.Playback;
    /// <summary>The composition the bomb shows, selected by the timed-frame animation or installed frame binding; none before its first frame.</summary>
    public ProjectileSpriteId? Sprite { get; internal set; }
    /// <summary>Collision half-extents in whole pixels, loaded from the timed frames; not the global Power Bomb blast radius.</summary>
    public Extent16<RoomPixels> Radius { get; internal set; }

    /// <summary>
    /// The flight of a bomb spread's bomb (#2459), created when the spread places it; null for a placed bomb or a Power
    /// Bomb, which do not fly.
    /// </summary>
    public BombSpreadFlight? SpreadFlight { get; internal set; }

    /// <summary>Whether the bomb is one of a bomb spread's five.</summary>
    public bool IsBombSpread => SpreadFlight is not null;

    /// <summary>
    /// The bomb's fuse (#2458): started when the bomb is placed, counted once per update while it burns, and over from the
    /// update it burns out, where nothing counts it again.
    /// </summary>
    public UpdateCountdown Fuse { get; internal set; } = UpdateCountdown.Ended();

    /// <summary>Whether the bomb's fuse has burned out: a normal bomb explodes, and a Power Bomb's blast has begun.</summary>
    public bool HasBurnedOut => Fuse.IsOver;

    /// <summary>Whether the bomb is live: a placed bomb runs its animation until it ends.</summary>
    public bool IsActive => Animation is not null;

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
        Animation = null;
        Sprite = null;
        Radius = Extent16<RoomPixels>.Zero;
        Fuse = UpdateCountdown.Ended();
        SpreadFlight = null;
    }
}
