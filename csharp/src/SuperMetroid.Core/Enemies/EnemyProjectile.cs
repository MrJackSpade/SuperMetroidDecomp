using SuperMetroid.Core.Enemies.Projectiles;

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
    /// pixel. Room pixels for every family except the few frame behaviours that deliberately keep screen-relative
    /// positions here.
    /// </summary>
    public FixedPosition<RoomPixels> Position { get; internal set; }
    /// <summary>How far the projectile moves each update, in 256ths of a pixel, for the shared movement helpers and the families that move by it.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }
    /// <summary>
    /// What the projectile leaves in its place for the next projectile spawned there: its whole position and velocity
    /// as they stand when that spawn takes the place (<see cref="EnemyProjectileHandOver"/>).
    /// </summary>
    internal EnemyProjectileHandOver LeftInPlace => new(Position.Whole, Velocity);
    /// <summary>The typed animation the projectile plays (#641), or null when it plays none.</summary>
    internal EnemyProjectileAnimation? Animation { get; private set; }
    /// <summary>Gets the countdown to the animation's next frame; a spawn starts it at one so the animation's first frame shows on its first pass.</summary>
    public WrappingCountdown FrameHold { get; internal set; }
    /// <summary>The frame the projectile shows: the blank frame from its spawn until its animation shows one, none once it is gone.</summary>
    public EnemyProjectileFrameId? Frame { get; internal set; }
    /// <summary>How the projectile moves itself on each pass, before its animation advances.</summary>
    internal EnemyProjectileFrameBehavior FrameBehavior { get; set; }
    /// <summary>Gets the sheet and shading the projectile draws with, as its spawner supplied them.</summary>
    public SpriteGraphics Graphics { get; internal set; }
    /// <summary>Gets the collision half-extents in pixels, from the projectile's definition.</summary>
    public Extent16<RoomPixels> Radius { get; internal set; }
    /// <summary>Gets the contact damage from the projectile's definition, before Samus's suit reduction.</summary>
    public int Damage
    {
        get;
        internal set => field = Ensure.AtLeastZero(value);
    }
    /// <summary>Gets the invincibility timer written to Samus on a damaging collision; definition initialization uses ninety-six frames.</summary>
    public int InvincibilityFrames
    {
        get;
        internal set => field = Ensure.AtLeastZero(value);
    }
    /// <summary>
    /// Whether the projectile is drawn in front of or behind Samus. Its definition sets the starting depth; Torizo's
    /// animations move it in front. This is independent of the projectile's depth against the room's tiles.
    /// </summary>
    public EnemyProjectileDepth Depth { get; internal set; }
    /// <summary>
    /// The count the projectile's animation repeats its counted loops by: a set-counter step loads it and a
    /// repeat-while-counting step counts it down. This is not the frame hold, and only those steps change it.
    /// </summary>
    public WrappingCountdown LoopCountdown { get; internal set; }
    /// <summary>
    /// How many more afterburns of Ridley's fireball or Mother Brain's bomb this one's chain spawns after it: each
    /// directional afterburn hands its successor one fewer, and one that has none left spawns no successor.
    /// </summary>
    public int RemainingAfterburns
    {
        get;
        internal set => field = Ensure.AtLeastZero(value);
    }
    /// <summary>The kind of the next directional afterburn the chain spawns.</summary>
    internal RoomEnemyProjectileKind NextAfterburnKind { get; set; }
    /// <summary>What the projectile's family keeps on it (#1470), given by its spawner; none until then.</summary>
    internal EnemyProjectileState? State { get; set; }

    /// <summary>The state the projectile's family gave it, read as that family's own type.</summary>
    /// <typeparam name="TState">The family's state type.</typeparam>
    /// <exception cref="InvalidOperationException">The projectile holds no state, or another family's.</exception>
    internal TState StateOf<TState>() where TState : EnemyProjectileState =>
        State as TState ?? throw new InvalidOperationException(
            $"Enemy projectile {Kind} in place {Place} holds {State?.GetType().Name ?? "no state"}, not {typeof(TState).Name}.");

    /// <summary>Gets whether this actor participates in damaging Samus-contact checks, independently of persistence and shot-blocking properties.</summary>
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
    /// <summary>The enemy a death explosion or its drop came from, and whether the explosion rebuilds it when it ends.</summary>
    public KilledEnemy KilledEnemy { get; internal set; }
    /// <summary>
    /// The enemy whose drops a death explosion scatters: the explosion reads that enemy's
    /// drop chances when it finishes.
    /// </summary>
    public EnemyDefinitionId DropDefinitionId { get; internal set; }
    /// <summary>The chance table a boss-selected drop rolls against instead of its enemy definition's, or null.</summary>
    internal EnemyDropChances? DropChancesOverride { get; set; }

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
    /// Frees the projectile's place without ending the projectile, for a power-bombed Draygon shot
    /// (<see cref="PowerBombedDraygonShotFinishesItsUpdate"/>): a later spawn may take the place, while the update that
    /// freed it goes on with the projectile, which keeps its animation, state and properties.
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
        FrameBehavior = EnemyProjectileFrameBehavior.Stationary;
        Frame = null;
        FrameHold = WrappingCountdown.Start(0);
        Animation = null;
        Radius = Extent16<RoomPixels>.Zero;
        Graphics = SpriteGraphics.SharedStandard;
        Damage = InvincibilityFrames = 0;
        LoopCountdown = WrappingCountdown.Start(0);
        Depth = EnemyProjectileDepth.BehindSamus;
        RemainingAfterburns = 0;
        NextAfterburnKind = RoomEnemyProjectileKind.None;
        State = null;
        CollisionOption = EnemyProjectileCollisionOption.RunShotList;
        KilledEnemy = default;
        DropDefinitionId = EnemyDefinitionId.None;
        DropChancesOverride = null;
        CanDamageSamus = PersistsOnSamusContact = BlocksSamusProjectiles = false;
    }
}
