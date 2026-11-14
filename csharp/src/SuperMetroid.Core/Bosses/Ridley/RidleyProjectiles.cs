using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ridley's fireballs and the afterburn chain they spawn, shared by the Ceres and Lower Norfair fights
/// and reused by Mother Brain's afterburn. It is the fireballs' and afterburns' projectile behaviour (#1801): their
/// flights and cues spawn the rest of the chain this spawns.
/// </summary>
[RoomLifetime]
internal sealed class RidleyProjectiles : EnemyProjectileBehavior, IEnemyProjectileCues<RidleyFireballCue>
{
    /// <summary>Provides Samus's current area when selecting Ridley projectile damage.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Owns slots used by fireballs and successive afterburn projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Supplies Ridley's current facing and calculated muzzle velocity.</summary>
    private readonly RidleyEncounter _ridleyEncounter;

    /// <summary>Binds the shared Ceres and Lower Norfair fireball chain to encounter state and projectile slots.</summary>
    internal RidleyProjectiles(EnemyDeathsAndDrops deathsAndDrops, EnemyProjectileEngine projectiles, RidleyEncounter ridleyEncounter)
    {
        _deathsAndDrops = deathsAndDrops;
        _projectiles = projectiles;
        _ridleyEncounter = ridleyEncounter;
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
    internal void CalculateRidleyFireballVelocity(RoomEnemySlot ridley, SamusState samus)
    {
        RidleyEnemyState state = _ridleyEncounter.RequireRidley(ridley);
        int muzzleX = ridley.Position.X.Whole + (state.FacingDirection == RidleyFacing.Left ? -25 : 25);
        int muzzleY = ridley.Position.Y.Whole - 43;
        TrigonometryAngle angle = EnemyMath.CalculateBearing(
            new(CoordinateAxis.Difference(samus.Position.X.Whole, muzzleX), CoordinateAxis.Difference(samus.Position.Y.Whole, muzzleY))).ToTrigonometryAngle().TurnedBy(TrigonometryAngle.QuarterTurn);

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

        // The sine and cosine multiplies scale the speed by the game's signed sine of the angle in steps, the cosine
        // being the sine a quarter turn on. Each multiply takes the speed by the sine's size,
        // divides by 256, then reapplies the sign. Speed 1280 therefore remains a
        // signed 1/256-pixel velocity whose magnitude never exceeds 1280.
        state.FireballVelocity = new EightEightVector<RoomPixels>(
            EnemyMath.SineSpeed(FireballSpeed, angle),
            EnemyMath.SineSpeed(FireballSpeed, angle.TurnedBy(TrigonometryAngle.QuarterTurn)));
    }

    /// <summary>Allocates Ridley's fireball at Ridley's facing-dependent muzzle with its calculated velocity and <paramref name="afterburns"/> afterburn stages.</summary>
    internal void SpawnRidleyFireball(RoomEnemySlot ridley, int afterburns)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        RidleyEnemyState state = _ridleyEncounter.RequireRidley(ridley);
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.CeresRidleyFireball,
            Fireballs,
            FireballGraphics);
        projectile.StartAnimation(new RidleyFireballAnimation(RidleyFireballSegment.Fireball));
        ApplyRidleyProjectileAreaDamage(projectile);
        projectile.Position = projectile.Position.WithWhole(new(CoordinateAxis.Wrap(ridley.Position.X.Whole +
            (state.FacingDirection == RidleyFacing.Left ? -25 : 25)), CoordinateAxis.Wrap(ridley.Position.Y.Whole - 43)));
        projectile.Velocity = state.FireballVelocity;
        projectile.RemainingAfterburns = afterburns;
    }

    /// <summary>Selects the damage row from Samus's current area, or Crateria for an unbound fixture.</summary>
    private void ApplyRidleyProjectileAreaDamage(EnemyProjectile projectile)
    {
        // The enemy update binds live Samus before dispatch, including her current room
        // identity. Standalone fixtures without a room owner retain the default row.
        AreaId area = _deathsAndDrops.SamusForEnemyDrops?.LiquidPhysics.Area ?? AreaId.Crateria;
        projectile.Damage = RidleyProjectileDamage.ForArea(area);
    }

    /// <summary>Allocates the dust enemy projectile with one authored dust variant.</summary>
    internal void SpawnRidleyDust(Position16<RoomPixels> position, MiscDustAnimation animation)
    {
        EnemyProjectile? dust = _projectiles.AllocateEnemyProjectile();
        if (dust is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            dust,
            RoomEnemyProjectileKind.MiscDustExplosion,
            MiscDustProjectileBehavior.Instance,
            graphics: default);
        dust.Position = dust.Position.WithWhole(position);
        dust.StartAnimation(new MiscDustProjectileAnimation(MiscDustProjectiles.Animation(animation)));
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
            EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(center, RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnCenter,
                Fireballs, FireballGraphics);
            center.StartAnimation(new RidleyFireballAnimation(RidleyFireballSegment.HorizontalCenter));
        }
        else
        {
            EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(center, RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnCenter,
                Fireballs, FireballGraphics);
            center.StartAnimation(new RidleyFireballAnimation(RidleyFireballSegment.VerticalCenter));
        }
        center.Position = center.Position.WithWhole(position);
        // The afterburn centre's steps explicitly stop the center; allocation retains prior velocities.
        center.Velocity = EightEightVector<RoomPixels>.Zero;
        center.RemainingAfterburns = remaining;
    }

    /// <summary>Emits the left/right or up/down branches from an impact center.</summary>
    internal void SpawnAfterburnPair(EnemyProjectile center, Axis axis)
    {
        if (axis == Axis.Horizontal)
        {
            SpawnDirectionalAfterburn(center, RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnRight, CeresRidleyAfterburnDefinitions.Right);
            SpawnDirectionalAfterburn(center, RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnLeft, CeresRidleyAfterburnDefinitions.Left);
        }
        else
        {
            SpawnDirectionalAfterburn(center, RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnUp, CeresRidleyAfterburnDefinitions.Up);
            SpawnDirectionalAfterburn(center, RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnDown, CeresRidleyAfterburnDefinitions.Down);
        }
    }

    /// <summary>Copies the impact position, damage, velocity, and remaining chain count into a directional branch.</summary>
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
        afterburn.RemainingAfterburns = source.RemainingAfterburns;
        afterburn.NextAfterburnKind = kind;
    }

    /// <summary>
    /// Continues the chain: an afterburn with afterburns left spends one and spawns the next, which carries on with the
    /// rest; one with none left ends the chain. A chain of <c>n</c> therefore spawns <c>n</c> more after each of the
    /// centre's pair.
    /// </summary>
    internal void SpawnNextAfterburn(EnemyProjectile source)
    {
        if (source.RemainingAfterburns == 0)
            return;
        source.RemainingAfterburns--;
        SpawnDirectionalAfterburn(
            source,
            source.NextAfterburnKind,
            source.Velocity);
    }

    /// <summary>Switches a collided branch to its final five-frame animation without clearing its velocity or damage fields.</summary>
    internal static void BeginAfterburnFinalAnimation(EnemyProjectile projectile)
    {
        projectile.StartAnimation(new RidleyFireballAnimation(RidleyFireballSegment.AfterburnFinal));
        // The afterburns replace the animation; its clear-frame-behaviour cue stops motion.
        // Velocity and contact properties remain unchanged. The final animation
        // clears movement, draws five frames, then deletes; it never disables damage.
    }

    /// <summary>
    /// Ridley's fireballs, the afterburn centres they leave and the directional afterburns those spawn (#1760): their
    /// spawners start their animations, and each kind's definition names its flight.
    /// </summary>
    internal EnemyProjectileBehavior Fireballs => this;

    /// <summary>
    /// Moves a fireball, afterburn centre or directional afterburn: the fireball's flight and both afterburn flights.
    /// </summary>
    /// <param name="context">The projectile and what its pass hands every frame behaviour.</param>
    private protected override void Move(EnemyProjectileFrameBehaviorContext context)
    {
        EnemyProjectile projectile = context.Projectile;
        EnemyProjectileFrameBehavior motion = projectile.FrameBehavior;
        if (motion == EnemyProjectileFrameBehavior.RidleyFireballFlight)
        {
            FlyFireball(projectile, context.Level);
        }
        else if (motion == EnemyProjectileFrameBehavior.HorizontalAfterburnFlight)
        {
            // The horizontal afterburn first uses the raw 1/256-pixel horizontal adder, not the room-collision
            // helper. Only the perpendicular vertical move may end this afterburn.
            projectile.Position = projectile.Position.AdvanceX(projectile.Velocity.X.ToSixteenSixteenDelta());
            if (EnemyProjectileEngine.MoveProjectileAxis(projectile, context.Level, axis: Axis.Vertical))
                BeginAfterburnFinalAnimation(projectile);
        }
        else if (motion == EnemyProjectileFrameBehavior.VerticalAfterburnFlight)
        {
            // The vertical afterburn is the transposed path: unrestricted vertical travel followed
            // by a horizontal room-collision test.
            projectile.Position = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta());
            if (EnemyProjectileEngine.MoveProjectileAxis(projectile, context.Level, axis: Axis.Horizontal))
                BeginAfterburnFinalAnimation(projectile);
        }
        else
        {
            base.Move(context);
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
            case RidleyFireballCue.ClearFrameBehavior:
                projectile.FrameBehavior = EnemyProjectileFrameBehavior.Stationary;
                return;
            case RidleyFireballCue.BeginFlight:
                projectile.FrameBehavior = EnemyProjectileFrameBehavior.RidleyFireballFlight;
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
            int count = projectile.RemainingAfterburns;
            projectile.Delete();
            if (count != 0)
                SpawnAfterburnCenter(horizontalCollision ? Axis.Vertical : Axis.Horizontal, impact, count);
        }
    }
}