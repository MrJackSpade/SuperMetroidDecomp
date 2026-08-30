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
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, RoomPointer);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        if (room.WidthInScreens != 1 || room.HeightInScreens != 1 ||
            room.AreaIndex != 3 || room.State.EnemyPopulationPointer != PopulationPointer)
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
            Pose = SamusState.FacingRightNormalPose,
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
            state.MusicRequest != 5 || state.Mouth.Parameter1 != 1 ||
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
        // window. One ordinary Missile first requests the real swoop branch, allowing the
        // authored full-body touch boxes to damage Samus; a later Super Missile then crosses
        // the one-shot rage threshold without any audit-only state mutation.
        var samusShots = new SamusProjectileSystem();
        var sharedProjectiles = new SamusBombProjectileSystem();
        bool firedSwoopTrigger = false;
        int swoopTriggerHitCount = 0;
        bool sawSwoop = false;
        bool resolvedSwoopContact = false;
        ushort samusHealthBeforeContact = 0;
        ushort samusHealthAfterContact = 0;
        bool firedRageTrigger = false;
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
            if (!firedSwoopTrigger &&
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
                samus.Pose = SamusState.FacingRightNormalPose;
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
                sawClockwiseRageFlame |= projectile.XVelocity == 0x0003;
                sawCounterclockwiseRageFlame |= projectile.XVelocity == 0xfffd;
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

        if (!firedSwoopTrigger || swoopTriggerHitCount != 1 || !sawSwoop ||
            !resolvedSwoopContact || samusHealthBeforeContact - samusHealthAfterContact != 40 ||
            !firedRageTrigger || rageHitCount != 1 ||
            healthBeforeRageShot - body.Health != 600 ||
            state.LastProjectileDamage != 600 || state.AcceptedProjectileHits != 2 ||
            state.LastCombatSoundEffect != 0x0073 ||
            !sawRageFadeOut || !sawRage || !sawPostRageFade || maximumRageRound < 7 ||
            !sawClockwiseRageFlame || !sawCounterclockwiseRageFlame ||
            !sawWhiteDamagePalette || !sawDamagedHealthPalette)
        {
            throw new InvalidDataException(
                $"Phantoon rage route mismatch after {rageFrame} frames: swoop fired/hits/state/contact=" +
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
            state.MusicRequest != 3 || vram.ReadWord(0x4800) != 0x0338 ||
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

        Console.WriteLine(
            $"Phantoon audit passed through combat, rage, and Wrecked Ship activation " +
            $"after {frame + rageFrame + deathFrame} frames: " +
            "retail 1x1 room/four-part population, cleared BG2 surface, independent body/eye/" +
            "tentacle/mouth lists, eight physical starting flames, activation/orbit contraction, " +
            "health palette materialization, delayed music, ROM figure-eight/eye timing, fade-out/" +
            "placement/vulnerable phases, eight packed rain columns with staggered motion, authored " +
            "extended hitboxes/vulnerability damage, eight alternating radial rage waves, ten death " +
            "fades, 29 explosion requests, wavy mosaic, power palette, boss bit, door, and music.");
        return 0;
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
