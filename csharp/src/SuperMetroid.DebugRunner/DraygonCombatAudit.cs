using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Encounter-level proof for Draygon's authored collision callbacks, three ordinary damage
/// channels, synchronized hurt palettes, and complete physical burial. Unlike a unit fixture,
/// this loads the retail room and lets ROM instruction lists own every boss transition.
/// </summary>
internal static class DraygonCombatAudit
{
    private const ushort RoomPointer = 0xda60;
    private const int CombatReadyFrameLimit = 3000;
    private const int DeathFrameLimit = 2400;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, RoomPointer);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState(0x4937);
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            XPosition = 0x0100,
            YPosition = 0x0100,
            Pose = SamusPoseIds.FacingRightNormalPose,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);

        bool bossBitSet = false;
        var enemies = new RoomEnemySystem();
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
            isAreaBossDefeated: () => bossBitSet,
            setAreaBossDefeated: () => bossBitSet = true);

        DraygonEnemyState state = enemies.Draygon ??
            throw new InvalidDataException("Draygon combat fixture did not load encounter state.");
        RoomEnemySlot body = state.Body;

        // Reach a naturally visible idle/swoop map. No enemy coordinate, function, timer,
        // health, or instruction field is written by this audit.
        int readyFrame;
        for (readyFrame = 0; readyFrame < CombatReadyFrameLimit; readyFrame++)
        {
            enemies.StepFrame(
                cameraX: 0,
                cameraY: 0,
                timeIsFrozen: false,
                samus,
                level: assets.LevelData,
                nmiFrameCounter8: unchecked((byte)readyFrame));
            if (body.XPosition is > 0 and < 0x0300 && body.YPosition < 0x0300 &&
                (body.SpritemapPointer & 0x8000) != 0 &&
                enemies.InteractiveEnemyIndexes.Contains(body.NativeIndex) &&
                state.Function is not (
                    DraygonAiFunction.IntroInitialDelay or DraygonAiFunction.IntroDance))
            {
                break;
            }
        }
        if (readyFrame >= CombatReadyFrameLimit)
            throw new InvalidDataException("Draygon never reached a visible combat map.");

        RoomEnemySlot eye = state.Eye ??
            throw new InvalidDataException("Draygon combat fixture lost the eye record.");
        RoomEnemySlot arms = state.Arms ??
            throw new InvalidDataException("Draygon combat fixture lost the arms record.");
        bool protectedPartsRemainNonInteractive =
            eye.Properties.HasAny(EnemyProperties.IgnoreSamusCollision) &&
            arms.Properties.HasAny(EnemyProperties.IgnoreSamusCollision) &&
            !enemies.InteractiveEnemyIndexes.Contains(eye.NativeIndex) &&
            !enemies.InteractiveEnemyIndexes.Contains(arms.NativeIndex);

        // Eye and arms are physical enemy records with header shot callback `$A5:804C`, but
        // population property $0400 deliberately excludes both from bank $A0's collision
        // index. The body's extended map owns every live weapon rectangle. A normal bomb in
        // its authored eye rectangle must therefore dispatch body callback $95F0 exactly
        // once: vulnerability byte $80 deals no HP damage, while the private prelude still
        // increases future swoop acceleration by eight.
        var normalBombs = new SamusBombProjectileSystem();
        var normalBombOrdinaryShots = new SamusProjectileSystem();
        SamusBombProjectileSlot normalBomb =
            EnemyProjectileAuditAssertions.ArmExplodingNormalBomb(
                normalBombs,
                body.XPosition,
                body.YPosition,
                damage: 1000);
        normalBomb.XRadius = 1;
        normalBomb.YRadius = 1;
        ushort healthBeforeNormalBomb = body.Health;
        ushort accelerationBeforeNormalBomb = state.SwoopYAcceleration;
        ushort expectedAccelerationAfterNormalBomb = unchecked((ushort)(
            accelerationBeforeNormalBomb + 8));
        if (unchecked((short)(expectedAccelerationAfterNormalBomb - 0x00a0)) >= 0)
            expectedAccelerationAfterNormalBomb = accelerationBeforeNormalBomb;
        int normalBombHits = enemies.ResolveOrdinaryBombHits(
            normalBombs,
            normalBombOrdinaryShots,
            samus);
        ushort healthAfterNormalBomb = body.Health;
        ushort accelerationAfterNormalBomb = state.SwoopYAcceleration;
        bool normalBombReactionAgreed = normalBombHits == 1 &&
            (normalBomb.Direction & 0x0010) != 0 &&
            healthAfterNormalBomb == healthBeforeNormalBomb &&
            accelerationAfterNormalBomb == expectedAccelerationAfterNormalBomb;

        // In maps $1A/$2D the first rectangle is the eye: touch is literal RTL, while shot
        // dispatches $95F0. The upper shell rectangle instead dispatches damaging touch and
        // shared dud-shot `$8046`. Place actors in those authored regions, not header radii.
        samus.XPosition = body.XPosition;
        samus.YPosition = body.YPosition;
        samus.InvincibilityTimer = 0;
        ushort healthBeforeEyeContact = samus.Health;
        bool eyeContactResolved = enemies.ResolveOrdinarySamusContact(samus, controllerInput: 0);
        ushort healthAfterEyeContact = samus.Health;

        short shellXOffset = state.FacingRight ? (short)24 : (short)-24;
        samus.XPosition = unchecked((ushort)(body.XPosition + shellXOffset));
        samus.YPosition = unchecked((ushort)(body.YPosition - 40));
        samus.InvincibilityTimer = 0;
        samus.KnockbackTimer = 0;
        samus.KnockbackActive = false;
        ushort healthBeforeShellContact = samus.Health;
        bool shellContactResolved = enemies.ResolveOrdinarySamusContact(samus, controllerInput: 0);
        ushort healthAfterShellContact = samus.Health;

        // The shared radius pass must read power-bomb vulnerability byte 15. Draygon's
        // retail table stores `$80` there, so the dispatcher rejects every body part before
        // consulting its reaction callback. The nearby `$81` is byte 17 (shinespark), not
        // a power-bomb multiplier; keeping this assertion prevents those fields being mixed.
        ushort healthBeforePowerBomb = body.Health;
        int powerBombReactions = enemies.ResolveOrdinaryPowerBombHits(
            bus,
            body.XPosition,
            body.YPosition,
            explosionRadius: 0x7f,
            samus);
        ushort healthAfterPowerBomb = body.Health;
        bool allPartsIgnoredPowerBomb = eye.Health == 6000 &&
            state.Tail!.Health == 6000 && arms.Health == 6000;

        bool sawWhiteBgPalette = false;
        bool sawWhiteSpritePalette = false;

        var shots = new SamusProjectileSystem();
        var bombs = new SamusBombProjectileSystem();
        SamusProjectileSlot projectile = shots.Slots[0];

        shellXOffset = state.FacingRight ? (short)24 : (short)-24;
        ArmProjectile(
            projectile,
            unchecked((ushort)(body.XPosition + shellXOffset)),
            unchecked((ushort)(body.YPosition - 40)),
            type: (ushort)SamusProjectileFamily.Missile,
            damage: 100);
        ushort healthBeforeDud = body.Health;
        int dudHits = enemies.ResolveOrdinaryProjectileHits(bus, shots, bombs, samus);
        ushort healthAfterDud = body.Health;

        int vulnerableEyeHits = 0;
        bool sawLowestHealthPalette = false;
        for (int shot = 0; shot < 20; shot++)
        {
            ArmProjectile(
                projectile,
                body.XPosition,
                body.YPosition,
                type: (ushort)SamusProjectileFamily.SuperMissile,
                damage: 300);
            vulnerableEyeHits += enemies.ResolveOrdinaryProjectileHits(bus, shots, bombs, samus);
            if (body.Health != 0)
                sawLowestHealthPalette |= PaletteMatchesHealthBand(bus, cgram, tableByteIndex: 14);
        }

        var deathFunctions = new HashSet<DraygonAiFunction> { state.Function };
        var priorEvirPositions = new Dictionary<int, (ushort X, ushort Y)>();
        var movedEvirSlots = new HashSet<int>();
        int maximumBurialEvirs = 0;
        int deathFrame;
        for (deathFrame = 0; deathFrame < DeathFrameLimit; deathFrame++)
        {
            byte nmi = unchecked((byte)(readyFrame + deathFrame + 1));
            enemies.StepFrame(
                cameraX: 0,
                cameraY: 0,
                timeIsFrozen: false,
                samus,
                level: assets.LevelData,
                nmiFrameCounter8: nmi);
            deathFunctions.Add(state.Function);
            enemies.DrawLayers(new OamBuffer(), 0, 0, firstLayer: 0, lastLayer: 7);
            // The final vulnerable eye hit leaves hurt AI active while the death drift
            // begins. Observe both palette targets here instead of manufacturing a hit on
            // Draygon's power-bomb-immune body.
            sawWhiteBgPalette |= PaletteIsWhite(cgram, 80);
            sawWhiteSpritePalette |= PaletteIsWhite(cgram, 240);

            int liveBurialEvirs = 0;
            foreach (RoomSpriteObjectSlot sprite in enemies.RoomSpriteObjects)
            {
                if (!sprite.IsActive || sprite.Kind is not (
                        RoomSpriteObjectKind.DraygonIntroEvir or
                        RoomSpriteObjectKind.DraygonDeathEvirFacingRight))
                {
                    continue;
                }
                liveBurialEvirs++;
                if (priorEvirPositions.TryGetValue(
                        sprite.SlotIndex,
                        out (ushort X, ushort Y) prior) &&
                    (prior.X != sprite.XPosition || prior.Y != sprite.YPosition))
                {
                    movedEvirSlots.Add(sprite.SlotIndex);
                }
                priorEvirPositions[sprite.SlotIndex] = (sprite.XPosition, sprite.YPosition);
            }
            maximumBurialEvirs = Math.Max(maximumBurialEvirs, liveBurialEvirs);
            if (state.BossDefeatPersisted)
                break;
        }

        bool allPartsDeleted = new[] { body, state.Eye!, state.Tail!, state.Arms! }
            .All(part => part.Properties.HasAny(EnemyProperties.Deleted));
        bool spritePoolCleared = enemies.RoomSpriteObjects.All(sprite => !sprite.IsActive);
        DraygonAiFunction[] requiredDeathFunctions =
        [
            DraygonAiFunction.Dying,
            DraygonAiFunction.DyingSink,
            DraygonAiFunction.DyingFinish,
        ];

        if (!eyeContactResolved || healthAfterEyeContact != healthBeforeEyeContact ||
            !shellContactResolved || healthBeforeShellContact - healthAfterShellContact != 160 ||
            !protectedPartsRemainNonInteractive || !normalBombReactionAgreed ||
            powerBombReactions != 0 || healthAfterPowerBomb != healthBeforePowerBomb ||
            !allPartsIgnoredPowerBomb || !sawWhiteBgPalette || !sawWhiteSpritePalette ||
            dudHits != 1 || healthAfterDud != healthBeforeDud || vulnerableEyeHits != 20 ||
            body.Health != 0 || state.SwoopYAcceleration != 0x0098 ||
            state.HealthPaletteTableByteIndex != 14 || !sawLowestHealthPalette ||
            requiredDeathFunctions.Any(function => !deathFunctions.Contains(function)) ||
            state.DeathAnimationObjectsSpawned == 0 || state.DeathSmokeObjectsSpawned == 0 ||
            !state.DeathEvirsSpawned || maximumBurialEvirs != 6 || movedEvirSlots.Count != 6 ||
            !state.ItemDropRequested || !state.BossDefeatPersisted || !bossBitSet ||
            state.MusicRequest != 3 || !allPartsDeleted || !spritePoolCleared ||
            deathFrame >= DeathFrameLimit)
        {
            throw new InvalidDataException(
                $"Draygon combat/death mismatch: ready={readyFrame}, contacts=" +
                $"{eyeContactResolved}:{healthBeforeEyeContact}->{healthAfterEyeContact}/" +
                $"{shellContactResolved}:{healthBeforeShellContact}->{healthAfterShellContact}, " +
                $"protected parts/normal bomb={protectedPartsRemainNonInteractive}/" +
                $"{normalBombHits}:${normalBomb.Direction:X4}:" +
                $"{healthBeforeNormalBomb}->{healthAfterNormalBomb}:" +
                $"{accelerationBeforeNormalBomb}->{accelerationAfterNormalBomb}, " +
                $"power bomb={powerBombReactions}:{healthBeforePowerBomb}->{healthAfterPowerBomb}, " +
                $"parts={allPartsIgnoredPowerBomb}, flashes={sawWhiteBgPalette}/" +
                $"{sawWhiteSpritePalette}, dud={dudHits}:{healthBeforeDud}->{healthAfterDud}, " +
                $"eye hits/health/accel={vulnerableEyeHits}/{body.Health}/" +
                $"${state.SwoopYAcceleration:X4}, palette={state.HealthPaletteTableByteIndex}/" +
                $"{sawLowestHealthPalette}, functions=" +
                $"{string.Join(',', deathFunctions.Select(function => $"${(ushort)function:X4}"))}, " +
                $"objects={state.DeathAnimationObjectsSpawned}/{state.DeathSmokeObjectsSpawned}, " +
                $"Evirs={state.DeathEvirsSpawned}/{maximumBurialEvirs}/{movedEvirSlots.Count}, " +
                $"item/boss/callback/music={state.ItemDropRequested}/{state.BossDefeatPersisted}/" +
                $"{bossBitSet}/{state.MusicRequest}, deleted/pool={allPartsDeleted}/{spritePoolCleared}, " +
                $"death frames={deathFrame}.");
        }

        return readyFrame + deathFrame + 1;
    }

    private static void ArmProjectile(
        SamusProjectileSlot projectile,
        ushort x,
        ushort y,
        ushort type,
        ushort damage)
    {
        projectile.ClearFields();
        projectile.Type = type;
        projectile.Damage = damage;
        projectile.Direction = (ushort)SamusProjectileDirection.Right;
        projectile.XPosition = x;
        projectile.YPosition = y;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    private static bool PaletteIsWhite(SnesCgram cgram, int destination)
    {
        for (int color = 0; color < 16; color++)
        {
            ushort expected = color == 0 ? (ushort)0x3800 : (ushort)0x7fff;
            if (cgram.Colors[destination + color] != expected)
                return false;
        }
        return true;
    }

    private static bool PaletteMatchesHealthBand(
        SuperMetroidAddressSpace bus,
        SnesCgram cgram,
        ushort tableByteIndex)
    {
        int source = 0xa596af + tableByteIndex * 4;
        for (int color = 0; color < 4; color++)
        {
            ushort expected = (ushort)(
                bus.ReadByte(source + color * 2) |
                (bus.ReadByte(source + color * 2 + 1) << 8));
            if (cgram.Colors[89 + color] != expected)
                return false;
        }
        return true;
    }
}
