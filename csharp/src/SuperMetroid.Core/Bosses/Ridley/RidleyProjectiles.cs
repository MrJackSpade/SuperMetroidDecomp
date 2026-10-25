using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ridley's fireballs and the afterburn chain they spawn, shared by the Ceres and Lower Norfair fights
/// and reused by Mother Brain's afterburn.
/// </summary>
[RoomLifetime]
internal sealed class RidleyProjectiles
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
    /// <summary>OBJ palette row used for fireballs and every afterburn stage.</summary>
    private static readonly ushort FireballGraphicsIndex = EnemyPaletteBits.Palette5;
    /// <summary>The fireball's 8.8 speed: five pixels per update.</summary>
    private const ushort FireballSpeed = 1280;

    /// <summary>Ports Ridley instruction $A6:E84D and retains its asymmetric aim clamps.</summary>
    internal void CalculateRidleyFireballVelocity(RoomEnemySlot ridley, SamusState samus)
    {
        RidleyEnemyState state = _ridleyEncounter.RequireRidley(ridley);
        int muzzleX = ridley.Position.X.Whole + (state.FacingDirection == RidleyFacing.Left ? -25 : 25);
        int muzzleY = ridley.Position.Y.Whole - 43;
        byte angle = EnemyMath.CalculateBearing(
            new(unchecked((short)(samus.Position.X.Whole - muzzleX)), unchecked((short)(samus.Position.Y.Whole - muzzleY)))).ToTableAngle().AddTableUnits(SnesByteAngle.QuarterTurn.RawValue).RawValue;

        if (state.FacingDirection == RidleyFacing.Left)
        {
            if (angle is < 64 or >= 235)
                angle = 235;
            else if (angle < 176)
                angle = 176;
        }
        else
        {
            if (angle is < 21 or >= 192)
                angle = 21;
            else if (angle >= 80)
                angle = 80;
        }

        // Math_MultBySin/Cos at $86:C26C reads signed words from the shared table at
        // $A0:B443. The apparently relevant $94:A957 address is executable grapple
        // code, not table data; treating that code as samples produces enormous bogus
        // velocities. The native index is `angle * 2` because X is a byte offset into
        // 16-bit entries; callers add $40 themselves when they want cosine. Its multiply
        // takes the absolute table word,
        // shifts by eight, then reapplies the sign. Speed $0500 therefore remains a
        // signed 8.8 velocity whose magnitude never exceeds $0500.
        state.FireballVelocity = new EightEightVector<RoomPixels>(
            new SnesSignedEightEight(EnemyMath.MultiplySineSample(FireballSpeed, new SnesByteAngle(angle))),
            new SnesSignedEightEight(EnemyMath.MultiplySineSample(FireballSpeed, new SnesByteAngle(angle).AddTableUnits(SnesByteAngle.QuarterTurn.RawValue))));
    }

    /// <summary>Allocates projectile $86:9642 at Ridley's facing-dependent muzzle with its calculated velocity and <paramref name="afterburns"/> afterburn stages.</summary>
    internal void SpawnRidleyFireball(RoomEnemySlot ridley, ushort afterburns)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        RidleyEnemyState state = _ridleyEncounter.RequireRidley(ridley);
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.CeresRidleyFireball,
            FireballGraphicsIndex);
        projectile.StartAnimation(new RidleyFireballAnimation(RidleyFireballProgram.Fireball));
        ApplyRidleyProjectileAreaDamage(projectile);
        projectile.Position = projectile.Position.WithWhole(new(unchecked((ushort)(ridley.Position.X.Whole +
            (state.FacingDirection == RidleyFacing.Left ? -25 : 25))), unchecked((ushort)(ridley.Position.Y.Whole - 43))));
        projectile.Velocity = state.FireballVelocity;
        projectile.RemainingAfterburns = afterburns;
    }

    /// <summary>Selects the native damage row from Samus's current area, or Crateria for an unbound fixture.</summary>
    private void ApplyRidleyProjectileAreaDamage(RoomEnemyProjectileSlot projectile)
    {
        // EnemyMain binds live Samus before dispatch, including her current room
        // identity. Standalone fixtures without a room owner retain the default row.
        AreaId area = _deathsAndDrops.SamusForEnemyDrops?.LiquidPhysics.AreaIndex ?? AreaId.Crateria;
        projectile.Damage = RidleyProjectileDamageDefinitions.ForArea(area);
    }

    /// <summary>Allocates enemy projectile $E509 with one ROM-authored dust variant.</summary>
    internal void SpawnRidleyDust(Position16<RoomPixels> position, ushort variant)
    {
        RoomEnemyProjectileSlot? dust = _projectiles.AllocateEnemyProjectile();
        if (dust is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            dust,
            RoomEnemyProjectileKind.MiscDustExplosion,
            graphicsIndex: 0);
        dust.Position = dust.Position.WithWhole(position);
        dust.StartAnimation(new MiscDustProjectileAnimation(MiscDustProjectileDefinitions.Program(
            (MiscDustAnimation)Math.Min(variant, (ushort)MiscDustAnimation.BigExplosion))));
    }

    /// <summary>Creates the stationary impact center that later emits a perpendicular afterburn pair.</summary>
    internal void SpawnAfterburnCenter(
        RoomEnemyProjectileKind kind,
        Position16<RoomPixels> position,
        ushort remaining)
    {
        RoomEnemyProjectileSlot? center = _projectiles.AllocateEnemyProjectile();
        if (center is null)
            return;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(center, kind, FireballGraphicsIndex);
        center.StartAnimation(new RidleyFireballAnimation(kind == RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnCenter
            ? RidleyFireballProgram.VerticalCenter
            : RidleyFireballProgram.HorizontalCenter));
        center.Position = center.Position.WithWhole(position);
        // $86:9499/$949C explicitly stop the center; allocation retains prior velocities.
        center.Velocity = new(new SnesSignedEightEight(0), new SnesSignedEightEight(0));
        center.RemainingAfterburns = remaining;
    }

    /// <summary>Emits the left/right or up/down branches from an impact center.</summary>
    internal void SpawnAfterburnPair(RoomEnemyProjectileSlot center, Axis axis)
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
        RoomEnemyProjectileSlot source,
        RoomEnemyProjectileKind kind,
        EightEightVector<RoomPixels> velocity)
    {
        RoomEnemyProjectileSlot? afterburn = _projectiles.AllocateEnemyProjectile();
        if (afterburn is null)
            return;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(afterburn, kind, FireballGraphicsIndex);
        afterburn.StartAnimation(new RidleyFireballAnimation(RidleyFireballProgram.DirectionalAfterburn));
        ApplyRidleyProjectileAreaDamage(afterburn);
        afterburn.Position = afterburn.Position.WithWhole(source.Position.Whole);
        afterburn.Velocity = velocity;
        afterburn.RemainingAfterburns = source.RemainingAfterburns;
        afterburn.NextAfterburnKind = kind;
    }

    /// <summary>Decrements the native low-byte chain counter and repeats the branch while its sign bit remains clear.</summary>
    internal void SpawnNextAfterburn(RoomEnemyProjectileSlot source)
    {
        LowByteCountdownStep step = WrappingCounter.DecrementLowByte(checked((ushort)source.RemainingAfterburns));
        source.RemainingAfterburns = step.Remaining;
        if (step.IsNegative)
            return;
        SpawnDirectionalAfterburn(
            source,
            source.NextAfterburnKind,
            source.Velocity);
    }

    /// <summary>Switches a collided branch to its final five-frame program without clearing its velocity or damage fields.</summary>
    internal static void BeginAfterburnFinalAnimation(RoomEnemyProjectileSlot projectile)
    {
        projectile.StartAnimation(new RidleyFireballAnimation(RidleyFireballProgram.AfterburnFinal));
        // $86:950D/$9522 replace the list; its clear-pre-instruction stops motion.
        // Velocity words and contact properties remain unchanged. The final list
        // clears movement, draws five frames, then deletes; it never disables damage.
    }

    /// <summary>Registers collision and motion callbacks for fireballs and both perpendicular afterburn paths.</summary>
    internal void AddFireballPreInstructions(EnemyProjectileHandlerTable table)
    {
        table.AddPreInstruction(context =>
        {
            bool horizontalCollision = EnemyProjectileEngine.MoveProjectileAxis(context.Projectile, context.Level, axis: Axis.Horizontal);
            bool verticalCollision = !horizontalCollision &&
                EnemyProjectileEngine.MoveProjectileAxis(context.Projectile, context.Level, axis: Axis.Vertical);
            if (horizontalCollision || verticalCollision)
            {
                Position16<RoomPixels> impact = context.Projectile.Position.Whole;
                ushort count = checked((ushort)context.Projectile.RemainingAfterburns);
                context.Projectile.Clear();
                if (count != 0)
                {
                    SpawnAfterburnCenter(
                        horizontalCollision
                            ? RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnCenter
                            : RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnCenter,
                        impact,
                        count);
                }
            }
        }, EnemyProjectilePreInstruction.RidleyFireball);

        table.AddPreInstruction(context =>
        {
            // $86:950D first uses the raw 8.8 horizontal adder, not the room-collision
            // helper. Only the perpendicular vertical move may end this afterburn.
            context.Projectile.Position = context.Projectile.Position.AdvanceX(context.Projectile.Velocity.X.ToSixteenSixteenDelta());
            if (EnemyProjectileEngine.MoveProjectileAxis(context.Projectile, context.Level, axis: Axis.Vertical))
                BeginAfterburnFinalAnimation(context.Projectile);
        }, EnemyProjectilePreInstruction.HorizontalAfterburn);

        table.AddPreInstruction(context =>
        {
            // $86:9522 is the transposed path: unrestricted vertical travel followed
            // by a horizontal room-collision test.
            context.Projectile.Position = context.Projectile.Position.AdvanceY(context.Projectile.Velocity.Y.ToSixteenSixteenDelta());
            if (EnemyProjectileEngine.MoveProjectileAxis(context.Projectile, context.Level, axis: Axis.Horizontal))
                BeginAfterburnFinalAnimation(context.Projectile);
        }, EnemyProjectilePreInstruction.VerticalAfterburn);
    }

    /// <summary>Carries out the cues of the fireball and afterburn programs: motion, the afterburn spawns and deletion.</summary>
    internal void AddFireballCues(EnemyProjectileHandlerTable table)
    {
        table.AddCue<RidleyFireballCue>((context, cue) =>
        {
            RoomEnemyProjectileSlot projectile = context.Projectile;
            switch (cue)
            {
                case RidleyFireballCue.ClearPreInstruction:
                    projectile.PreInstruction = EnemyProjectilePreInstruction.RTS_868170;
                    return;
                case RidleyFireballCue.BeginFlight:
                    projectile.PreInstruction = EnemyProjectilePreInstruction.RidleyFireball;
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
                    projectile.Clear();
                    return;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(RidleyFireballCue)} {cue}.");
            }
        });
    }
}
