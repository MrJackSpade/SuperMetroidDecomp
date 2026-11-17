using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ridley's fireballs and the afterburn chain they spawn, shared by the Ceres and Lower Norfair fights
/// and reused by Mother Brain's afterburn. It is the fireballs' and afterburns' projectile behaviour (#1801): their
/// flights and cues spawn the rest of the chain this spawns.
/// </summary>
[RoomLifetime]
internal sealed class RidleyProjectiles : EnemyProjectileMotionBehavior<RidleyFireballMotion>, IEnemyProjectileCues<RidleyFireballCue>
{
    /// <summary>The loaded room, whose area selects Ridley's projectile damage.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns places used by fireballs and successive afterburn projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Binds the shared Ceres and Lower Norfair fireball chain to the room's projectiles.</summary>
    internal RidleyProjectiles(EnemyRoomBinding room, EnemyProjectileEngine projectiles)
        : base(StartingMotions)
    {
        _room = room;
        _projectiles = projectiles;
    }
    /// <summary>Sprite colour set used for fireballs and every afterburn stage.</summary>
    private static readonly SpriteGraphics FireballGraphics = SpriteGraphics.SharedIn(EnemyShades.InProjectileLook);
    /// <summary>The fireball's 1/256-pixel speed: five pixels per update.</summary>
    private static readonly SignedEightEight FireballSpeed = SignedEightEight.FromPixels(5);

    /// <summary>The start of the arc a left-facing Ridley aims his fireballs within.</summary>
    private static readonly TrigonometryAngle LeftFacingAimStart = new(176);

    /// <summary>The end of the arc a left-facing Ridley aims his fireballs within; the clamp keeps this end itself.</summary>
    private static readonly TrigonometryAngle LeftFacingAimEnd = new(235);

    /// <summary>Below this a left-facing Ridley's aim clamps to the arc's end rather than its start: a quarter turn.</summary>
    private static readonly TrigonometryAngle LeftFacingAimSplit = new(64);

    /// <summary>The start of the arc a right-facing Ridley aims his fireballs within.</summary>
    private static readonly TrigonometryAngle RightFacingAimStart = new(21);

    /// <summary>The end of the arc a right-facing Ridley aims his fireballs within; the clamp keeps this end itself.</summary>
    private static readonly TrigonometryAngle RightFacingAimEnd = new(80);

    /// <summary>From this a right-facing Ridley's aim clamps to the arc's start rather than its end: three quarters of a turn.</summary>
    private static readonly TrigonometryAngle RightFacingAimSplit = new(192);

    /// <summary>Runs Ridley's fireball aim step and retains its asymmetric aim clamps.</summary>
    /// <param name="ridley">Ridley's body.</param>
    /// <param name="state">His body in flight, which faces the way he aims and keeps the aimed velocity.</param>
    /// <param name="samus">Samus, whom he aims at.</param>
    internal static void CalculateRidleyFireballVelocity(RoomEnemyPlace ridley, RidleyFlightState state, SamusState samus)
    {
        Position16<RoomPixels> muzzle = Muzzle(ridley, state);
        TrigonometryAngle angle = EnemyMath.CalculateBearing(
            samus.Position.Whole - muzzle).ToTrigonometryAngle().TurnedBy(TrigonometryAngle.QuarterTurn);

        // Each facing keeps its aim within its own arc; an aim beyond the arc's far end, up to the split, takes that
        // end, and any other aim outside it takes the near end.
        if (state.FacingDirection == RidleyFacing.Left)
        {
            if (angle < LeftFacingAimSplit || angle >= LeftFacingAimEnd)
                angle = LeftFacingAimEnd;
            else if (angle < LeftFacingAimStart)
                angle = LeftFacingAimStart;
        }
        else
        {
            if (angle < RightFacingAimStart || angle >= RightFacingAimSplit)
                angle = RightFacingAimStart;
            else if (angle >= RightFacingAimEnd)
                angle = RightFacingAimEnd;
        }

        // The speed is scaled by the game's sine and cosine of the aim, the cosine being the sine a quarter turn on:
        // each takes the speed by the sine's size in 256ths and keeps the sine's direction, so neither part of the
        // velocity ever exceeds the speed of 1280/256 of a pixel an update.
        state.FireballVelocity = new EightEightVector<RoomPixels>(
            EnemyMath.SineSpeed(FireballSpeed, angle),
            EnemyMath.SineSpeed(FireballSpeed, angle.TurnedBy(TrigonometryAngle.QuarterTurn)));
    }

    /// <summary>Allocates Ridley's fireball at Ridley's facing-dependent muzzle with its calculated velocity and <paramref name="afterburns"/> afterburn stages.</summary>
    /// <param name="ridley">Ridley's body.</param>
    /// <param name="state">His body in flight, which faces the way he fires and keeps the aimed velocity.</param>
    /// <param name="afterburns">The afterburn stages the fireball's impact spawns.</param>
    internal void SpawnRidleyFireball(RoomEnemyPlace ridley, RidleyFlightState state, int afterburns)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.RidleyFireball,
            Fireballs,
            FireballGraphics);
        projectile.StartAnimation(new RidleyFireballAnimation(RidleyFireballSegment.Fireball));
        ApplyRidleyProjectileAreaDamage(projectile);
        projectile.Position = projectile.Position.WithWhole(Muzzle(ridley, state));
        projectile.Velocity = state.FireballVelocity;
        projectile.State = new RidleyAfterburnChainState(afterburns);
    }

    /// <summary>Ridley's mouth for the way he faces: facing right or the screen mirrors his left-facing mouth.</summary>
    private static Position16<RoomPixels> Muzzle(RoomEnemyPlace ridley, RidleyFlightState state) =>
        ridley.Position.Whole + (state.FacingDirection == RidleyFacing.Left
            ? RidleyFireballDefinitions.MuzzleFacingLeft
            : RidleyFireballDefinitions.MuzzleFacingLeft.MirroredX());

    /// <summary>Ridley's fireball and afterburn damage in the loaded room's area.</summary>
    private void ApplyRidleyProjectileAreaDamage(EnemyProjectile projectile) =>
        projectile.Damage = RidleyProjectileDamage.ForArea(_room.Area);

    /// <summary>Allocates the dust enemy projectile with one authored dust variant.</summary>
    internal void SpawnRidleyDust(Position16<RoomPixels> position, DustExplosionAnimation animation)
    {
        EnemyProjectile? dust = _projectiles.AllocateEnemyProjectile();
        if (dust is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            dust,
            RoomEnemyProjectileKind.DustOrExplosion,
            DustExplosionProjectileBehavior.Instance,
            graphics: default);
        dust.Position = dust.Position.WithWhole(position);
        dust.StartAnimation(new DustExplosionProjectileAnimation(animation));
    }

    /// <summary>
    /// Creates the stationary impact center that later emits an afterburn pair flying along <paramref name="pairAxis"/>:
    /// a horizontal centre's pair flies left and right, a vertical centre's up and down.
    /// </summary>
    internal void SpawnAfterburnCenter(
        Axis pairAxis,
        Position16<RoomPixels> position,
        int remaining)
    {
        EnemyProjectile? center = _projectiles.AllocateEnemyProjectile();
        if (center is null)
            return;
        if (pairAxis == Axis.Horizontal)
        {
            EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(center, RoomEnemyProjectileKind.RidleyHorizontalAfterburnCenter,
                Fireballs, FireballGraphics);
            center.StartAnimation(new RidleyFireballAnimation(RidleyFireballSegment.HorizontalCenter));
        }
        else
        {
            EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(center, RoomEnemyProjectileKind.RidleyVerticalAfterburnCenter,
                Fireballs, FireballGraphics);
            center.StartAnimation(new RidleyFireballAnimation(RidleyFireballSegment.VerticalCenter));
        }
        center.Position = center.Position.WithWhole(position);
        // The afterburn's centre starts at rest; a new projectile otherwise keeps the velocity its place last held.
        center.Velocity = EightEightVector<RoomPixels>.Zero;
        center.State = new RidleyAfterburnChainState(remaining);
    }

    /// <summary>Emits the left/right or up/down branches from an impact center.</summary>
    internal void SpawnAfterburnPair(EnemyProjectile center, Axis axis)
    {
        if (axis == Axis.Horizontal)
        {
            SpawnDirectionalAfterburn(center, RoomEnemyProjectileKind.RidleyHorizontalAfterburnRight, RidleyAfterburnDefinitions.Right);
            SpawnDirectionalAfterburn(center, RoomEnemyProjectileKind.RidleyHorizontalAfterburnLeft, RidleyAfterburnDefinitions.Left);
        }
        else
        {
            SpawnDirectionalAfterburn(center, RoomEnemyProjectileKind.RidleyVerticalAfterburnUp, RidleyAfterburnDefinitions.Up);
            SpawnDirectionalAfterburn(center, RoomEnemyProjectileKind.RidleyVerticalAfterburnDown, RidleyAfterburnDefinitions.Down);
        }
    }

    /// <summary>Spawns a directional afterburn of <paramref name="kind"/> at <paramref name="source"/>, flying at <paramref name="velocity"/>, with the afterburns its source's chain has left.</summary>
    internal void SpawnDirectionalAfterburn(
        EnemyProjectile source,
        RoomEnemyProjectileKind kind,
        EightEightVector<RoomPixels> velocity)
    {
        EnemyProjectile? afterburn = _projectiles.AllocateEnemyProjectile();
        if (afterburn is null)
            return;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(afterburn, kind, Fireballs, FireballGraphics);
        afterburn.StartAnimation(new RidleyFireballAnimation(RidleyFireballSegment.DirectionalAfterburn));
        ApplyRidleyProjectileAreaDamage(afterburn);
        afterburn.Position = afterburn.Position.WithWhole(source.Position.Whole);
        afterburn.Velocity = velocity;
        afterburn.State = new RidleyAfterburnChainState(source.StateOf<RidleyAfterburnChainState>().Remaining);
    }

    /// <summary>
    /// Continues the chain: an afterburn with afterburns left spends one and spawns the next, flying its own way at its own
    /// speed, which carries on with the rest; one with none left ends the chain. A chain of <c>n</c> therefore spawns
    /// <c>n</c> more after each of the centre's pair.
    /// </summary>
    internal void SpawnNextAfterburn(EnemyProjectile source)
    {
        RidleyAfterburnChainState chain = source.StateOf<RidleyAfterburnChainState>();
        if (chain.Remaining == 0)
            return;
        chain.Remaining--;
        SpawnDirectionalAfterburn(
            source,
            source.Kind,
            source.Velocity);
    }

    /// <summary>Switches an afterburn branch that struck something to its final five-frame animation; it keeps moving and still hurts Samus.</summary>
    internal static void BeginAfterburnFinalAnimation(EnemyProjectile projectile)
    {
        projectile.StartAnimation(new RidleyFireballAnimation(RidleyFireballSegment.AfterburnFinal));
        // The final animation's first cue stops the afterburn, which keeps its velocity and still hurts Samus: it
        // shows five frames, then the afterburn is gone.
    }

    /// <summary>
    /// Ridley's fireballs, the afterburn centres they leave and the directional afterburns those spawn (#1760): their
    /// spawners start their animations, and each kind starts its own flight.
    /// </summary>
    internal EnemyProjectileBehavior Fireballs => this;

    /// <summary>A fireball and the directional afterburns fly from their spawn; the afterburn centres hold still.</summary>
    private static readonly Dictionary<RoomEnemyProjectileKind, RidleyFireballMotion?> StartingMotions = new()
    {
        [RoomEnemyProjectileKind.RidleyFireball] = RidleyFireballMotion.FireballFlying,
        [RoomEnemyProjectileKind.RidleyHorizontalAfterburnRight] = RidleyFireballMotion.HorizontalAfterburnFlying,
        [RoomEnemyProjectileKind.RidleyHorizontalAfterburnLeft] = RidleyFireballMotion.HorizontalAfterburnFlying,
        [RoomEnemyProjectileKind.RidleyVerticalAfterburnUp] = RidleyFireballMotion.VerticalAfterburnFlying,
        [RoomEnemyProjectileKind.RidleyVerticalAfterburnDown] = RidleyFireballMotion.VerticalAfterburnFlying,
        [RoomEnemyProjectileKind.RidleyHorizontalAfterburnCenter] = null,
        [RoomEnemyProjectileKind.RidleyVerticalAfterburnCenter] = null,
    };

    /// <summary>
    /// Moves a fireball, afterburn centre or directional afterburn: the fireball's flight and both afterburn flights.
    /// </summary>
    /// <param name="context">The projectile and what its pass hands every motion.</param>
    /// <param name="motion">The motion the projectile holds.</param>
    private protected override void Move(EnemyProjectileMotionContext context, RidleyFireballMotion motion)
    {
        EnemyProjectile projectile = context.Projectile;
        switch (motion)
        {
            case RidleyFireballMotion.FireballFlying:
                FlyFireball(projectile, context.Level);
                break;
            case RidleyFireballMotion.HorizontalAfterburnFlying:
                // The horizontal afterburn moves sideways through the room's blocks; only its vertical move may meet
                // the room and end it.
                projectile.Position = projectile.Position.AdvanceX(projectile.Velocity.X.ToSixteenSixteenDelta());
                if (EnemyProjectileEngine.MoveProjectileAxis(projectile, context.Level, axis: Axis.Vertical))
                    BeginAfterburnFinalAnimation(projectile);
                break;
            case RidleyFireballMotion.VerticalAfterburnFlying:
                // The vertical afterburn moves up or down through the room's blocks; only its sideways move may meet
                // the room and end it.
                projectile.Position = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta());
                if (EnemyProjectileEngine.MoveProjectileAxis(projectile, context.Level, axis: Axis.Horizontal))
                    BeginAfterburnFinalAnimation(projectile);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(RidleyFireballMotion)} {motion}.");
        }
    }

    /// <summary>
    /// Carries out a cue of the fireball and afterburn animations: motion, the afterburn spawns and deletion.
    /// </summary>
    /// <param name="context">The projectile, Samus and the camera of the pass running the animation.</param>
    /// <param name="cue">The cue the animation published.</param>
    public void RunCue(EnemyProjectileCueContext context, RidleyFireballCue cue)
    {
        EnemyProjectile projectile = context.Projectile;
        switch (cue)
        {
            case RidleyFireballCue.StopMoving:
                projectile.HoldStill();
                return;
            case RidleyFireballCue.BeginFlight:
                projectile.StartMotion(RidleyFireballMotion.FireballFlying);
                return;
            case RidleyFireballCue.SpawnHorizontalAfterburns:
                SpawnAfterburnPair(projectile, axis: Axis.Horizontal);
                return;
            case RidleyFireballCue.SpawnVerticalAfterburns:
                SpawnAfterburnPair(projectile, axis: Axis.Vertical);
                return;
            case RidleyFireballCue.SpawnNextAfterburn:
                SpawnNextAfterburn(projectile);
                return;
            case RidleyFireballCue.Delete:
                projectile.Delete();
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(RidleyFireballCue)} {cue}.");
        }
    }

    /// <summary>
    /// Flies a fireball with horizontal, then vertical, room collision. One that strikes the room ends; with afterburns
    /// left, it leaves the centre whose pair flies along the wall it struck.
    /// </summary>
    private void FlyFireball(EnemyProjectile projectile, RoomLevelData level)
    {
        bool horizontalCollision = EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal);
        bool verticalCollision = !horizontalCollision &&
            EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical);
        if (horizontalCollision || verticalCollision)
        {
            Position16<RoomPixels> impact = projectile.Position.Whole;
            int count = projectile.StateOf<RidleyAfterburnChainState>().Remaining;
            projectile.Delete();
            if (count != 0)
                SpawnAfterburnCenter(horizontalCollision ? Axis.Vertical : Axis.Horizontal, impact, count);
        }
    }
}