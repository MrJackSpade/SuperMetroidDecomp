using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Artwork;
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// One place in Samus's shot pool and the ordinary projectile holding it: a beam, missile, special-attack particle, echo or
/// explosion. Each place is created fresh when a projectile takes it (<see cref="SamusShotPool.Occupy"/>), and the
/// projectile holding it (<see cref="Occupant"/>, #2558) owns all its state, from its firing, release or setting off until
/// it ends, when it is deleted, replaced, or Samus's shots are cleared. A free place holds nothing of a projectile (#2560):
/// what it reads as is the cleared place's, no projectile at rest at the room's origin, which the blocks and enemies a
/// shot's update goes on to reach after the shot ended read. Nothing a projectile holds reaches the next projectile in its
/// place but two values gameplay carries over: its horizontal position's fraction of a pixel, which is
/// <see cref="LeftBeamStartFromLastShot"/>'s, and, for a beam fired with every place held over the shot in the last place,
/// the count that shot leaves (<see cref="BeamFiredOverLastShot"/>).
/// </summary>
public sealed partial class SamusProjectile
{
    /// <summary>Creates the free <paramref name="place"/> of the shot pool.</summary>
    internal SamusProjectile(int place) => Place = place;

    /// <summary>
    /// The projectile's place in the shot pool, 0 through 4. Gameplay reads the place: shots take the lowest free one and
    /// update and draw from the highest down, a special attack's particles take the first four, the crash echoes the
    /// last two, and a Super Missile names its link's place (<see cref="SamusShotPool"/>).
    /// </summary>
    [IntegerQuantity("ordinal of the projectile's place in Samus's shot pool")]
    public int Place { get; }

    /// <summary>
    /// The projectile holding the place (#2558, #2560), which owns all it is and does: from its firing, release or setting
    /// off (<see cref="Occupy"/>) until it ends; null while the place holds no projectile.
    /// </summary>
    public SamusProjectileOccupant? Occupant { get; private set; }

    /// <summary>
    /// Whether the projectile holds its place: from the update it takes the place with its type until it ends. The
    /// firing and the Super Missile's link take the lowest place no projectile holds.
    /// </summary>
    public bool IsOccupied => Occupant is not null;

    /// <summary>
    /// Takes the place with a projectile of <paramref name="type"/> dealing <paramref name="damage"/> at
    /// <paramref name="position"/>, as its firing, release or setting off does: the damage is the one its type deals.
    /// </summary>
    /// <param name="type">What the projectile is.</param>
    /// <param name="damage">The damage the projectile deals, one or more.</param>
    /// <param name="position">Where the projectile starts.</param>
    /// <returns>The projectile now holding the place.</returns>
    /// <exception cref="InvalidOperationException">A projectile already holds the place.</exception>
    internal SamusProjectileOccupant Occupy(SamusProjectileType type, int damage, FixedPosition<RoomPixels> position)
    {
        if (Occupant is { } held)
            throw new InvalidOperationException($"Samus's projectile place {Place} already holds a {held.Type}.");
        Occupant = new SamusProjectileOccupant(this, type, damage, position);
        return Occupant;
    }

    /// <summary>
    /// The beam combination whose pictures the projectile holding the place shows (<see cref="SamusProjectileOccupant.PicturedBeams"/>);
    /// the Power beam's for a free place.
    /// </summary>
    public SamusBeamCombination PicturedBeams => Occupant is { } held ? held.PicturedBeams : SamusBeamCombination.Power;

    /// <summary>
    /// The direction the projectile holding the place was fired or reflected in (<see cref="SamusProjectileOccupant.Direction"/>);
    /// the cleared place's for a free place.
    /// </summary>
    public SamusProjectileDirection Direction => Occupant is { } held ? held.Direction : default;

    /// <summary>
    /// Whether the projectile holding the place has already struck something (<see cref="SamusProjectileOccupant.IsSpent"/>);
    /// a free place has struck nothing.
    /// </summary>
    public bool IsSpent => Occupant is { IsSpent: true };

    /// <summary>
    /// The room position of the projectile holding the place, to a fraction of a pixel; the room's origin for a free place.
    /// </summary>
    public FixedPosition<RoomPixels> Position => Occupant is { } held ? held.Position : FixedPosition<RoomPixels>.Zero;

    /// <summary>The velocity of the projectile holding the place, in pixels per update; none for a free place.</summary>
    public PixelVelocity<RoomPixels> Velocity => Occupant is { } held ? held.Velocity : default;

    /// <summary>The collision half-extents of the projectile holding the place, in pixels; none for a free place.</summary>
    public Extent16<RoomPixels> Radius => Occupant is { } held ? held.Radius : Extent16<RoomPixels>.Zero;

    /// <summary>
    /// The animation of the projectile holding the place (<see cref="SamusProjectileOccupant.Animation"/>); null while it has
    /// none to animate or the place is free.
    /// </summary>
    internal AnimationPlayer<SamusProjectileSegment, SamusProjectileFrameId, NoAnimationCue>? Animation => Occupant?.Animation;

    /// <summary>The animation of the projectile holding the place and where it stands in it; null while it has none or the place is free.</summary>
    internal AnimationPlayback<SamusProjectileSegment>? Playback => Occupant?.Playback;

    /// <summary>The composition the projectile holding the place shows; none before its first frame or for a free place.</summary>
    public ProjectileSpriteId? Sprite => Occupant?.Sprite;

    /// <summary>
    /// Counts one update of the flight of the shot holding the place toward its next trail
    /// (<see cref="SamusProjectileOccupant.CountsDownToTrail"/>); a free place leaves none.
    /// </summary>
    /// <param name="interval">The updates from this trail to the next, at least one.</param>
    internal bool CountsDownToTrail(int interval) => Occupant is { } held && held.CountsDownToTrail(interval);

    /// <summary>
    /// What the projectile holding the place does on its update (<see cref="SamusProjectileOccupant.Behavior"/>); null for an
    /// explosion, before the projectile is set up, and for a free place.
    /// </summary>
    internal SamusShotBehavior? Behavior => Occupant?.Behavior;

    /// <summary>
    /// The Power Bomb fuse the beam holding the place carries (<see cref="SamusProjectileOccupant.BeamFuse"/>); none for a
    /// free place.
    /// </summary>
    internal UpdateCountdown BeamFuse => Occupant is { } held ? held.BeamFuse : UpdateCountdown.Ended();

    /// <summary>
    /// What a beam fired into this projectile's place finds there (#2524), from which <see cref="BeamFiredOverLastShot"/>
    /// gives the beam its Power Bomb fuse: a free place once the projectile has ended, or its kind with the ignition,
    /// ignition speed, link place, departure angle or carried fuse that kind keeps. A beam, the Hyper Beam's shot and their
    /// explosions keep no kind state and carry their fuse; a missile's explosion keeps its missile's.
    /// </summary>
    /// <exception cref="InvalidOperationException">The projectile keeps an undefined ignition.</exception>
    internal BeamFiredOverLastShot.ReplacedShot AsReplacedByBeam => Occupant switch
    {
        null => new BeamFiredOverLastShot.ReplacedShot.FreePlace(),
        { KindState: { } state } => state.ReplacedByBeam,
        { } held => new BeamFiredOverLastShot.ReplacedShot.Beam(held.BeamFuse.UpdatesLeft),
    };

    /// <summary>The state the kind of the projectile holding the place keeps, which must be a <typeparamref name="TState"/>.</summary>
    /// <exception cref="InvalidOperationException">No projectile holds the place, or the one holding it keeps no such state.</exception>
    internal TState KindStateAs<TState>() where TState : SamusShotKindState => Occupant switch
    {
        { KindState: TState state } => state,
        { } held => throw new InvalidOperationException(
            $"The projectile in place {Place} keeps no {typeof(TState).Name}; it keeps {held.KindState?.GetType().Name ?? "nothing"}."),
        null => throw new InvalidOperationException($"No projectile holds place {Place}, so none keeps a {typeof(TState).Name}."),
    };

    /// <summary>Whether a projectile holds the place with an animation to play, which marks it drawn and updated.</summary>
    public bool IsActive => Occupant is { IsAnimated: true };

    /// <summary>
    /// Enemy collision admits a beam, missile, or Super Missile even when it has no animation to play. This
    /// distinction is observable for the left-facing Murder Beam, whose type and damage remain live while it has none.
    /// </summary>
    public bool HasEnemyCollisionPayload => Occupant is { HasEnemyCollisionPayload: true };

    /// <summary>
    /// The type the projectile strikes blocks with: its own while it holds its place, otherwise the cleared place's
    /// (<see cref="Compatibility.ClearedShotPlaceType"/>), for a block its update reaches after it ended.
    /// </summary>
    internal SamusProjectileType StrikingType =>
        Compatibility.ClearedShotPlaceType.Striking(Occupant, shot => shot.Type, SamusProjectileType.Beam(SamusBeamCombination.Power, charged: false));

    /// <summary>
    /// Ends the projectile: it no longer holds its place, which reads as the cleared place to whatever still holds it and to
    /// anything that reads it before the next projectile takes it.
    /// </summary>
    internal void End() => Occupant = null;
}
