using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// Untouched-ROM encounter audit for Phantoon's retail Wrecked Ship room. This checkpoint
/// covers the complete load and fight introduction plus the no-damage route through the
/// first and subsequent flame rains. Later shot/damage/death slices extend this same audit
/// instead of replacing retail state with hand-authored fixtures.
/// </summary>
internal static class PhantoonAudit
{
    private const ushort RoomPointer = 0xcd13;
    private const ushort PopulationPointer = 0xccd4;
    private static readonly ushort[] ExpectedDefinitions = [0xe4bf, 0xe4ff, 0xe53f, 0xe57f];

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        // Read the region-specific immediate operands rather than assuming PAL's +/-3.
        ushort clockwiseRageSpeed = ReadRageSpeedOperand(PhantoonAuditReferenceData.ClockwiseRageSpeedInstruction);
        ushort counterclockwiseRageSpeed = ReadRageSpeedOperand(PhantoonAuditReferenceData.CounterclockwiseRageSpeedInstruction);
        ushort ReadRageSpeedOperand(int address)
        {
            if (bus.ReadByte(address) != 0xa9)
                throw new InvalidDataException($"Expected rage initializer LDA immediate at ${address:X6}.");
            return (ushort)(bus.ReadByte(address + 1) | bus.ReadByte(address + 2) << 8);
        }
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, RoomPointer);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        if (room.WidthInScreens != 1 || room.HeightInScreens != 1 ||
            room.AreaIndex != AreaId.WreckedShip ||
            room.State.EnemyPopulationPointer != PopulationPointer)
        {
            throw new InvalidDataException(
                $"Phantoon room mismatch: {room.WidthInScreens}x{room.HeightInScreens}, " +
                $"area={room.AreaIndex}, population=$A1:{room.State.EnemyPopulationPointer:X4}.");
        }

        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState(0x1234);
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            XPosition = 128,
            YPosition = 192,
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
            isAreaBossDefeated: () => false,
            setAreaBossDefeated: () => bossBitSet = true);

        PhantoonEnemyState state = enemies.Phantoon ??
            throw new InvalidDataException("Phantoon room did not allocate typed encounter state.");
        if (enemies.EnemyCount != 4 || state.Eye is null ||
            state.Tentacles is null || state.Mouth is null)
        {
            throw new InvalidDataException("Phantoon's four physical records were not linked.");
        }
        for (int slot = 0; slot < ExpectedDefinitions.Length; slot++)
        {
            if (enemies.Slots[slot].EnemyDefinitionPointer != ExpectedDefinitions[slot])
            {
                throw new InvalidDataException(
                    $"Phantoon slot {slot} loaded ${enemies.Slots[slot].EnemyDefinitionPointer:X4}, " +
                    $"expected ${ExpectedDefinitions[slot]:X4}.");
            }
        }

        RoomEnemySlot body = state.Body;
        if (body.XPosition != 128 || body.YPosition != 96 || body.Health != 2500 ||
            body.CurrentInstruction != 0xcc41 || state.Eye.CurrentInstruction != 0xcc7b ||
            state.Tentacles.CurrentInstruction != 0xccd7 || state.Mouth.CurrentInstruction != 0xccf7 ||
            body.VariableE != 0x0060 ||
            body.VariableF != (ushort)PhantoonAiFunction.SpawnStartingFlames ||
            state.Mouth.VariableC != 0xffff || !state.BackgroundTilemapPrepared ||
            state.Bg2TilemapSize != 0x0360 || vram.ReadWord(0x4800) != 0x0338 ||
            vram.ReadWord(0x4fff) != 0x0338)
        {
            throw new InvalidDataException(
                $"Phantoon initialization mismatch: body=({body.XPosition},{body.YPosition}) " +
                $"hp={body.Health}, list=$A7:{body.CurrentInstruction:X4}, " +
                $"function=$A7:{body.VariableF:X4}, timer={body.VariableE}, " +
                $"mouth pattern=${state.Mouth.VariableC:X4}, BG={state.BackgroundTilemapPrepared}.");
        }

        var functions = new HashSet<PhantoonAiFunction>();
        var tentacleMaps = new HashSet<ushort>();
        var startingFlameSlots = new HashSet<int>();
        var movedStartingFlames = new HashSet<int>();
        var previousFlamePositions = new Dictionary<int, (ushort X, ushort Y)>();
        var rainColumns = new HashSet<ushort>();
        var rainDelays = new HashSet<ushort>();
        var casualFlameStates = new HashSet<ushort>();
        ushort initialBodyX = body.XPosition;
        ushort initialBodyY = body.YPosition;
        int firstRoundFrames = 0;
        bool reachedFirstFigureEight = false;
        bool sawEyeTracking = false;
        bool sawInitialFlameRain = false;
        bool sawFlameRainVulnerability = false;
        bool sawSubsequentFlameRain = false;
        bool sawRainFlameMove = false;
        bool sawFullHealthPalette = false;
        bool sawBattleMusicRequest = false;
        int frame;
        for (frame = 0; frame < 10000; frame++)
        {
            byte nmi = unchecked((byte)frame);
            enemies.StepFrame(
                cameraX: 0,
                cameraY: 0,
                timeIsFrozen: false,
                samus,
                level: assets.LevelData,
                nmiFrameCounter8: nmi);
            // Music publications are one-frame events, not durable boss state. Observe
            // the actual request during the introduction instead of expecting it to
            // survive through the subsequent rain cycle.
            sawBattleMusicRequest |= state.MusicRequest?.RawValue == 5;
            tentacleMaps.Add(state.Tentacles.SpritemapPointer);
            PhantoonAiFunction function = (PhantoonAiFunction)body.VariableF;
            functions.Add(function);
            sawEyeTracking |= function == PhantoonAiFunction.EyeTracksSamus;
            sawInitialFlameRain |= function == PhantoonAiFunction.FadeOutBeforeFirstFlameRain;
            sawFlameRainVulnerability |= function == PhantoonAiFunction.TrackSamusDuringFlameRain;
            sawSubsequentFlameRain |= function == PhantoonAiFunction.SpawnFlameRain;

            foreach (RoomEnemyProjectileSlot projectile in enemies.EnemyProjectiles)
            {
                if (projectile.Kind == RoomEnemyProjectileKind.PhantoonStartingFlame)
                {
                    startingFlameSlots.Add(projectile.SlotIndex);
                    if (previousFlamePositions.TryGetValue(
                            projectile.SlotIndex,
                            out (ushort X, ushort Y) previous) &&
                        (previous.X != projectile.XPosition || previous.Y != projectile.YPosition))
                    {
                        movedStartingFlames.Add(projectile.SlotIndex);
                    }
                    previousFlamePositions[projectile.SlotIndex] =
                        (projectile.XPosition, projectile.YPosition);
                }

                // The same destroyable-flame definition serves casual, rain, spiral, and
                // rage producers. Its native pre-instruction therefore identifies which
                // behavior is under observation without inventing a host-only kind.
                if (projectile.Kind == RoomEnemyProjectileKind.PhantoonDestroyableFlame)
                {
                    casualFlameStates.Add(projectile.PreInstruction);
                    if (projectile.PreInstruction == 0x9a94)
                    {
                        rainColumns.Add(projectile.XPosition);
                        rainDelays.Add(projectile.XVelocity);
                        sawRainFlameMove |= projectile.YPosition > 40;
                    }
                }
            }

            enemies.StepEnemyProjectiles(
                assets.LevelData,
                samus,
                cameraX: 0,
                cameraY: 0,
                nmiFrameCounter8: nmi);

            // Extended body/eye/tentacle/mouth maps are the actual renderer producer. Run
            // the draw pass during the audit so a malformed BG2 stream cannot hide behind
            // state-only assertions.
            enemies.DrawLayers(new OamBuffer(), 0, 0, firstLayer: 0, lastLayer: 7);

            if (body.VariableF == (ushort)PhantoonAiFunction.MoveInFigureEightThenOpenEye)
            {
                firstRoundFrames++;
                reachedFirstFigureEight |= firstRoundFrames >= 32;
                sawFullHealthPalette |= PhantoonPaletteMatchesFullHealth(bus, cgram);
            }


            // The subsequent-rain state is recorded before its timer expires. Once all
            // eight physical rain actors have also moved, this route has exercised the
            // complete hidden/fade/show/vulnerable/fade loop and can stop deterministically.
            if (sawSubsequentFlameRain && sawRainFlameMove && rainColumns.Count == 8)
                break;
        }

        int liveStartingFlames = enemies.EnemyProjectiles.Count(
            projectile => projectile.Kind == RoomEnemyProjectileKind.PhantoonStartingFlame);
        ushort[] expectedRainDelays = [8, 16, 24, 32, 40, 48, 56, 64];
        bool rainDelaysMatch = expectedRainDelays.All(rainDelays.Contains);
        bool rainColumnsAreAuthored = rainColumns.All(
            x => x >= 0x30 && x <= 0xd0 && ((x - 0x30) % 0x14) == 0);

        if (!reachedFirstFigureEight || functions.Count < 10 ||
            state.StartingFlameRequests != 8 || state.StartingFlamesSpawned != 8 ||
            startingFlameSlots.Count != 8 || movedStartingFlames.Count != 8 ||
            liveStartingFlames != 0 || state.BossDoorPlmRequest != 0xb781 ||
            !sawBattleMusicRequest || state.Mouth.Parameter1 != 1 ||
            !sawFullHealthPalette || tentacleMaps.Count != 3 ||
            (body.XPosition == initialBodyX && body.YPosition == initialBodyY) ||
            !sawEyeTracking || !sawInitialFlameRain || !sawFlameRainVulnerability ||
            !sawSubsequentFlameRain || rainColumns.Count != 8 || !rainColumnsAreAuthored ||
            !rainDelaysMatch || !sawRainFlameMove || !casualFlameStates.Contains(0x9981))
        {
            throw new InvalidDataException(
                $"Phantoon combat route mismatch after {frame} frames: function=$A7:{body.VariableF:X4}, " +
                $"round frames={firstRoundFrames}, functions={functions.Count}, flames=" +
                $"{state.StartingFlamesSpawned}/{state.StartingFlameRequests} " +
                $"slots/moved/live={startingFlameSlots.Count}/{movedStartingFlames.Count}/{liveStartingFlames}, " +
                $"door={state.BossDoorPlmRequest}, music={state.MusicRequest}, " +
                $"mouth control=${state.Mouth.Parameter1:X4}, palette={sawFullHealthPalette}, " +
                $"tentacle maps={tentacleMaps.Count}, eye/rain0/vulnerable/rainN=" +
                $"{sawEyeTracking}/{sawInitialFlameRain}/{sawFlameRainVulnerability}/" +
                $"{sawSubsequentFlameRain}, rain columns/delays/moved=" +
                $"{rainColumns.Count}/{rainDelays.Count}/{sawRainFlameMove}, " +
                $"casual states={string.Join(',', casualFlameStates.Select(x => $"${x:X4}"))}, " +
                $"body=({body.XPosition},{body.YPosition}).");
        }
        if (bossBitSet)
            throw new InvalidDataException("Phantoon intro unexpectedly persisted boss defeat.");

        // Continue the same untouched-ROM encounter until the next visible flame-rain
        // window. A low-strength physical normal bomb first exercises the `$DD9B` component
        // callback without crossing the 300-damage close threshold. One ordinary Missile
        // then requests the real swoop branch, allowing the authored full-body touch boxes
        // to damage Samus; a later Super Missile crosses the one-shot rage threshold without
        // any audit-only encounter-state mutation.
        var samusShots = new SamusProjectileSystem();
        var sharedProjectiles = new SamusBombProjectileSystem();
        var normalBombProjectiles = new SamusBombProjectileSystem();
        bool firedNormalBomb = false;
        int normalBombHitCount = 0;
        ushort healthBeforeNormalBomb = 0;
        ushort healthAfterNormalBomb = 0;
        ushort expectedNormalBombDamage = 0;
        bool firedSwoopTrigger = false;
        int swoopTriggerHitCount = 0;
        bool sawSwoop = false;
        bool resolvedSwoopContact = false;
        ushort samusHealthBeforeContact = 0;
        ushort samusHealthAfterContact = 0;
        bool firedRageTrigger = false;
        bool rageHitQueuedSound = false;
        bool sawRageFadeOut = false;
        bool sawRage = false;
        bool sawPostRageFade = false;
        bool sawClockwiseRageFlame = false;
        bool sawCounterclockwiseRageFlame = false;
        bool sawWhiteDamagePalette = false;
        bool sawDamagedHealthPalette = false;
        ushort maximumRageRound = 0;
        ushort healthBeforeRageShot = 0;
        int rageHitCount = 0;
        int rageFrame;
        for (rageFrame = 0; rageFrame < 2400; rageFrame++)
        {
            byte nmi = unchecked((byte)(frame + rageFrame + 1));
            enemies.StepFrame(
                cameraX: 0,
                cameraY: 0,
                timeIsFrozen: false,
                samus,
                level: assets.LevelData,
                nmiFrameCounter8: nmi);

            PhantoonAiFunction function = (PhantoonAiFunction)body.VariableF;
            sawWhiteDamagePalette |= PhantoonPaletteIsWhite(cgram);
            if (firedRageTrigger)
            {
                sawDamagedHealthPalette |= PhantoonPaletteMatchesHealth(
                    bus,
                    cgram,
                    body.Health);
            }
            if (!firedNormalBomb &&
                function == PhantoonAiFunction.TrackSamusDuringFlameRain &&
                body.XPosition != 0 &&
                enemies.InteractiveEnemyIndexes.Contains(body.NativeIndex))
            {
                ushort vulnerabilityPointer = body.Definition.VulnerabilityPointer != 0
                    ? body.Definition.VulnerabilityPointer
                    : (ushort)0xec1c;
                byte normalBombVulnerability = bus.ReadByte(
                    0xb40000 | unchecked((ushort)(vulnerabilityPointer + 14)));
                expectedNormalBombDamage = unchecked((ushort)(
                    (2 >> 1) * (normalBombVulnerability & 0x7f)));
                SamusBombProjectileSlot normalBomb =
                    EnemyProjectileAuditAssertions.ArmExplodingNormalBomb(
                        normalBombProjectiles,
                        body.XPosition,
                        body.YPosition,
                        damage: 2);
                healthBeforeNormalBomb = body.Health;
                int acceptedBeforeNormalBomb = state.AcceptedProjectileHits;
                normalBombHitCount = enemies.ResolveOrdinaryBombHits(
                    normalBombProjectiles,
                    samusShots,
                    samus);
                healthAfterNormalBomb = body.Health;
                if (normalBombHitCount != 1 || (normalBomb.Direction & 0x0010) == 0 ||
                    healthBeforeNormalBomb - healthAfterNormalBomb != expectedNormalBombDamage ||
                    state.AcceptedProjectileHits != acceptedBeforeNormalBomb + 1 ||
                    state.LastProjectileDamage != expectedNormalBombDamage ||
                    body.Properties.HasAny(EnemyProperties.Deleted))
                {
                    throw new InvalidDataException(
                        "Phantoon normal-bomb callback mismatch: " +
                        $"hits={normalBombHitCount}, direction=${normalBomb.Direction:X4}, " +
                        $"health={healthBeforeNormalBomb}->{healthAfterNormalBomb}, " +
                        $"expected damage={expectedNormalBombDamage}, " +
                        $"accepted={acceptedBeforeNormalBomb}->{state.AcceptedProjectileHits}, " +
                        $"reported damage={state.LastProjectileDamage}, " +
                        $"body=({body.XPosition},{body.YPosition}), " +
                        $"map=$A7:{body.SpritemapPointer:X4}, properties=${body.Properties:X4}.");
                }
                firedNormalBomb = true;
                function = (PhantoonAiFunction)body.VariableF;
            }
            else if (firedNormalBomb && !firedSwoopTrigger &&
                function == PhantoonAiFunction.TrackSamusDuringFlameRain)
            {
                ArmPhantoonProjectile(
                    samusShots.Slots[0],
                    body.XPosition,
                    body.YPosition,
                    type: 0x0100,
                    damage: 100);
                swoopTriggerHitCount = enemies.ResolvePhantoonProjectileHits(
                    bus,
                    samusShots,
                    sharedProjectiles);
                firedSwoopTrigger = swoopTriggerHitCount == 1;
                function = (PhantoonAiFunction)body.VariableF;
            }
            else if (firedSwoopTrigger && resolvedSwoopContact && !firedRageTrigger &&
                function == PhantoonAiFunction.TrackSamusDuringFlameRain)
            {
                healthBeforeRageShot = body.Health;
                ArmPhantoonProjectile(
                    samusShots.Slots[0],
                    body.XPosition,
                    body.YPosition,
                    type: 0x0200,
                    damage: 300);
                rageHitCount = enemies.ResolvePhantoonProjectileHits(
                    bus,
                    samusShots,
                    sharedProjectiles);
                rageHitQueuedSound = state.LastCombatSoundEffect == 0x0073;
                firedRageTrigger = true;
                function = (PhantoonAiFunction)body.VariableF;
            }

            sawSwoop |= function == PhantoonAiFunction.Swooping;
            if (!resolvedSwoopContact && function == PhantoonAiFunction.Swooping)
            {
                ushort savedX = samus.XPosition;
                ushort savedY = samus.YPosition;
                samus.XPosition = body.XPosition;
                samus.YPosition = body.YPosition;
                samus.InvincibilityTimer = 0;
                samus.KnockbackActive = false;
                samus.KnockbackDirection = 0;
                samus.KnockbackXDirection = 0;
                samus.KnockbackTimer = 0;
                samus.Pose = SamusPoseIds.FacingRightNormalPose;
                samus.RefreshCollisionRadii(bus);
                samus.InitializeAnimation(bus);
                samusHealthBeforeContact = samus.Health;
                resolvedSwoopContact = enemies.ResolveOrdinarySamusContact(
                    samus,
                    controllerInput: 0);
                samusHealthAfterContact = samus.Health;
                samus.XPosition = savedX;
                samus.YPosition = savedY;
            }

            sawRageFadeOut |= function == PhantoonAiFunction.FadeOutBeforeRage;
            sawRage |= function == PhantoonAiFunction.Enraged;
            sawPostRageFade |= function == PhantoonAiFunction.FadeOutAfterRage;
            maximumRageRound = Math.Max(maximumRageRound, state.Eye.VariableF);

            foreach (RoomEnemyProjectileSlot projectile in enemies.EnemyProjectiles)
            {
                if (projectile.Kind != RoomEnemyProjectileKind.PhantoonDestroyableFlame ||
                    projectile.PreInstruction != 0x9a45)
                {
                    continue;
                }
                sawClockwiseRageFlame |= projectile.XVelocity == clockwiseRageSpeed;
                sawCounterclockwiseRageFlame |= projectile.XVelocity == counterclockwiseRageSpeed;
            }

            enemies.StepEnemyProjectiles(
                assets.LevelData,
                samus,
                cameraX: 0,
                cameraY: 0,
                nmiFrameCounter8: nmi);
            enemies.DrawLayers(new OamBuffer(), 0, 0, firstLayer: 0, lastLayer: 7);

            if (sawPostRageFade &&
                function == PhantoonAiFunction.WaitAfterFadeOut)
            {
                break;
            }
        }

        if (!firedNormalBomb || normalBombHitCount != 1 ||
            healthBeforeNormalBomb - healthAfterNormalBomb != expectedNormalBombDamage ||
            !firedSwoopTrigger || swoopTriggerHitCount != 1 || !sawSwoop ||
            !resolvedSwoopContact || samusHealthBeforeContact - samusHealthAfterContact != 40 ||
            !firedRageTrigger || rageHitCount != 1 ||
            healthBeforeRageShot - body.Health != 600 ||
            state.LastProjectileDamage != 600 || state.AcceptedProjectileHits != 3 ||
            !rageHitQueuedSound ||
            !sawRageFadeOut || !sawRage || !sawPostRageFade || maximumRageRound < 7 ||
            !sawClockwiseRageFlame || !sawCounterclockwiseRageFlame ||
            !sawWhiteDamagePalette || !sawDamagedHealthPalette)
        {
            throw new InvalidDataException(
                $"Phantoon rage route mismatch after {rageFrame} frames: bomb fired/hits/health/damage=" +
                $"{firedNormalBomb}/{normalBombHitCount}/" +
                $"{healthBeforeNormalBomb}->{healthAfterNormalBomb}/{expectedNormalBombDamage}, " +
                "swoop fired/hits/state/contact=" +
                $"{firedSwoopTrigger}/{swoopTriggerHitCount}/{sawSwoop}/{resolvedSwoopContact}, " +
                $"Samus={samusHealthBeforeContact}->{samusHealthAfterContact}, rage fired/hits=" +
                $"{firedRageTrigger}/{rageHitCount}, health={healthBeforeRageShot}->{body.Health}, " +
                $"damage={state.LastProjectileDamage}, accepted={state.AcceptedProjectileHits}, " +
                $"sound={state.LastCombatSoundEffect}, fade/rage/post=" +
                $"{sawRageFadeOut}/{sawRage}/{sawPostRageFade}, max round={maximumRageRound}, " +
                $"directions={sawClockwiseRageFlame}/{sawCounterclockwiseRageFlame}, " +
                $"hurt palettes={sawWhiteDamagePalette}/{sawDamagedHealthPalette}, " +
                $"function=$A7:{body.VariableF:X4}, map=$A7:{body.SpritemapPointer:X4}.");
        }

        // Let the post-rage encounter choose its next visible window naturally, then land
        // one lethal Super Missile. This proves the shot callback's no-common-death branch
        // and every frame-driven death state through the external boss/door/music effects.
        bool firedLethalShot = false;
        int lethalHitCount = 0;
        ushort healthBeforeLethalShot = 0;
        var deathFunctions = new HashSet<PhantoonAiFunction>();
        byte maximumMosaic = 0;
        bool sawPostBattleMusic = false;
        int deathFrame;
        for (deathFrame = 0; deathFrame < 5000; deathFrame++)
        {
            byte nmi = unchecked((byte)(frame + rageFrame + deathFrame + 2));
            enemies.StepFrame(
                cameraX: 0,
                cameraY: 0,
                timeIsFrozen: false,
                samus,
                level: assets.LevelData,
                nmiFrameCounter8: nmi);

            PhantoonAiFunction function = (PhantoonAiFunction)body.VariableF;
            deathFunctions.Add(function);
            sawPostBattleMusic |= state.MusicRequest?.RawValue == 3;
            if (!firedLethalShot && function is
                PhantoonAiFunction.EyeTracksSamus or
                PhantoonAiFunction.TrackSamusDuringFlameRain)
            {
                healthBeforeLethalShot = body.Health;
                ushort targetY = function == PhantoonAiFunction.EyeTracksSamus
                    ? unchecked((ushort)(body.YPosition + 30))
                    : body.YPosition;
                ArmPhantoonProjectile(
                    samusShots.Slots[0],
                    body.XPosition,
                    targetY,
                    type: 0x0200,
                    damage: 2000);
                lethalHitCount = enemies.ResolvePhantoonProjectileHits(
                    bus,
                    samusShots,
                    sharedProjectiles);
                firedLethalShot = lethalHitCount == 1;
                function = (PhantoonAiFunction)body.VariableF;
                deathFunctions.Add(function);
            }

            maximumMosaic = Math.Max(maximumMosaic, state.MosaicRegister);
            enemies.StepEnemyProjectiles(
                assets.LevelData,
                samus,
                cameraX: 0,
                cameraY: 0,
                nmiFrameCounter8: nmi);
            enemies.DrawLayers(new OamBuffer(), 0, 0, firstLayer: 0, lastLayer: 7);

            if (state.BossDefeatPersisted)
                break;
        }

        bool powerPaletteMatches = true;
        for (int color = 0; color < 112; color++)
        {
            ushort expected = (ushort)(bus.ReadByte(0xa7ca61 + color * 2) |
                (bus.ReadByte(0xa7ca62 + color * 2) << 8));
            powerPaletteMatches &= cgram.Colors[color] == expected;
        }
        bool allPartsDeleted = new[] { body, state.Eye, state.Tentacles, state.Mouth }
            .All(part => part.Properties.HasAny(EnemyProperties.Deleted));

        PhantoonAiFunction[] requiredDeathFunctions =
        [
            PhantoonAiFunction.DyingFadeInOut,
            PhantoonAiFunction.DyingExplosions,
            PhantoonAiFunction.BeginFinalWavyDeath,
            PhantoonAiFunction.DyingFadeOut,
            PhantoonAiFunction.AlmostDead,
            PhantoonAiFunction.Dead,
        ];
        if (!firedLethalShot || lethalHitCount != 1 || healthBeforeLethalShot == 0 ||
            body.Health != 0 || requiredDeathFunctions.Any(f => !deathFunctions.Contains(f)) ||
            state.DeathExplosionRequests != 29 || state.DeathExplosionsSpawned == 0 ||
            maximumMosaic != 0xf2 || !state.MainScreenBg2Enabled ||
            !state.ItemDropRequested || !state.BossDefeatPersisted || !bossBitSet ||
            !state.WreckedShipPowerPaletteComplete || !powerPaletteMatches ||
            !allPartsDeleted || state.BossDoorPlmRequest != 0xb78b ||
            !sawPostBattleMusic || vram.ReadWord(0x4800) != 0x0338 ||
            vram.ReadWord(0x49ff) != 0x0338)
        {
            throw new InvalidDataException(
                $"Phantoon death mismatch after {deathFrame} frames: fired/hits=" +
                $"{firedLethalShot}/{lethalHitCount}, health={healthBeforeLethalShot}->{body.Health}, " +
                $"functions={string.Join(',', deathFunctions.Select(f => $"${(ushort)f:X4}"))}, " +
                $"explosions={state.DeathExplosionsSpawned}/{state.DeathExplosionRequests}, " +
                $"mosaic=${maximumMosaic:X2}, BG2/item/boss=" +
                $"{state.MainScreenBg2Enabled}/{state.ItemDropRequested}/" +
                $"{state.BossDefeatPersisted}, callback={bossBitSet}, palette={powerPaletteMatches}, " +
                $"deleted={allPartsDeleted}, door={state.BossDoorPlmRequest}, " +
                $"music={state.MusicRequest}, BG words=${vram.ReadWord(0x4800):X4}/" +
                $"${vram.ReadWord(0x49ff):X4}.");
        }

        // Contact and shot tests each consume a flame and would alter the timing of the
        // long-form fight above. Run them in independent copies of the same retail room so
        // both common dispatchers receive an actor genuinely produced by Phantoon AI while
        // the complete no-interference boss route remains authoritative.
        (int contactProducerFrame, int shotProducerFrame, int shotAnimationMaps) =
            VerifyDestroyableFlameInteractions(bus, room, assets);

        Console.WriteLine(
            $"Phantoon audit passed through combat, rage, and Wrecked Ship activation " +
            $"after {frame + rageFrame + deathFrame} frames: " +
            "retail 1x1 room/four-part population, cleared BG2 surface, independent body/eye/" +
            "tentacle/mouth lists, eight physical starting flames, activation/orbit contraction, " +
            "health palette materialization, delayed music, ROM figure-eight/eye timing, fade-out/" +
            "placement/vulnerable phases, eight packed rain columns with staggered motion, authored " +
            $"extended hitboxes/vulnerability damage, natural casual-flame contact at frame " +
            $"{contactProducerFrame}, shot destruction at frame {shotProducerFrame} across " +
            $"{shotAnimationMaps} response maps, eight alternating radial rage waves, ten death " +
            "fades, 29 explosion requests, wavy mosaic, power palette, boss bit, door, and music.");
        return 0;
    }

    /// <summary>
    /// Proves both interactive halves of definition <c>$86:9C29</c>. Casual flames start
    /// with cartridge properties masked to <c>$2028</c>; only a real floor collision may
    /// promote them to damage-enabled, shot-blocking actors. Two isolated encounter copies
    /// let contact consume one actor and a power-beam collision consume another.
    /// </summary>
    private static (int ContactProducerFrame, int ShotProducerFrame, int ShotAnimationMaps)
        VerifyDestroyableFlameInteractions(
            SuperMetroidAddressSpace bus,
            CartridgeRoomHeader room,
            CartridgeRoomAssets assets)
    {
        RoomEnemySystem contactEnemies = LoadInteractionProbe(
            bus,
            room,
            assets,
            randomSeed: 0x1234,
            out SamusState contactSamus);
        (RoomEnemyProjectileSlot contactFlame, int contactFrame) =
            AdvanceToInteractiveCasualFlame(contactEnemies, contactSamus, assets.LevelData);
        EnemyProjectileAuditAssertions.VerifyNaturalSamusContact(
            bus,
            contactEnemies,
            contactSamus,
            new SamusBombProjectileSystem(),
            assets.LevelData,
            contactFlame,
            cameraX: 0,
            cameraY: 0,
            frame: unchecked((byte)contactFrame));

        RoomEnemySystem shotEnemies = LoadInteractionProbe(
            bus,
            room,
            assets,
            randomSeed: 0x1234,
            out SamusState shotSamus);
        (RoomEnemyProjectileSlot shotFlame, int shotFrame) =
            AdvanceToInteractiveCasualFlame(shotEnemies, shotSamus, assets.LevelData);
        ushort shotX = shotFlame.XPosition;
        ushort shotY = shotFlame.YPosition;
        ushort shotResponse = EnemyProjectileAuditAssertions.VerifyNaturalDestructibleSamusShot(
            bus,
            shotEnemies,
            new SamusProjectileSystem(),
            new SamusBombProjectileSystem(),
            shotFlame);

        // Continue the cartridge response list rather than accepting dispatcher state as a
        // proxy for behavior. Every non-sentinel map is retained, and the actor must reach
        // its authored terminal delete command within a deliberately generous bound.
        var shotMaps = new HashSet<ushort>();
        // Drop selection reads current health/ammo. Keep that real actor available,
        // outside projectile contact range, instead of passing an absent Samus.
        shotSamus.XPosition = shotSamus.YPosition = 1024;
        for (int responseFrame = 0; responseFrame < 128 && shotFlame.IsActive; responseFrame++)
        {
            shotEnemies.StepEnemyProjectiles(
                assets.LevelData,
                samus: shotSamus,
                cameraX: 0,
                cameraY: 0,
                nmiFrameCounter8: unchecked((byte)(shotFrame + responseFrame + 1)));
            if (shotFlame.IsActive && shotFlame.SpritemapPointer is not 0 and not 0x8000)
                shotMaps.Add(shotFlame.SpritemapPointer);
        }

        PhantoonFlameDropRequest? drop = shotEnemies.PhantoonFlameDropRequests.Count == 1
            ? shotEnemies.PhantoonFlameDropRequests[0]
            : null;
        if (shotFlame.IsActive || shotMaps.Count == 0 || drop is null ||
            drop.Value.X != shotX || drop.Value.Y != shotY ||
            drop.Value.EnemyDefinitionPointer != 0xe4ff ||
            drop.Value.ItemDropChancesPointer != 0xf43a)
        {
            throw new InvalidDataException(
                $"Phantoon destroyable flame shot response $86:{shotResponse:X4} ended with " +
                $"live={shotFlame.IsActive} after {shotMaps.Count} visible maps; drop=" +
                (drop is null
                    ? "none."
                    : $"(${drop.Value.X},{drop.Value.Y}) header/table=" +
                      $"${drop.Value.EnemyDefinitionPointer:X4}/" +
                      $"${drop.Value.ItemDropChancesPointer:X4}."));
        }

        return (contactFrame, shotFrame, shotMaps.Count);
    }

    /// <summary>
    /// Advances an untouched Phantoon encounter until the same physical casual-flame slot
    /// has first been observed falling under <c>$86:9981</c> and then enabled collision by
    /// striking real room terrain. This forbids a rain/rage flame from accidentally making
    /// the focused interaction test pass without covering the conditional property change.
    /// </summary>
    private static (RoomEnemyProjectileSlot Flame, int Frame) AdvanceToInteractiveCasualFlame(
        RoomEnemySystem enemies,
        SamusState samus,
        RoomLevelData level)
    {
        var observedFallingSlots = new HashSet<int>();
        for (int frame = 0; frame < 10000; frame++)
        {
            byte nmi = unchecked((byte)frame);
            enemies.StepFrame(
                cameraX: 0,
                cameraY: 0,
                timeIsFrozen: false,
                samus,
                level: level,
                nmiFrameCounter8: nmi);

            foreach (RoomEnemyProjectileSlot projectile in enemies.EnemyProjectiles)
            {
                if (projectile.Kind == RoomEnemyProjectileKind.PhantoonDestroyableFlame &&
                    projectile.PreInstruction == 0x9981)
                {
                    observedFallingSlots.Add(projectile.SlotIndex);
                }
            }

            // A null Samus deliberately disables only common projectile contact. Phantoon's
            // producer still receives the normal Samus state above for targeting and state
            // selection, while the candidate survives long enough to be audited.
            enemies.StepEnemyProjectiles(
                level,
                samus: null,
                cameraX: 0,
                cameraY: 0,
                nmiFrameCounter8: nmi);

            RoomEnemyProjectileSlot? interactive = enemies.EnemyProjectiles.FirstOrDefault(
                projectile =>
                    projectile.Kind == RoomEnemyProjectileKind.PhantoonDestroyableFlame &&
                    observedFallingSlots.Contains(projectile.SlotIndex) &&
                    projectile.CanDamageSamus && projectile.BlocksSamusProjectiles &&
                    projectile.CollisionOption == 0);
            if (interactive is not null)
                return (interactive, frame);
        }

        throw new InvalidDataException(
            "Phantoon produced no casual flame that transitioned from falling to interactive.");
    }

    /// <summary>Builds a fresh copy of the retail encounter for a destructive probe.</summary>
    private static RoomEnemySystem LoadInteractionProbe(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        ushort randomSeed,
        out SamusState samus)
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState(randomSeed);
        samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            XPosition = 128,
            YPosition = 192,
            Pose = SamusPoseIds.FacingRightNormalPose,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);

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
            isAreaBossDefeated: () => false,
            setAreaBossDefeated: () => { });
        return enemies;
    }

    private static void ArmPhantoonProjectile(
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

    private static bool PhantoonPaletteMatchesFullHealth(
        SuperMetroidAddressSpace bus,
        SnesCgram cgram) =>
        PhantoonPaletteMatchesHealth(bus, cgram, health: 2500);

    private static bool PhantoonPaletteIsWhite(SnesCgram cgram)
    {
        for (int color = 0; color < 16; color++)
        {
            if (cgram.Colors[112 + color] != 0x7fff)
                return false;
        }
        return true;
    }

    private static bool PhantoonPaletteMatchesHealth(
        SuperMetroidAddressSpace bus,
        SnesCgram cgram,
        ushort health)
    {
        int healthBand = Math.Min(7, Math.Max(0, (health - 1) / 312));
        int palette = 0xa7cb41 + healthBand * 32;
        for (int color = 0; color < 16; color++)
        {
            ushort expected = (ushort)(bus.ReadByte(palette + color * 2) |
                (bus.ReadByte(palette + color * 2 + 1) << 8));
            if (cgram.Colors[112 + color] != expected)
                return false;
        }
        return true;
    }
}
