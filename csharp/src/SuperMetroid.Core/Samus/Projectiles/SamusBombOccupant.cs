using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The bomb holding one of Samus's bomb places (#2558, #2560): what it is and the damage it deals, where it lies, its fuse,
/// what it shows and, for a bomb spread's bomb, its flight, from its placing until it ends. Only a held place has an
/// occupant, so nothing of a bomb stands for an empty place: a bomb that ends takes all it holds with it.
/// </summary>
[ArchitectureReview("back-reference", "The bomb holding a place and the place it holds (#2558): the place's taking creates the occupant, and the place's queries read the occupant, so the two share the bomb's lifetime.")]
public sealed partial class SamusBombOccupant
{
    /// <summary>The bomb of <paramref name="kind"/> dealing <paramref name="damage"/> taking <paramref name="bomb"/>'s place.</summary>
    /// <param name="bomb">The place the bomb holds, with its position, fuse and animation.</param>
    /// <param name="kind">What the bomb is.</param>
    /// <param name="damage">The damage the bomb deals, from its kind's bomb definition.</param>
    internal SamusBombOccupant(SamusBomb bomb, SamusBombKind kind, int damage)
    {
        Bomb = Ensure.NotNull(bomb);
        Kind = kind;
        Type = TypeOf[kind];
        Damage = Ensure.AtLeastZero(damage);
    }

    /// <summary>The projectile type of each kind of bomb.</summary>
    private static IdentityTable<SamusBombKind, SamusProjectileType> TypeOf { get; } = new(new Dictionary<SamusBombKind, SamusProjectileType>
    {
        [SamusBombKind.Bomb] = SamusProjectileType.Of(SamusProjectileFamily.Bomb),
        [SamusBombKind.PowerBomb] = SamusProjectileType.Of(SamusProjectileFamily.PowerBomb),
    });

    /// <summary>What the bomb is: a Morph Ball bomb or a Power Bomb.</summary>
    public SamusBombKind Kind { get; }

    /// <summary>The damage the bomb deals, from its family's bomb definition.</summary>
    public int Damage { get; }

    /// <summary>The place the bomb holds.</summary>
    public SamusBomb Bomb { get; }

    /// <summary>The projectile type of the bomb's kind, which its contacts strike with.</summary>
    public SamusProjectileType Type { get; }

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

    /// <summary>
    /// The beam combination whose pictures the bomb and its explosion show (#1586): the beams Samus had equipped when
    /// she placed it, whatever she equips while it burns.
    /// </summary>
    public SamusBeamCombination PicturedBeams { get; internal set; }

    /// <summary>Whether a placed bomb's explosion has reacted with the five-block cross of terrain around it; it does so once.</summary>
    public bool HasReactedToTerrain { get; internal set; }

    /// <summary>
    /// The bomb's animation (#1600, #2458): the frame it shows, the updates that frame has left and where its play stands;
    /// null until its placing starts it.
    /// </summary>
    internal AnimationPlayer<SamusProjectileSegment, SamusProjectileFrameId, NoAnimationCue>? Animation { get; set; }

    /// <summary>The bomb's animation and where it stands in it; null until its placing starts it.</summary>
    internal AnimationPlayback<SamusProjectileSegment>? Playback => Animation?.Playback;

    /// <summary>Whether the bomb is live: a placed bomb runs its animation until it ends.</summary>
    public bool IsAnimated => Animation is not null;

    /// <summary>The composition the bomb shows, selected by the timed-frame animation or installed frame binding; none before its first frame.</summary>
    public ProjectileSpriteId? Sprite { get; internal set; }

    /// <summary>Collision half-extents in whole pixels, loaded from the timed frames; not the global Power Bomb blast radius.</summary>
    public Extent16<RoomPixels> Radius { get; internal set; }

    /// <summary>
    /// The flight of a bomb spread's bomb (#2459), created when the spread places it; null for a placed bomb or a Power
    /// Bomb, which do not fly.
    /// </summary>
    public BombSpreadFlight? SpreadFlight { get; internal set; }

    /// <summary>
    /// The bomb's fuse (#2458): started when the bomb is placed, counted once per update while it burns, and over from the
    /// update it burns out, where nothing counts it again.
    /// </summary>
    public UpdateCountdown Fuse { get; internal set; } = UpdateCountdown.Ended();
}
