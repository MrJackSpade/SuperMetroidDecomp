using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// Combat and death half of <see cref="NorfairRidleyAudit"/>. Keeping this separate from
/// room/header/reveal validation makes each failure land in a small debugger-friendly file
/// while both halves still run from the single <c>--norfair-ridley-audit</c> command.
/// </summary>
internal static partial class NorfairRidleyAudit
{
    private const ushort RidleyBreakupDefinition = 0xe1bf;

    /// <summary>
    /// Drives a fresh retail encounter through every collision family added for the real
    /// boss, then kills him by the authored zero-health lunge/grab route. Synthetic shots
    /// only supply deterministic projectile position and damage; hitboxes, vulnerability,
    /// callbacks, state changes, animations, and persistence all remain cartridge-backed.
    /// </summary>
    private static RidleyBattleAuditResult VerifyCombatDamageAndDeath(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        bool bossDefeated = false;
        var random = new Bank80SystemState(0x4d21);
        var samus = CreateCombatSamus(bus);
        var enemies = new RoomEnemySystem();
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        enemies.Load(
            bus,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            random.NextRandom,
            random.SetRandomNumber,
            readRandomNumber: () => random.RandomNumber,
            level: assets.LevelData,
            samus: samus,
            isAreaBossDefeated: () => bossDefeated,
            setAreaBossDefeated: () => bossDefeated = true,
            cameraX: CameraX,
            cameraY: CameraY);

        RoomEnemySlot body = enemies.Slots[0];
        RidleyEnemyState state = enemies.Ridley ??
            throw new InvalidDataException("Combat fixture did not allocate Ridley's shared state.");
        AdvanceFreshRidleyToCombat(enemies, assets.LevelData, samus, state);

        // Body contact must come from the current extended spritemap, not the tiny dummy
        // eight-pixel header radius. Search a compact body-local grid so the audit remains
        // valid across legitimate wing/body instruction changes at the combat handoff.
        (short bodyHitOffsetX, short bodyHitOffsetY) = FindRidleyBodyContactPoint(
            bus,
            enemies,
            body,
            samus);

        // Tail contact is resolved before the body walk and owns a distinct 120-damage
        // word in Lower Norfair. Placing Samus at the solved tip proves that ordering and
        // avoids silently substituting the body's 160-point definition damage.
        ResetSamusForContact(bus, samus);
        RidleyTailSegment tailTip = state.TailSegments[6];
        samus.XPosition = tailTip.XPosition;
        samus.YPosition = tailTip.YPosition;
        ushort tailHealthBefore = samus.Health;
        if (!enemies.ResolveRidleySamusContact(samus, controllerInput: 0) ||
            tailHealthBefore - samus.Health != state.TailDamage ||
            state.TailDamage != 120)
        {
            throw new InvalidDataException(
                $"Ridley tail contact mismatch: health={tailHealthBefore}->{samus.Health}, " +
                $"damage={state.TailDamage}, tip=({tailTip.XPosition},{tailTip.YPosition}).");
        }

        // $A6:E088 treats the final two tail joints as armor. A real beam-family word has
        // its sign bit set; on collision it is snapped to the newly solved joint, marked
        // with direction bit $10, and replaced by a bank-$86 dust actor without touching HP.
        var tailShots = new SamusProjectileSystem();
        SamusProjectileSlot tailShot = tailShots.Slots[0];
        ArmRidleyProjectile(
            tailShot,
            tailTip.XPosition,
            tailTip.YPosition,
            type: 0x8000,
            damage: 200,
            radius: 12);
        ushort healthBeforeTailShot = body.Health;
        int dustBeforeTailShot = CountActiveRidleyDust(enemies);
        StepRidley(enemies, assets.LevelData, samus, tailShots);
        tailTip = state.TailSegments[6];
        if ((tailShot.Direction & 0x0010) == 0 ||
            tailShot.XPosition != tailTip.XPosition ||
            tailShot.YPosition != tailTip.YPosition ||
            body.Health != healthBeforeTailShot ||
            CountActiveRidleyDust(enemies) <= dustBeforeTailShot)
        {
            throw new InvalidDataException(
                $"Ridley tail armor mismatch: direction=${tailShot.Direction:X4}, " +
                $"shot=({tailShot.XPosition},{tailShot.YPosition}), " +
                $"tip=({tailTip.XPosition},{tailTip.YPosition}), " +
                $"health={healthBeforeTailShot}->{body.Health}, " +
                $"dust={dustBeforeTailShot}->{CountActiveRidleyDust(enemies)}.");
        }

        // Ridley's twelve beam vulnerability bytes are all `$80`: an ordinary beam still
        // collides with the active extended rectangle and becomes an impact, but its low
        // seven-bit multiplier is zero. Assert that immune path before using the genuinely
        // vulnerable missile entry (`$82`) for the hurt/palette proof below.
        var immuneBeam = new SamusProjectileSystem();
        var sharedProjectiles = new SamusBombProjectileSystem();
        ArmRidleyProjectile(
            immuneBeam.Slots[0],
            unchecked((ushort)(body.XPosition + bodyHitOffsetX)),
            unchecked((ushort)(body.YPosition + bodyHitOffsetY)),
            type: 0x8000,
            damage: 400,
            radius: 4);
        ushort healthBeforeImmuneBeam = body.Health;
        int immuneBeamHits = enemies.ResolveOrdinaryProjectileHits(
            bus,
            immuneBeam,
            sharedProjectiles,
            samus);
        if (immuneBeamHits != 1 || body.Health != healthBeforeImmuneBeam ||
            body.FlashTimer != 0 || (body.AiHandlerBits & 0x0002) != 0 ||
            body.Properties.HasAny(EnemyProperties.Deleted))
        {
            throw new InvalidDataException(
                $"Ridley immune-beam reaction mismatch: hits={immuneBeamHits}, " +
                $"health={healthBeforeImmuneBeam}->{body.Health}, flash={body.FlashTimer}, " +
                $"AI=${body.AiHandlerBits:X4}, properties=${body.Properties:X4}.");
        }

        // Normal bombs live in physical projectile slots five through nine, so they must
        // reach `$A6:DF8A` through EnemyBombCollHandler rather than the five-slot ordinary
        // shot walk above. Ridley's retail bomb byte is `$80`: the callback still accepts
        // the authored extended body rectangle and marks the explosion, but its low-seven
        // multiplier is zero and common no-death-check damage leaves every boss word alone.
        ushort vulnerabilityPointer = body.Definition.VulnerabilityPointer != 0
            ? body.Definition.VulnerabilityPointer
            : (ushort)0xec1c;
        byte normalBombVulnerability = bus.ReadByte(
            0xb40000 | unchecked((ushort)(vulnerabilityPointer + 14)));
        SamusBombProjectileSlot normalBomb =
            EnemyProjectileAuditAssertions.ArmExplodingNormalBomb(
                sharedProjectiles,
                unchecked((ushort)(body.XPosition + bodyHitOffsetX)),
                unchecked((ushort)(body.YPosition + bodyHitOffsetY)));
        ushort healthBeforeNormalBomb = body.Health;
        ushort propertiesBeforeNormalBomb = body.Properties;
        int normalBombHits = enemies.ResolveOrdinaryBombHits(
            sharedProjectiles,
            immuneBeam,
            samus);
        if (normalBombVulnerability != 0x80 || normalBombHits != 1 ||
            (normalBomb.Direction & 0x0010) == 0 ||
            body.Health != healthBeforeNormalBomb || body.FlashTimer != 0 ||
            body.InvincibilityTimer != 0 || (body.AiHandlerBits & 0x0002) != 0 ||
            body.Properties != propertiesBeforeNormalBomb)
        {
            throw new InvalidDataException(
                $"Ridley normal-bomb reaction mismatch: vulnerability=" +
                $"${normalBombVulnerability:X2}, hits={normalBombHits}, " +
                $"direction=${normalBomb.Direction:X4}, health=" +
                $"{healthBeforeNormalBomb}->{body.Health}, invincibility/flash=" +
                $"{body.InvincibilityTimer}/{body.FlashTimer}, " +
                $"AI=${body.AiHandlerBits:X4}, properties=" +
                $"${propertiesBeforeNormalBomb:X4}->${body.Properties:X4}.");
        }
        normalBomb.ClearFields();

        // Recompute the world point because the armor frame moved Ridley. Missile family
        // `$8100` selects vulnerability `$82`, so common no-death-check damage must arm the
        // boss-specific hurt AI and palette flash while leaving the actor alive.
        var bodyMissile = new SamusProjectileSystem();
        ArmRidleyProjectile(
            bodyMissile.Slots[0],
            unchecked((ushort)(body.XPosition + bodyHitOffsetX)),
            unchecked((ushort)(body.YPosition + bodyHitOffsetY)),
            type: 0x8100,
            damage: 400,
            radius: 4);
        ushort healthBeforeMissile = body.Health;
        int missileHits = enemies.ResolveOrdinaryProjectileHits(
            bus,
            bodyMissile,
            sharedProjectiles,
            samus);
        if (missileHits != 1 || body.Health >= healthBeforeMissile || body.Health == 0 ||
            body.FlashTimer == 0 || (body.AiHandlerBits & 0x0002) == 0 ||
            body.Properties.HasAny(EnemyProperties.Deleted))
        {
            throw new InvalidDataException(
                $"Ridley missile reaction mismatch: hits={missileHits}, " +
                $"health={healthBeforeMissile}->{body.Health}, flash={body.FlashTimer}, " +
                $"AI=${body.AiHandlerBits:X4}, properties=${body.Properties:X4}.");
        }

        // The table bytes adjacent to the normal-bomb entry are intentionally different:
        // `$B4:F1C0` is bomb `$80`, while `$B4:F1C1` is Power Bomb `$82`. The all-slot
        // ellipse therefore applies 200 common no-death-check damage, installs the 48-frame
        // reaction, dispatches `$A6:DFB2`, and latches the authored next-frame lunge without
        // deleting Ridley even if a later blast exhausts his health.
        ClearRidleyDamageTimers(body);
        byte powerBombVulnerability = bus.ReadByte(
            0xb40000 | unchecked((ushort)(vulnerabilityPointer + 15)));
        ushort healthBeforePowerBomb = body.Health;
        ushort propertiesBeforePowerBomb = body.Properties;
        int powerBombHits = enemies.ResolveOrdinaryPowerBombHits(
            bus,
            body.XPosition,
            body.YPosition,
            explosionRadius: 0xff,
            samus);
        ushort expectedPowerBombHealth = unchecked((ushort)(
            healthBeforePowerBomb - 100 * (powerBombVulnerability & 0x7f)));
        ushort expectedPowerBombFlash = unchecked((ushort)(
            (body.HurtAiTime == 0 ? 4 : body.HurtAiTime) + 8));
        if (powerBombVulnerability != 0x82 || powerBombHits != 1 ||
            body.Health != expectedPowerBombHealth ||
            body.InvincibilityTimer != 48 || body.FlashTimer != expectedPowerBombFlash ||
            (body.AiHandlerBits & 0x0002) == 0 || state.PowerBombReactionLatched != 2 ||
            body.Properties.HasAny(EnemyProperties.Deleted) ||
            !body.Properties.HasAny(EnemyProperties.ProcessOffScreen))
        {
            throw new InvalidDataException(
                $"Ridley power-bomb reaction mismatch: vulnerability=" +
                $"${powerBombVulnerability:X2}, hits={powerBombHits}, " +
                $"health={healthBeforePowerBomb}->{body.Health}/{expectedPowerBombHealth}, " +
                $"invincibility/flash={body.InvincibilityTimer}/{body.FlashTimer}, " +
                $"latch={state.PowerBombReactionLatched}, " +
                $"properties=${propertiesBeforePowerBomb:X4}->${body.Properties:X4}.");
        }

        // Lethal normal damage deliberately does not delete Ridley. The next attack select
        // reads the ROM's eight-entry zero-health table ($B3DC: eight lunges), and only a
        // successful claw grab may latch fight mode $FFFF and enter the death sequence.
        ClearRidleyDamageTimers(body);
        state.PowerBombReactionLatched = 0;
        var lethalShots = new SamusProjectileSystem();
        ArmRidleyProjectile(
            lethalShots.Slots[0],
            unchecked((ushort)(body.XPosition + bodyHitOffsetX)),
            unchecked((ushort)(body.YPosition + bodyHitOffsetY)),
            type: 0x8100,
            damage: ushort.MaxValue,
            radius: 8);
        int lethalHits = enemies.ResolveOrdinaryProjectileHits(
            bus,
            lethalShots,
            sharedProjectiles,
            samus);
        if (lethalHits != 1 || body.Health != 0 ||
            body.Properties.HasAny(EnemyProperties.Deleted) ||
            unchecked((short)state.FightMode) < 0)
        {
            throw new InvalidDataException(
                $"Ridley lethal no-death-check mismatch: hits={lethalHits}, " +
                $"health={body.Health}, fight=${state.FightMode:X4}, " +
                $"properties=${body.Properties:X4}.");
        }

        ClearRidleyDamageTimers(body);
        state.Function = RidleyAiFunction.NorfairSelectAttack;
        state.GrabState = 0;
        StepRidley(enemies, assets.LevelData, samus);
        if (state.ZeroHealthLungeCount != 1 ||
            state.Function != RidleyAiFunction.NorfairGrabApproach)
        {
            throw new InvalidDataException(
                $"Ridley zero-health selector mismatch: count={state.ZeroHealthLungeCount}, " +
                $"function=$A6:{(ushort)state.Function:X4}.");
        }

        PlaceSamusAtRidleyClaw(bus, body, state, samus);
        StepRidley(enemies, assets.LevelData, samus);
        if (state.GrabState == 0 || state.FightMode != 0xffff ||
            state.Function != RidleyAiFunction.NorfairReleaseSamus ||
            !body.Properties.HasAny(EnemyProperties.IgnoreSamusCollision))
        {
            throw new InvalidDataException(
                $"Ridley lethal grab mismatch: grabbed={state.GrabState}, " +
                $"fight=${state.FightMode:X4}, function=$A6:{(ushort)state.Function:X4}, " +
                $"properties=${body.Properties:X4}.");
        }

        return AdvanceRidleyDeathToCompletion(
            enemies,
            assets.LevelData,
            samus,
            body,
            state,
            () => bossDefeated);
    }

    private static SamusState CreateCombatSamus(ISnesAddressSpace bus)
    {
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            XPosition = 128,
            YPosition = 352,
            Pose = SamusState.FacingRightNormalPose,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        return samus;
    }

    private static void AdvanceFreshRidleyToCombat(
        RoomEnemySystem enemies,
        RoomLevelData level,
        SamusState samus,
        RidleyEnemyState state)
    {
        for (int frame = 0; frame < 1600 && state.FightMode == 0; frame++)
            StepRidley(enemies, level, samus);
        if (state.FightMode != 1)
        {
            throw new InvalidDataException(
                $"Fresh Ridley combat fixture stopped at $A6:{(ushort)state.Function:X4}/" +
                $"fight={state.FightMode}.");
        }
    }

    /// <summary>
    /// Finds one live body rectangle by asking the public shared contact dispatcher. Every
    /// trial resets Samus's native hurt words, so a tail hit or empty point cannot poison
    /// the next candidate. The returned offset follows Ridley's moving world origin.
    /// </summary>
    private static (short X, short Y) FindRidleyBodyContactPoint(
        ISnesAddressSpace bus,
        RoomEnemySystem enemies,
        RoomEnemySlot body,
        SamusState samus)
    {
        for (short y = -48; y <= 48; y += 4)
        {
            for (short x = -48; x <= 48; x += 4)
            {
                ResetSamusForContact(bus, samus);
                samus.XPosition = unchecked((ushort)(body.XPosition + x));
                samus.YPosition = unchecked((ushort)(body.YPosition + y));
                ushort healthBefore = samus.Health;
                if (!enemies.ResolveRidleySamusContact(samus, controllerInput: 0))
                    continue;
                if (healthBefore - samus.Health == body.Definition.Damage)
                    return (x, y);
            }
        }

        throw new InvalidDataException(
            $"Ridley map $A6:{body.SpritemapPointer:X4} exposed no 160-damage body rectangle " +
            $"within 48 pixels of ({body.XPosition},{body.YPosition}).");
    }

    private static void ResetSamusForContact(ISnesAddressSpace bus, SamusState samus)
    {
        samus.Health = 999;
        samus.XPosition = 128;
        samus.YPosition = 352;
        samus.Pose = SamusState.FacingRightNormalPose;
        samus.InvincibilityTimer = 0;
        samus.KnockbackTimer = 0;
        samus.KnockbackDirection = 0;
        samus.KnockbackActive = false;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
    }

    private static void ArmRidleyProjectile(
        SamusProjectileSlot projectile,
        ushort x,
        ushort y,
        ushort type,
        ushort damage,
        ushort radius)
    {
        projectile.ClearFields();
        projectile.Type = type;
        projectile.Damage = damage;
        projectile.Direction = (ushort)SamusProjectileDirection.Right;
        projectile.XPosition = x;
        projectile.YPosition = y;
        projectile.XRadius = radius;
        projectile.YRadius = radius;
        // A nonzero bank-$93 list pointer is the real allocation/draw sentinel. The audit
        // resolves collision before interpreting this harmless synthetic pointer.
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    private static void ClearRidleyDamageTimers(RoomEnemySlot body)
    {
        body.FlashTimer = 0;
        body.InvincibilityTimer = 0;
        body.FrozenTimer = 0;
        body.AiHandlerBits = unchecked((ushort)(body.AiHandlerBits & ~0x0006));
    }

    private static void PlaceSamusAtRidleyClaw(
        ISnesAddressSpace bus,
        RoomEnemySlot body,
        RidleyEnemyState state,
        SamusState samus)
    {
        ResetSamusForContact(bus, samus);
        int facingOffset = Math.Min(state.FacingDirection, (ushort)2) * 2;
        int footIndex = Math.Min(state.FeetDistanceIndex >> 1, (ushort)8);
        samus.XPosition = unchecked((ushort)(
            body.XPosition + unchecked((short)ReadAuditWord(bus, 0xa6b9d5 + facingOffset))));
        samus.YPosition = unchecked((ushort)(
            body.YPosition + unchecked((short)ReadAuditWord(bus, 0xa6b9db + footIndex * 2))));
    }

    private static RidleyBattleAuditResult AdvanceRidleyDeathToCompletion(
        RoomEnemySystem enemies,
        RoomLevelData level,
        SamusState samus,
        RoomEnemySlot body,
        RidleyEnemyState state,
        Func<bool> bossDefeated)
    {
        var functions = new HashSet<RidleyAiFunction>();
        bool sawSmallExplosion = false;
        bool sawBreakupMotion = false;
        bool sawBreakupFlicker = false;
        bool sawHiddenBody = false;
        int peakBreakupActorCount = 0;
        var initialFragmentPositions = new Dictionary<int, (ushort X, ushort Y)>();
        int deathFrames;
        for (deathFrames = 0;
            deathFrames < 4000 && !body.Properties.HasAny(EnemyProperties.Deleted);
            deathFrames++)
        {
            functions.Add(state.Function);
            StepRidley(enemies, level, samus);
            // Sample newly allocated dust before bank $86 advances it. Several small-dust
            // instruction variants legitimately delete on their first interpreter call;
            // observing only the post-step pool would mistake that short native lifetime
            // for a failed spawn.
            sawSmallExplosion |= CountActiveRidleyDust(enemies) != 0;
            enemies.StepEnemyProjectiles(level, samus, CameraX, CameraY);
            sawHiddenBody |= body.Properties.HasAny(EnemyProperties.Invisible);

            RoomEnemySlot[] liveFragments = enemies.Slots.Where(
                candidate => candidate.EnemyDefinitionPointer == RidleyBreakupDefinition)
                .ToArray();
            peakBreakupActorCount = Math.Max(peakBreakupActorCount, liveFragments.Length);
            foreach (RoomEnemySlot fragment in liveFragments)
            {
                if (!initialFragmentPositions.TryGetValue(
                        fragment.SlotIndex,
                        out (ushort X, ushort Y) initial))
                {
                    initialFragmentPositions.Add(
                        fragment.SlotIndex,
                        (fragment.XPosition, fragment.YPosition));
                }
                else
                {
                    sawBreakupMotion |= fragment.XPosition != initial.X ||
                        fragment.YPosition != initial.Y;
                }
                sawBreakupFlicker |= fragment.Properties.HasAny(EnemyProperties.Invisible);
            }
        }

        // Deleted enemy slots are reclaimed before the body's final 256-frame wait ends.
        // The relevant invariant is the simultaneous peak at $C588's spawn point, not the
        // empty physical pool after every fragment has completed its own death animation.
        int breakupActorCount = peakBreakupActorCount;
        RidleyAiFunction[] requiredFunctions =
        [
            RidleyAiFunction.NorfairReleaseSamus,
            RidleyAiFunction.NorfairDeathExplosions,
            RidleyAiFunction.NorfairDeathFall,
            RidleyAiFunction.NorfairDeathImpact,
            RidleyAiFunction.NorfairDeathWait,
            RidleyAiFunction.NorfairDeathFinish,
        ];
        RidleyAiFunction[] missing = requiredFunctions.Where(function =>
            !functions.Contains(function)).ToArray();
        if (deathFrames >= 4000 || !bossDefeated() || !state.BossDefeatPublished ||
            !enemies.RidleyDeathDropRequested || state.MusicRequest != 3 ||
            !body.Properties.HasAny(EnemyProperties.Deleted) || state.GrabState != 0 ||
            breakupActorCount != 12 || !state.DeathBreakupSpawned ||
            !sawSmallExplosion || !sawHiddenBody || !sawBreakupMotion ||
            !sawBreakupFlicker || missing.Length != 0)
        {
            throw new InvalidDataException(
                $"Ridley death sequence mismatch after {deathFrames} frames: " +
                $"boss={bossDefeated()}/{state.BossDefeatPublished}, " +
                $"drop/music={enemies.RidleyDeathDropRequested}/{state.MusicRequest}, " +
                $"deleted/grab={body.Properties.HasAny(EnemyProperties.Deleted)}/{state.GrabState}, " +
                $"breakup={breakupActorCount}/{state.DeathBreakupSpawned}, " +
                $"dust/hidden/move/flicker={sawSmallExplosion}/{sawHiddenBody}/" +
                $"{sawBreakupMotion}/{sawBreakupFlicker}, " +
                $"missing=[{string.Join(',', missing.Select(value => $"$A6:{(ushort)value:X4}"))}].");
        }

        return new RidleyBattleAuditResult(deathFrames, breakupActorCount);
    }

    private static void StepRidley(
        RoomEnemySystem enemies,
        RoomLevelData level,
        SamusState samus,
        SamusProjectileSystem? projectiles = null) =>
        enemies.StepFrame(
            CameraX,
            CameraY,
            timeIsFrozen: false,
            samus,
            level: level,
            samusProjectiles: projectiles);

    private static int CountActiveRidleyDust(RoomEnemySystem enemies) =>
        enemies.EnemyProjectiles.Count(projectile =>
            projectile.Kind == RoomEnemyProjectileKind.MiscDustExplosion &&
            projectile.IsActive);

    private static ushort ReadAuditWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private readonly record struct RidleyBattleAuditResult(
        int DeathFrames,
        int BreakupActorCount);
}
