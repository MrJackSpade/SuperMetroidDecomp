using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Artwork;
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// One of Samus's five bomb places (<see cref="SamusBombProjectileSystem"/>) and the bomb holding it: a placed bomb, a Power
/// Bomb, or a bomb-spread bomb. Each place is created fresh when a bomb takes it, and the bomb holding it
/// (<see cref="Occupant"/>, #2558) owns all its state until it is deleted or Samus's bombs are cleared. A free place holds
/// nothing of a bomb (#2560): what it reads as is the cleared place's, no bomb at rest at the room's origin, until the next
/// bomb takes it.
/// </summary>
public sealed class SamusBomb
{
    /// <summary>Creates the free place <paramref name="place"/> among the five.</summary>
    internal SamusBomb(int place) => Place = place;

    /// <summary>The bomb's place among the five, zero through four: bombs take the lowest free one, and a spread fills all five.</summary>
    public int Place { get; }

    /// <summary>
    /// The bomb holding the place (#2558, #2560), which owns all it is and does: from its placing, which gives it its type
    /// (<see cref="Occupy"/>), until it ends; null while the place holds no bomb.
    /// </summary>
    public SamusBombOccupant? Occupant { get; private set; }

    /// <summary>The bomb holding the place, for a step that changes it.</summary>
    /// <exception cref="InvalidOperationException">No bomb holds the place.</exception>
    public SamusBombOccupant Held =>
        Occupant ?? throw new InvalidOperationException($"No bomb holds Samus's bomb place {Place}.");

    /// <summary>Whether a bomb holds the place: from its placing, which gives it its type, until it ends.</summary>
    public bool IsOccupied => Occupant is not null;

    /// <summary>
    /// Takes the place with a bomb of <paramref name="type"/> dealing <paramref name="damage"/>, as its placing does: the
    /// damage is the one its family's bomb definition gives.
    /// </summary>
    /// <param name="type">What the bomb is.</param>
    /// <param name="damage">The damage the bomb deals.</param>
    /// <returns>The bomb now holding the place.</returns>
    /// <exception cref="InvalidOperationException">A bomb already holds the place.</exception>
    internal SamusBombOccupant Occupy(SamusProjectileType type, int damage)
    {
        if (Occupant is { } held)
            throw new InvalidOperationException($"Samus's bomb place {Place} already holds a {held.Type}.");
        Occupant = new SamusBombOccupant(this, type, damage);
        return Occupant;
    }

    /// <summary>The room position of the bomb holding the place, to a fraction of a pixel; the room's origin for a free place.</summary>
    /// <exception cref="InvalidOperationException">Set while no bomb holds the place.</exception>
    public FixedPosition<RoomPixels> Position
    {
        get => Occupant is { } held ? held.Position : FixedPosition<RoomPixels>.Zero;
        internal set => Held.Position = value;
    }

    /// <summary>Whether the bomb holding the place has already struck an enemy (<see cref="SamusBombOccupant.IsSpent"/>); a free place has not.</summary>
    public bool IsSpent => Occupant is { IsSpent: true };

    /// <summary>The bomb holding the place strikes an enemy (#2562).</summary>
    /// <exception cref="InvalidOperationException">No bomb holds the place.</exception>
    internal void Spend() => Held.Spend();

    /// <summary>The explosion of the bomb holding the place passes through an enemy as though it had struck nothing (#2562).</summary>
    /// <exception cref="InvalidOperationException">No bomb holds the place.</exception>
    internal void FlyOn() => Held.FlyOn();

    /// <summary>
    /// The beam combination whose pictures the bomb holding the place shows (<see cref="SamusBombOccupant.PicturedBeams"/>);
    /// the Power beam's for a free place.
    /// </summary>
    /// <exception cref="InvalidOperationException">Set while no bomb holds the place.</exception>
    public SamusBeamCombination PicturedBeams
    {
        get => Occupant is { } held ? held.PicturedBeams : SamusBeamCombination.Power;
        internal set => Held.PicturedBeams = value;
    }

    /// <summary>
    /// Whether the explosion of the bomb holding the place has reacted with the terrain around it
    /// (<see cref="SamusBombOccupant.HasReactedToTerrain"/>); a free place has not.
    /// </summary>
    /// <exception cref="InvalidOperationException">Set while no bomb holds the place.</exception>
    public bool HasReactedToTerrain
    {
        get => Occupant is { HasReactedToTerrain: true };
        internal set => Held.HasReactedToTerrain = value;
    }

    /// <summary>The animation of the bomb holding the place (<see cref="SamusBombOccupant.Animation"/>); null for a free place.</summary>
    /// <exception cref="InvalidOperationException">Set while no bomb holds the place.</exception>
    internal AnimationPlayer<SamusProjectileSegment, SamusProjectileFrameId, NoAnimationCue>? Animation
    {
        get => Occupant?.Animation;
        set => Held.Animation = value;
    }

    /// <summary>The animation of the bomb holding the place and where it stands in it; null for a free place.</summary>
    internal AnimationPlayback<SamusProjectileSegment>? Playback => Occupant?.Playback;

    /// <summary>The composition the bomb holding the place shows; none before its first frame or for a free place.</summary>
    /// <exception cref="InvalidOperationException">Set while no bomb holds the place.</exception>
    public ProjectileSpriteId? Sprite
    {
        get => Occupant?.Sprite;
        internal set => Held.Sprite = value;
    }

    /// <summary>The collision half-extents of the bomb holding the place, in whole pixels; none for a free place.</summary>
    /// <exception cref="InvalidOperationException">Set while no bomb holds the place.</exception>
    public Extent16<RoomPixels> Radius
    {
        get => Occupant is { } held ? held.Radius : Extent16<RoomPixels>.Zero;
        internal set => Held.Radius = value;
    }

    /// <summary>
    /// The flight of the bomb spread's bomb holding the place (<see cref="SamusBombOccupant.SpreadFlight"/>); null for a placed
    /// bomb, a Power Bomb or a free place.
    /// </summary>
    /// <exception cref="InvalidOperationException">Set while no bomb holds the place.</exception>
    public BombSpreadFlight? SpreadFlight
    {
        get => Occupant?.SpreadFlight;
        internal set => Held.SpreadFlight = value;
    }

    /// <summary>Whether the bomb holding the place is one of a bomb spread's five.</summary>
    public bool IsBombSpread => SpreadFlight is not null;

    /// <summary>The fuse of the bomb holding the place (<see cref="SamusBombOccupant.Fuse"/>); over for a free place.</summary>
    /// <exception cref="InvalidOperationException">Set while no bomb holds the place.</exception>
    public UpdateCountdown Fuse
    {
        get => Occupant is { } held ? held.Fuse : UpdateCountdown.Ended();
        internal set => Held.Fuse = value;
    }

    /// <summary>Whether the bomb's fuse has burned out: a normal bomb explodes, and a Power Bomb's blast has begun.</summary>
    public bool HasBurnedOut => Fuse.IsOver;

    /// <summary>Whether a bomb holds the place and is live: a placed bomb runs its animation until it ends.</summary>
    public bool IsActive => Occupant is { IsAnimated: true };

    /// <summary>
    /// Ends the bomb: it no longer holds its place, which reads as the cleared place to whatever still holds it and to
    /// anything that reads it before the next bomb takes it.
    /// </summary>
    internal void End() => Occupant = null;
}
