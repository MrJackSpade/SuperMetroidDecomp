using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ridley's fireballs and the afterburn chain they spawn, shared by the Ceres and Lower Norfair fights
/// and reused by Mother Brain's afterburn. It is the fireballs' and afterburns' projectile behaviour (#1801): their
/// flights and cues spawn the rest of the chain this spawns.
/// </summary>
[RoomLifetime]
internal sealed class RidleyProjectiles : EnemyProjectileMotionBehavior<RidleyFireballMotion>, IEnemyProjectileCues<RidleyFireballCue>
{
    /// <summary>How many afterburns each fireball and afterburn has left to raise, given as it spawns.</summary>
    internal EnemyProjectileStates<RidleyAfterburnChainState> Afterburns { get; } = new();

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

    /// <summary>Runs Ridley's fireball aim step and retains its asymmetric aim clamps.</summary>
    /// <param name="ridley">Ridley's body.</param>
    /// <param name="state">His body in flight, which faces the way he aims and keeps the aimed velocity.</param>
    /// <param name="samus">Samus, whom he aims at.</param>
    internal static void CalculateRidleyFireballVelocity(RoomEnemyPlace ridley, RidleyFlightState state, SamusState samus)
    {
        Position16<RoomPixels> muzzle = Muzzle(ridley, state);
        TrigonometryAngle angle = EnemyMath.CalculateAimAngle(
            samus.Position.Whole - muzzle).TurnedBy(TrigonometryAngle.QuarterTurn);

        // Each facing keeps its aim within its own arc; an aim beyond the arc's far end, up to the split, takes that
        // end, and any other aim outside it takes the near end.
        if (state.FacingDirection == RidleyFacing.Left)
        {
            if (angle < RidleyFireballDefinitions.LeftFacingAimSplit || angle >= RidleyFireballDefinitions.LeftFacingAimEnd)
                angle = RidleyFireballDefinitions.LeftFacingAimEnd;
            else if (angle < RidleyFireballDefinitions.LeftFacingAimStart)
                angle = RidleyFireballDefinitions.LeftFacingAimStart;
        }
        else
        {
            if (angle < RidleyFireballDefinitions.RightFacingAimStart || angle >= RidleyFireballDefinitions.RightFacingAimSplit)
                angle = RidleyFireballDefinitions.RightFacingAimStart;
            else if (angle >= RidleyFireballDefinitions.RightFacingAimEnd)
                angle = RidleyFireballDefinitions.RightFacingAimEnd;
        }

        // The speed is split along the aim, so neither part of the velocity exceeds the speed.
        state.FireballVelocity = new PixelVelocity<RoomPixels>(
            EnemyMath.SineSpeed(RidleyFireballDefinitions.FireballSpeed, angle),
            EnemyMath.SineSpeed(RidleyFireballDefinitions.FireballSpeed, angle.TurnedBy(TrigonometryAngle.QuarterTurn)));
    }

    /// <summary>Allocates Ridley's fireball at Ridley's facing-dependent muzzle with its calculated velocity and <paramref name="afterburns"/> afterburn stages.</summary>
    /// <param name="ridley">Ridley's body.</param>
    /// <param name="state">His body in flight, which faces the way he fires and keeps the aimed velocity.</param>
    /// <param name="afterburns">The afterburn stages the fireball's impact spawns.</param>
    internal void SpawnRidleyFireball(RoomEnemyPlace ridley, RidleyFlightState state, int afterburns)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.RidleyFireball, Fireballs, FireballGraphics);
        if (projectile is null)
            return;

        projectile.StartAnimation(new RidleyFireballAnimation(this, RidleyFireballSegment.Fireball));
        ApplyRidleyProjectileAreaDamage(projectile);
        projectile.Position = projectile.Position.WithWhole(Muzzle(ridley, state));
        projectile.Velocity = state.FireballVelocity;
        Afterburns.Start(projectile, new RidleyAfterburnChainState(afterburns));
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
        EnemyProjectile? dust = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.DustOrExplosion, DustExplosionProjectileBehavior.Instance, graphics: SpriteGraphics.SharedStandard);
        if (dust is null)
            return;

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
        EnemyProjectile? center = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.RidleyAfterburnCenter, Fireballs, FireballGraphics);
        if (center is null)
            return;
        // The centre's animation raises the spawn of the pair along its axis.
        center.StartAnimation(new RidleyFireballAnimation(this,
            pairAxis == Axis.Horizontal ? RidleyFireballSegment.HorizontalCenter : RidleyFireballSegment.VerticalCenter));
        center.Position = center.Position.WithWhole(position);
        // The afterburn's centre starts at rest; a new projectile otherwise keeps the velocity its place last held.
        center.Velocity = PixelVelocity<RoomPixels>.Zero;
        Afterburns.Start(center, new RidleyAfterburnChainState(remaining));
    }

    /// <summary>Emits the left/right or up/down branches from an impact center.</summary>
    internal void SpawnAfterburnPair(EnemyProjectile center, Axis axis)
    {
        if (axis == Axis.Horizontal)
        {
            SpawnDirectionalAfterburn(center, RoomEnemyProjectileKind.RidleyHorizontalAfterburn, RidleyAfterburnDefinitions.Right);
            SpawnDirectionalAfterburn(center, RoomEnemyProjectileKind.RidleyHorizontalAfterburn, RidleyAfterburnDefinitions.Left);
        }
        else
        {
            SpawnDirectionalAfterburn(center, RoomEnemyProjectileKind.RidleyVerticalAfterburn, RidleyAfterburnDefinitions.Up);
            SpawnDirectionalAfterburn(center, RoomEnemyProjectileKind.RidleyVerticalAfterburn, RidleyAfterburnDefinitions.Down);
        }
    }

    /// <summary>
    /// Spawns a directional afterburn of <paramref name="kind"/>, horizontal or vertical, at <paramref name="source"/>,
    /// flying at <paramref name="velocity"/>, which holds the way it flies, with the afterburns its source's chain has left.
    /// </summary>
    internal void SpawnDirectionalAfterburn(
        EnemyProjectile source,
        RoomEnemyProjectileKind kind,
        PixelVelocity<RoomPixels> velocity)
    {
        EnemyProjectile? afterburn = _projectiles.AllocateEnemyProjectile(kind, Fireballs, FireballGraphics);
        if (afterburn is null)
            return;
        afterburn.StartAnimation(new RidleyFireballAnimation(this, RidleyFireballSegment.DirectionalAfterburn));
        ApplyRidleyProjectileAreaDamage(afterburn);
        afterburn.Position = afterburn.Position.WithWhole(source.Position.Whole);
        afterburn.Velocity = velocity;
        Afterburns.Start(afterburn, new RidleyAfterburnChainState(Afterburns.Of(source).Remaining));
    }

    /// <summary>
    /// Continues the chain: an afterburn with afterburns left spends one and spawns the next, flying its own way at its own
    /// speed, which carries on with the rest; one with none left ends the chain. A chain of <c>n</c> therefore spawns
    /// <c>n</c> more after each of the centre's pair.
    /// </summary>
    internal void SpawnNextAfterburn(EnemyProjectile source)
    {
        RidleyAfterburnChainState chain = Afterburns.Of(source);
        if (chain.Remaining == 0)
            return;
        chain.Remaining--;
        SpawnDirectionalAfterburn(
            source,
            source.Kind,
            source.Velocity);
    }

    /// <summary>Switches an afterburn branch that struck something to its final animation: it stops, sparks and puffs while it still hurts Samus.</summary>
    internal void BeginAfterburnFinalAnimation(EnemyProjectile projectile)
    {
        projectile.StartAnimation(new RidleyFireballAnimation(this, RidleyFireballSegment.AfterburnFinal));
        // The final animation's first cue stops the afterburn, which keeps its velocity and still hurts Samus: it
        // shows five frames, then the afterburn is gone.
    }

    /// <summary>
    /// Ridley's fireballs, the afterburn centres they leave and the directional afterburns those spawn (#1760): their
    /// spawners start their animations, and each kind starts its own flight.
    /// </summary>
    internal RidleyProjectiles Fireballs => this;

    /// <summary>A fireball and the directional afterburns fly from their spawn; the afterburn centres hold still.</summary>
    private static readonly Dictionary<RoomEnemyProjectileKind, RidleyFireballMotion?> StartingMotions = new()
    {
        [RoomEnemyProjectileKind.RidleyFireball] = RidleyFireballMotion.FireballFlying,
        [RoomEnemyProjectileKind.RidleyHorizontalAfterburn] = RidleyFireballMotion.HorizontalAfterburnFlying,
        [RoomEnemyProjectileKind.RidleyVerticalAfterburn] = RidleyFireballMotion.VerticalAfterburnFlying,
        [RoomEnemyProjectileKind.RidleyAfterburnCenter] = null,
    };

    /// <summary>
    /// Moves a fireball, afterburn centre or directional afterburn: the fireball's flight and both afterburn flights.
    /// </summary>
    /// <param name="context">The projectile and what its pass hands every motion.</param>
    /// <param name="motion">The motion the projectile holds.</param>
    internal override void Move(EnemyProjectileMotionContext context, RidleyFireballMotion motion)
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
                projectile.StartMotion(this, RidleyFireballMotion.FireballFlying);
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
            int count = Afterburns.Of(projectile).Remaining;
            projectile.Delete();
            if (count != 0)
                SpawnAfterburnCenter(horizontalCollision ? Axis.Vertical : Axis.Horizontal, impact, count);
        }
    }
}