using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.Projectiles.Artwork;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// One enemy projectile, created fresh in a free enemy-projectile place each time one is spawned (#1590). It holds its
/// own state: nothing an earlier projectile in its place held reaches it, except the whole position and velocity that
/// projectile left there (<see cref="EnemyProjectileHandOver"/>), which a spawn that does not set them starts from.
/// Until a room spawns into a place, the place holds a projectile that is already gone, standing where the room before
/// left that place's projectile.
/// </summary>
public sealed class EnemyProjectile
{
    /// <summary>
    /// Starts a projectile in <paramref name="place"/> from what the place's previous projectile left there: its whole
    /// position and its velocity. Everything else starts afresh, and the projectile is gone until its spawner gives it a kind.
    /// </summary>
    /// <param name="place">The zero-based enemy-projectile place the projectile takes.</param>
    /// <param name="left">What the place's previous projectile left there.</param>
    internal EnemyProjectile(int place, EnemyProjectileHandOver left)
    {
        Place = Ensure.BetweenInclusive(0, EnemyProjectileEngine.PlaceCount - 1, place);
        Position = FixedPosition<RoomPixels>.FromWhole(left.Position);
        Velocity = left.Velocity;
        Vacate();
    }

    /// <summary>
    /// The enemy-projectile place the projectile was spawned into, zero for the last place spawns search. Gameplay reads
    /// it only where the place decides what happens: an explosion in the first place leaves no pickup
    /// (<see cref="FirstPlaceExplosionLeavesNoPickup"/>) and Botwoon's dying segments fall by place
    /// (<see cref="BotwoonSegmentFallDelay"/>).
    /// </summary>
    public int Place { get; }
    /// <summary>What the projectile is, or <see cref="RoomEnemyProjectileKind.None"/> once it is gone or has freed its place.</summary>
    public RoomEnemyProjectileKind Kind { get; internal set; }
    /// <summary>Gets whether the projectile holds its place and takes part in the projectile passes.</summary>
    public bool IsActive => Kind != RoomEnemyProjectileKind.None;
    /// <summary>
    /// Where the projectile is, in whole room pixels and fractions of a pixel, to which its motion adds in 256ths of a
    /// pixel. Every projectile is placed in the room and drawn where its place lies on screen, the Mother Brain death
    /// subtitle's fixed place included (#1851).
    /// </summary>
    public FixedPosition<RoomPixels> Position { get; internal set; }
    /// <summary>How far the projectile moves each update, for the shared movements and the families that move by it.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }
    /// <summary>
    /// What the projectile leaves in its place for the next projectile spawned there: its whole position and velocity
    /// as they stand when that spawn takes the place (<see cref="EnemyProjectileHandOver"/>).
    /// </summary>
    internal EnemyProjectileHandOver LeftInPlace => new(Position.Whole, Velocity);
    /// <summary>The typed animation the projectile plays (#641), or null when it plays none.</summary>
    internal EnemyProjectileAnimation? Animation { get; private set; }
    /// <summary>
    /// Gets the countdown to the animation's next frame; a spawn starts it at one so the animation's first frame shows on its
    /// first pass. Setting it runs a stopped hold again (<see cref="IsFrameHoldStopped"/>).
    /// </summary>
    public WrappingCountdown FrameHold
    {
        get;
        internal set
        {
            field = value;
            IsFrameHoldStopped = false;
        }
    }

    /// <summary>
    /// Whether the frame hold is stopped (#2380): the animation shows a frame for good, or rests where its timeline holds, so
    /// the hold counts nothing and the animation takes no further step until the projectile's owner sets the hold again or
    /// starts an animation (<see cref="StopFrameHold"/>).
    /// </summary>
    public bool IsFrameHoldStopped { get; private set; }

    /// <summary>
    /// Stops the frame hold (#2380): the frame shown stays, and the animation takes no further step, until the projectile's
    /// owner sets the hold again or starts an animation. The hold reads zero while it is stopped.
    /// </summary>
    internal void StopFrameHold()
    {
        FrameHold = WrappingCountdown.Start(0);
        IsFrameHoldStopped = true;
    }
    /// <summary>The frame the projectile shows: the blank frame from its spawn until its animation shows one, none once it is gone.</summary>
    public EnemyProjectileFrameId? Frame { get; internal set; }
    /// <summary>
    /// How the projectile moves itself on each pass, before its animation advances (#1843): one of the motions its
    /// behaviour owns (<see cref="EnemyProjectileMotionBehavior{TMotion}"/>), or null while it holds still. Its behaviour starts
    /// it at spawn and changes it; a shot that destroys the projectile stops it.
    /// </summary>
    internal Enum? Motion { get; set; }
    /// <summary>
    /// What the projectile's kind does (#1760): it moves the projectile each update, carries out its animations'
    /// cues and names its shot and touch animations. The spawner binds it with the kind; none until then. A projectile
    /// that ends keeps it, so the cues its animation publishes on the way out still reach their owner.
    /// </summary>
    internal EnemyProjectileBehavior? Behavior { get; set; }
    /// <summary>Gets the sheet and shading the projectile draws its own pictures with, as its spawner supplied them.</summary>
    public SpriteGraphics Graphics { get; internal set; }

    /// <summary>
    /// The sheet and shading the frame the projectile shows draws with (#2260): a room picture, such as the burst a destroyed
    /// projectile shows, draws from the room's shared sheet in its standard colours; any other frame with
    /// <see cref="Graphics"/>.
    /// </summary>
    public SpriteGraphics ShownGraphics => Frame is { } frame && EnemyProjectileRoomPictureDefinitions.RoomPictures.Contains(frame)
        ? SpriteGraphics.SharedStandard : Graphics;
    /// <summary>Gets the collision half-extents in pixels, from the projectile's definition.</summary>
    public Extent16<RoomPixels> Radius { get; internal set; }
    /// <summary>Gets the contact damage from the projectile's definition, before Samus's suit reduction.</summary>
    public int Damage
    {
        get;
        internal set => field = Ensure.AtLeastZero(value);
    }
    /// <summary>
    /// Whether the projectile is drawn in front of or behind Samus. Its definition sets the starting depth; Torizo's
    /// animations move it in front. This is independent of the projectile's depth against the room's tiles.
    /// </summary>
    public EnemyProjectileDepth Depth { get; internal set; }
    /// <summary>What the projectile's family keeps on it (#1470), given by its spawner; none until then.</summary>
    internal EnemyProjectileState? State { get; set; }

    /// <summary>The state the projectile's family gave it, read as that family's own type.</summary>
    /// <typeparam name="TState">The family's state type.</typeparam>
    /// <exception cref="InvalidOperationException">The projectile holds no state, or another family's.</exception>
    internal TState StateOf<TState>() where TState : EnemyProjectileState =>
        State as TState ?? throw new InvalidOperationException(
            $"Enemy projectile {Kind} in place {Place} holds {State?.GetType().Name ?? "no state"}, not {typeof(TState).Name}.");

    /// <summary>Gets whether this actor damages Samus when it touches her, whether or not it survives contact or stops shots.</summary>
    public bool CanDamageSamus { get; internal set; }
    /// <summary>Contact does not delete this actor.</summary>
    public bool PersistsOnSamusContact { get; internal set; }
    /// <summary>Samus's shots test this actor for collision.</summary>
    public bool BlocksSamusProjectiles { get; internal set; }
    /// <summary>
    /// How Samus's shots that reach the projectile act on it: they play its shot animation, meet an indestructible dud, or are
    /// not tested at all. This is deliberately separate from <see cref="BlocksSamusProjectiles"/>: Kago enables that
    /// property only after its bug has moved far enough from the source shell, while still playing its shot animation.
    /// </summary>
    public EnemyProjectileCollisionOption CollisionOption { get; internal set; }

    /// <summary>
    /// Makes the projectile <paramref name="kind"/> with <paramref name="behavior"/> bound to it for its life (#1760), in
    /// the motion the behaviour starts that kind with (#1843).
    /// </summary>
    internal void Bind(RoomEnemyProjectileKind kind, EnemyProjectileBehavior behavior)
    {
        Kind = kind;
        Behavior = Ensure.NotNull(behavior);
        Motion = behavior.InitialMotion(kind);
    }

    /// <summary>Moves the projectile by <paramref name="motion"/>, one of its behaviour's motions, from its next pass.</summary>
    /// <typeparam name="TMotion">The behaviour's motions.</typeparam>
    internal void StartMotion<TMotion>(TMotion motion) where TMotion : struct, Enum => Motion = motion;

    /// <summary>Stops the projectile moving itself: it holds still while its animation plays, or its owner positions it.</summary>
    internal void HoldStill() => Motion = null;

    /// <summary>Runs <paramref name="animation"/> from its first frame on the next pass, in place of any animation it plays.</summary>
    internal void StartAnimation(EnemyProjectileAnimation animation)
    {
        Animation = Ensure.NotNull(animation);
        FrameHold = WrappingCountdown.Start(1);
    }

    /// <summary>
    /// Replaces the running animation with <paramref name="animation"/> without touching the frame timer, as a
    /// direct animation change does: the new segment's first frame shows when the current frame expires.
    /// </summary>
    internal void InstallAnimation(EnemyProjectileAnimation animation) => Animation = Ensure.NotNull(animation);

    /// <summary>
    /// Frees the projectile's place without ending the projectile, for a Draygon goop or wall-turret shot a power bomb's
    /// explosion catches (#1590): a later spawn may take the place, while the update that freed it goes on with the
    /// projectile, which keeps its animation, state and properties. The caught shot finishes the update it was caught in:
    /// it still moves, a flying goop still tests Samus and, on reaching her, sticks to her and slows her again after the
    /// explosion has cleared her slowdown, and its animation still runs this pass; where it moved to is what it leaves in
    /// its place for the next projectile spawned there (<see cref="EnemyProjectileHandOver"/>). From the next pass on it
    /// takes no part in the game.
    /// </summary>
    internal void FreePlace() => Kind = RoomEnemyProjectileKind.None;

    /// <summary>
    /// Ends the projectile: it frees its place, shows and runs nothing more, and keeps none of its family's state. Its
    /// whole position and velocity stay where it left them, for the next projectile spawned into its place
    /// (<see cref="EnemyProjectileHandOver"/>).
    /// </summary>
    internal void Delete()
    {
        Position = Position.WithFractionX(0).WithFractionY(0);
        Vacate();
    }

    /// <summary>Sets everything but the position and velocity to what a projectile holds before its spawner sets it up.</summary>
    private void Vacate()
    {
        Kind = RoomEnemyProjectileKind.None;
        Motion = null;
        Frame = null;
        FrameHold = WrappingCountdown.Start(0);
        Animation = null;
        Radius = Extent16<RoomPixels>.Zero;
        Graphics = SpriteGraphics.SharedStandard;
        Damage = 0;
        Depth = EnemyProjectileDepth.BehindSamus;
        State = null;
        CollisionOption = EnemyProjectileCollisionOption.PlaysShotAnimation;
        CanDamageSamus = PersistsOnSamusContact = BlocksSamusProjectiles = false;
    }
}
