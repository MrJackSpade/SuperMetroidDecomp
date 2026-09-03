using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Retail-ROM regression for Crocomire's multipart load, extended hitboxes, fight bytecode,
/// movement callbacks, mouth damage response, power-bomb reaction, and bank-$86 projectile.
/// </summary>
internal static class CrocomireAudit
{
    private const ushort RoomHeader = 0xa98d;
    private const ushort NormalRoomState = 0xa99f;
    private const ushort Population = 0xbb0e;
    private const ushort BodyDefinition = 0xddbf;
    private const ushort TongueDefinition = 0xddff;
    private const ushort MouthShotCallback = 0xba05;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        VerifyHeadersAndPopulation(bus);

        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, RoomHeader);
        if (room.State.Pointer != NormalRoomState ||
            room.State.EnemyPopulationPointer != Population)
        {
            throw new InvalidDataException(
                $"Crocomire room selected state/population ${room.State.Pointer:X4}/" +
                $"${room.State.EnemyPopulationPointer:X4}.");
        }
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);

        VerifyInitializationAndWake(bus, room, assets);
        VerifyInstructionMovementAndProjectile(bus, room, assets);
        VerifyMouthAndPowerBombReactions(bus, room, assets);
        VerifyCompleteDeathSequence(bus, room, assets);

        Console.WriteLine(
            "Crocomire audit passed: retail body/tongue records, palette/list setup, " +
            "extended-map wake animation, four-pixel instruction movement, nine-shot " +
            "projectile cadence/vector setup, charged-beam and normal-bomb mouth reactions, " +
            "and the retail " +
            "power-bomb vulnerability gate agreed; bridge collapse, both melting passes, " +
            "skeleton wall break, spike debris, item drop, and boss completion also ran " +
            "end-to-end through the cartridge state graph.");
        return 0;
    }

    private static void VerifyHeadersAndPopulation(SuperMetroidAddressSpace bus)
    {
        RoomEnemyDefinition body = RoomEnemySystem.ReadDefinition(bus, BodyDefinition);
        if (body.Health != 0x7fff || body.Damage != 40 || body.Bank != 0xa4 ||
            body.InitializationAiPointer != 0x8a5a || body.MainAiPointer != 0x8c04 ||
            body.HurtAiPointer != 0x8687 || body.TouchAiPointer != 0xb950 ||
            body.ShotAiPointer != 0)
        {
            throw new InvalidDataException("Crocomire $DDBF header disagrees with bank $A4.");
        }

        RoomEnemyDefinition tongue = RoomEnemySystem.ReadDefinition(bus, TongueDefinition);
        if (tongue.Bank != 0xa4 || tongue.InitializationAiPointer != 0xf67a ||
            tongue.MainAiPointer != 0xf6bb || tongue.TouchAiPointer != 0x8023 ||
            tongue.ShotAiPointer != 0x802d)
        {
            throw new InvalidDataException("Crocomire tongue $DDFF header disagrees with bank $A4.");
        }

        ushort[] expected =
        [
            0xddbf, 0x0480, 0x0078, 0xbd2a, 0xa800, 0x0004, 0x0000, 0x0000,
            0xddff, 0x0480, 0x0078, 0xbd2a, 0xa800, 0x0004, 0x0000, 0x0000,
            0xffff,
        ];
        for (int word = 0; word < expected.Length; word++)
        {
            ushort actual = ReadWord(bus, 0xa10000 | (Population + word * 2));
            if (actual != expected[word])
            {
                throw new InvalidDataException(
                    $"Crocomire population word {word} was ${actual:X4}, " +
                    $"expected ${expected[word]:X4}.");
            }
        }
    }

    private static void VerifyInitializationAndWake(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedCrocomire loaded = Load(bus, room, assets);
        RoomEnemySlot body = loaded.Enemies.Slots[0];
        RoomEnemySlot tongue = loaded.Enemies.Slots[1];
        CrocomireEnemyState state = RequireState(loaded);
        if (loaded.Enemies.BossId != 6 || loaded.Enemies.EnemyCount != 2 ||
            body.EnemyDefinitionPointer != BodyDefinition ||
            body.CurrentInstruction != 0xbade || body.InstructionTimer != 1 ||
            state.DeathSequenceIndex != 0 ||
            state.FightFunction != CrocomireFightFunction.Sleeping ||
            !body.ExtraProperties.HasAny(EnemyExtraProperties.UsesExtendedSpritemap) ||
            tongue.EnemyDefinitionPointer != TongueDefinition ||
            tongue.CurrentInstruction != 0xbe56 || tongue.VariableA != 23 ||
            tongue.PaletteIndex != 0x0e00 || state.Tongue != tongue)
        {
            throw new InvalidDataException(
                $"Crocomire initialization failed: body list=${body.CurrentInstruction:X4}, " +
                $"fight={state.FightFunction}, tongue list/palette=" +
                $"${tongue.CurrentInstruction:X4}/${tongue.PaletteIndex:X4}.");
        }

        // The inclusive $20..0 palette loop copies seventeen words to each target row.
        for (int color = 0; color < 17; color++)
        {
            if (loaded.Cgram.Colors[160 + color] != ReadWord(bus, 0xa4b8bd + color * 2) ||
                loaded.Cgram.Colors[208 + color] != ReadWord(bus, 0xa4b8dd + color * 2))
            {
                throw new InvalidDataException($"Crocomire palette copy failed at color {color}.");
            }
        }

        // Frame one installs BADE's initial timed map; frame two reaches $86A6 and can
        // switch to the first-damage list because Samus is inside the 224-pixel wake range.
        Step(loaded);
        Step(loaded);
        if (state.FightFunction != CrocomireFightFunction.WaitingForFirstDamage ||
            body.SpritemapPointer == 0x804f || (body.SpritemapPointer & 0x8000) == 0)
        {
            throw new InvalidDataException(
                $"Crocomire did not wake into a mouth-bearing extended map: " +
                $"fight={state.FightFunction}, map=${body.SpritemapPointer:X4}.");
        }
    }

    private static void VerifyInstructionMovementAndProjectile(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedCrocomire movement = Load(bus, room, assets);
        CrocomireEnemyState movementState = RequireState(movement);
        RoomEnemySlot movingBody = movementState.Body;
        ushort startX = movingBody.XPosition;
        movementState.FightFunction = CrocomireFightFunction.Sleeping;
        movingBody.CurrentInstruction = 0xbbf0; // $8FDF, then fight AI at $BBF2.
        movingBody.InstructionTimer = 1;
        Step(movement);
        if (movingBody.XPosition != startX - 4)
        {
            throw new InvalidDataException(
                $"Crocomire move-left instruction changed X ${startX:X4}->" +
                $"${movingBody.XPosition:X4}, not four pixels.");
        }

        LoadedCrocomire volley = Load(bus, room, assets);
        CrocomireEnemyState volleyState = RequireState(volley);
        volleyState.FightFunction = CrocomireFightFunction.ProjectileAttack;
        volleyState.ProjectileCounter = 0;
        volleyState.Body.CurrentInstruction = 0xbb94; // Fight-AI opcode inside volley loop.
        volleyState.Body.InstructionTimer = 1;
        Step(volley);
        RoomEnemyProjectileSlot projectile = volley.Enemies.EnemyProjectiles.Single(
            candidate => candidate.Kind == RoomEnemyProjectileKind.CrocomireProjectile);
        if (volleyState.ProjectileCounter != 2 || projectile.DirectionParameter != 2 ||
            projectile.XVelocity != 0xfe00 || projectile.PreInstruction != 0x906b)
        {
            throw new InvalidDataException(
                $"Crocomire volley spawn failed: counter={volleyState.ProjectileCounter}, " +
                $"parameter={projectile.DirectionParameter}, velocity/pre=" +
                $"${projectile.XVelocity:X4}/${projectile.PreInstruction:X4}.");
        }

        volley.Enemies.StepEnemyProjectiles(
            volley.Level,
            volley.Samus,
            cameraX: 0x0400,
            cameraY: 0);
        if (!projectile.IsActive || projectile.PreInstruction != 0x90b3 ||
            projectile.XVelocity == 0xfe00)
        {
            throw new InvalidDataException(
                $"Crocomire projectile setup failed: active={projectile.IsActive}, " +
                $"velocity=(${projectile.XVelocity:X4},${projectile.YVelocity:X4}), " +
                $"pre=${projectile.PreInstruction:X4}.");
        }

        // The exhaustive ordinary-AI sweep cannot naturally reach Crocomire's explicit
        // projectile-attack state. Close that residual here with the real actor spawned
        // above: first prove SpawnEprojInner copied this ROM definition verbatim, then use
        // the shared domain assertion to exercise common Samus damage and deletion.
        int definition = 0x860000 | (ushort)RoomEnemyProjectileKind.CrocomireProjectile;
        ushort radii = ReadWord(bus, definition + 6);
        ushort properties = ReadWord(bus, definition + 8);
        if (projectile.XRadius != unchecked((byte)radii) ||
            projectile.YRadius != unchecked((byte)(radii >> 8)) ||
            projectile.Damage != (properties & 0x0fff) ||
            projectile.CanDamageSamus != ((properties & 0x2000) == 0) ||
            projectile.PersistsOnSamusContact != ((properties & 0x4000) != 0))
        {
            throw new InvalidDataException(
                $"Crocomire projectile definition copy diverged: radius=" +
                $"{projectile.XRadius}x{projectile.YRadius}/${radii:X4}, damage=" +
                $"{projectile.Damage}/${properties & 0x0fff}, collision/persistence=" +
                $"{projectile.CanDamageSamus}/{projectile.PersistsOnSamusContact}.");
        }

        EnemyProjectileAuditAssertions.VerifyNaturalSamusContact(
            bus,
            volley.Enemies,
            volley.Samus,
            new SamusBombProjectileSystem(),
            volley.Level,
            projectile,
            cameraX: 0x0400,
            cameraY: 0);
    }

    private static void VerifyMouthAndPowerBombReactions(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedCrocomire mouth = Load(bus, room, assets);
        CrocomireEnemyState mouthState = RequireState(mouth);
        (ushort mouthX, ushort mouthY) = AdvanceUntilHitbox(
            bus,
            mouth,
            MouthShotCallback);

        var projectiles = new SamusProjectileSystem();
        var shared = new SamusBombProjectileSystem();
        SamusProjectileSlot chargedBeam = projectiles.Slots[0];
        chargedBeam.ClearFields();
        chargedBeam.Type = 0x0010;
        chargedBeam.Damage = 20;
        chargedBeam.Direction = 2;
        chargedBeam.XPosition = mouthX;
        chargedBeam.YPosition = mouthY;
        chargedBeam.XRadius = 1;
        chargedBeam.YRadius = 1;
        chargedBeam.InstructionPointer = 0x9000;
        chargedBeam.InstructionTimer = 1;
        int hits = mouth.Enemies.ResolveOrdinaryProjectileHits(
            bus,
            projectiles,
            shared,
            mouth.Samus);
        if (hits != 1 || mouthState.StepCounter != 2 ||
            (mouthState.FightFlags & 0x0800) == 0 || mouthState.Body.FlashTimer != 14)
        {
            throw new InvalidDataException(
                $"Crocomire charged mouth hit failed: hits={hits}, steps=" +
                $"{mouthState.StepCounter}, flags=${mouthState.FightFlags:X4}, " +
                $"flash={mouthState.Body.FlashTimer}.");
        }

        LoadedCrocomire normalBomb = Load(bus, room, assets);
        CrocomireEnemyState normalBombState = RequireState(normalBomb);
        (ushort bombX, ushort bombY) = AdvanceUntilHitbox(
            bus,
            normalBomb,
            MouthShotCallback);
        RoomEnemySlot normalBombBody = normalBombState.Body;
        ushort healthBeforeBomb = normalBombBody.Health;
        ushort stepsBeforeBomb = normalBombState.StepCounter;
        ushort flagsBeforeBomb = normalBombState.FightFlags;
        ushort timerBeforeBomb = normalBombState.ReactionTimer;
        ushort flashBeforeBomb = normalBombBody.FlashTimer;

        // `$A4:BA05` treats family $0500 as a zero-step mouth hit. It installs the
        // fourteen-frame hurt flash, but must not apply vulnerability damage or publish
        // Crocomire's beam/missile push reaction.
        var bombProjectiles = new SamusBombProjectileSystem();
        var ordinaryProjectiles = new SamusProjectileSystem();
        SamusBombProjectileSlot physicalBomb =
            EnemyProjectileAuditAssertions.ArmExplodingNormalBomb(
                bombProjectiles,
                bombX,
                bombY,
                damage: 1000);
        // Keep the probe inside the selected mouth rectangle. A large explosion radius can
        // legitimately encounter an earlier component and dispatch that component instead.
        physicalBomb.XRadius = 1;
        physicalBomb.YRadius = 1;
        int bombHits = normalBomb.Enemies.ResolveOrdinaryBombHits(
            bombProjectiles,
            ordinaryProjectiles,
            normalBomb.Samus);
        if (bombHits != 1 || (physicalBomb.Direction & 0x0010) == 0 ||
            normalBombBody.Health != healthBeforeBomb ||
            normalBombState.StepCounter != stepsBeforeBomb ||
            normalBombState.FightFlags != flagsBeforeBomb ||
            normalBombState.ReactionTimer != timerBeforeBomb ||
            normalBombBody.InvincibilityTimer != 0 ||
            normalBombBody.FlashTimer != unchecked((ushort)(flashBeforeBomb + 14)) ||
            (normalBombBody.AiHandlerBits & 0x0002) == 0)
        {
            throw new InvalidDataException(
                $"Crocomire normal-bomb mouth reaction failed: hits={bombHits}, " +
                $"marked={(physicalBomb.Direction & 0x0010) != 0}, health=" +
                $"{healthBeforeBomb}->{normalBombBody.Health}, steps/flags/timer=" +
                $"{normalBombState.StepCounter}/${normalBombState.FightFlags:X4}/" +
                $"{normalBombState.ReactionTimer}, flash/invinc=" +
                $"{normalBombBody.FlashTimer}/{normalBombBody.InvincibilityTimer}.");
        }

        LoadedCrocomire powerBomb = Load(bus, room, assets);
        Step(powerBomb);
        Step(powerBomb);
        CrocomireEnemyState powerBombState = RequireState(powerBomb);
        RoomEnemySlot powerBombBody = powerBombState.Body;
        RoomEnemySlot powerBombTongue = powerBombState.Tongue ??
            throw new InvalidDataException("Crocomire power-bomb audit lost the tongue record.");
        ushort bodyHealthBeforePowerBomb = powerBombBody.Health;
        ushort tongueHealthBeforePowerBomb = powerBombTongue.Health;
        int reactions = powerBomb.Enemies.ResolveOrdinaryPowerBombHits(
            bus,
            powerBombState.Body.XPosition,
            powerBombState.Body.YPosition,
            explosionRadius: byte.MaxValue,
            powerBomb.Samus);
        // Both definitions point at vulnerability record $F102, whose power-bomb byte at
        // $F111 is $82. Native $A0:A306 masks bit seven, admits both physical records, and
        // sets process-off-screen on each. The body dispatches private $A4:B992 without HP
        // damage; the tongue has a zero callback and therefore receives ordinary 2*100 HP
        // damage. The prior assertion was based on the adjacent $F101/$F110 bytes.
        ushort expectedTongueHealth = tongueHealthBeforePowerBomb <= 200
            ? (ushort)0
            : unchecked((ushort)(tongueHealthBeforePowerBomb - 200));
        ushort expectedTongueFlash = unchecked((ushort)(
            (powerBombTongue.HurtAiTime == 0 ? 4 : powerBombTongue.HurtAiTime) + 8));
        if (bus.ReadByte(0xb4f111) != 0x82 || reactions != 2 ||
            powerBombState.FightFunction != CrocomireFightFunction.PowerBombCharge ||
            powerBombState.StepCounter != 3 || powerBombState.ReactionTimer != 10 ||
            (powerBombState.FightFlags & 0x8000) == 0 ||
            powerBombBody.Health != bodyHealthBeforePowerBomb ||
            powerBombBody.FlashTimer != 4 || powerBombBody.InvincibilityTimer != 0 ||
            !powerBombBody.Properties.HasAny(EnemyProperties.ProcessOffScreen) ||
            powerBombTongue.Health != expectedTongueHealth ||
            powerBombTongue.InvincibilityTimer != 48 ||
            powerBombTongue.FlashTimer != expectedTongueFlash ||
            !powerBombTongue.Properties.HasAny(EnemyProperties.ProcessOffScreen))
        {
            throw new InvalidDataException(
                $"Crocomire power-bomb reaction failed: reactions={reactions}, " +
                $"fight={powerBombState.FightFunction}, steps={powerBombState.StepCounter}, " +
                $"timer/flags={powerBombState.ReactionTimer}/${powerBombState.FightFlags:X4}, " +
                $"body health/flash/invinc={bodyHealthBeforePowerBomb}->" +
                $"{powerBombBody.Health}/{powerBombBody.FlashTimer}/" +
                $"{powerBombBody.InvincibilityTimer}, tongue={tongueHealthBeforePowerBomb}->" +
                $"{powerBombTongue.Health}/{powerBombTongue.FlashTimer}/" +
                $"{powerBombTongue.InvincibilityTimer}.");
        }
    }

    private static void VerifyCompleteDeathSequence(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedCrocomire loaded = Load(bus, room, assets);
        CrocomireEnemyState state = RequireState(loaded);
        CrocomireDeathState death = loaded.Enemies.CrocomireDeath ??
            throw new InvalidDataException("Crocomire death extension was not initialized.");

        // `$A4:8D5E` begins the graph at the exact bridge threshold. This first frame must
        // publish all ten clear-block PLMs plus the invisible wall before state two runs.
        state.Body.XPosition = 0x0640;
        Step(loaded);
        if (state.DeathSequenceIndex != 0x0002 ||
            !loaded.Enemies.CrocomireBridgeCollapseStarted ||
            loaded.Enemies.CrocomirePlmRequests.Count != 11 ||
            loaded.Enemies.CrocomirePlmRequests.Count(request => request.Header == 0xb74f) != 10 ||
            !loaded.Enemies.CrocomirePlmRequests.Contains(
                new CrocomirePlmRequest(0x4e, 0x03, 0xb757)) ||
            state.Tongue is not { } tongue ||
            !tongue.Properties.HasAny(EnemyProperties.Invisible))
        {
            throw new InvalidDataException(
                $"Crocomire bridge transition failed: state=${state.DeathSequenceIndex:X2}, " +
                $"PLMs={loaded.Enemies.CrocomirePlmRequests.Count}, " +
                $"tongue properties=${state.Tongue?.Properties ?? 0:X4}.");
        }

        var visited = new HashSet<ushort> { state.DeathSequenceIndex };
        var publishedPlmHeaders = new HashSet<ushort>(
            loaded.Enemies.CrocomirePlmRequests.Select(request => request.Header));
        bool sawBridgeFragment = false;
        bool sawSpikeWallPiece = false;
        bool sawNonUniformMeltingScroll = false;
        bool sawItemDrop = false;
        bool sawDeathMusic = false;
        var oam = new OamBuffer();

        const int maximumFrames = 20000;
        int frame;
        for (frame = 0; frame < maximumFrames && state.DeathSequenceIndex != 0x0052; frame++)
        {
            // State $3E waits for Samus to return to the left side of the wall. The retail
            // room normally achieves this through player movement; move the audit actor at
            // that explicit gate without bypassing any boss state.
            if (state.DeathSequenceIndex == 0x003e)
                loaded.Samus.XPosition = 0x0270;

            Step(loaded);
            visited.Add(state.DeathSequenceIndex);
            foreach (CrocomirePlmRequest request in loaded.Enemies.CrocomirePlmRequests)
                publishedPlmHeaders.Add(request.Header);
            sawItemDrop |= loaded.Enemies.LastCrocomireDropRequest is not null;
            sawDeathMusic |= loaded.Enemies.LastCrocomireMusicRequest is not null;

            // ProcessExtendedTilemap belongs to the draw pass, not EnemyMain. Exercising it
            // every frame proves the melting tilemaps reach modeled BG2 VRAM instead of only
            // changing typed debugger state.
            oam.BeginFrame();
            loaded.Enemies.DrawLayers(oam, 0x0400, 0, firstLayer: 0, lastLayer: 7);
            loaded.Enemies.StepEnemyProjectiles(
                loaded.Level,
                loaded.Samus,
                cameraX: 0x0400,
                cameraY: 0);

            sawBridgeFragment |= loaded.Enemies.EnemyProjectiles.Any(
                projectile => projectile.Kind == RoomEnemyProjectileKind.CrocomireBridgeCrumbling);
            sawSpikeWallPiece |= loaded.Enemies.EnemyProjectiles.Any(
                projectile => projectile.Kind == RoomEnemyProjectileKind.CrocomireSpikeWallPieces);
            ushort firstScroll = death.Bg2ScrollByScanline[0];
            sawNonUniformMeltingScroll |= death.Bg2ScrollByScanline.Any(
                scroll => scroll != firstScroll);
        }

        ushort[] requiredStates =
        [
            0x02, 0x04, 0x06, 0x08, 0x0a, 0x0c, 0x0e,
            0x10, 0x12, 0x14, 0x16, 0x18, 0x1a, 0x1c,
            0x1e, 0x20, 0x22, 0x24, 0x26, 0x28, 0x2a,
            0x2c, 0x2e, 0x30, 0x32, 0x34, 0x36, 0x38, 0x3a, 0x3c,
            0x58, 0x3e, 0x40, 0x42, 0x44, 0x46, 0x48, 0x4a, 0x4c,
            0x4e, 0x50, 0x52,
        ];
        ushort[] missingStates = requiredStates.Where(required => !visited.Contains(required)).ToArray();
        ushort[] requiredHeaders = [0xb747, 0xb74f, 0xb753, 0xb757];
        ushort[] missingHeaders = requiredHeaders
            .Where(required => !publishedPlmHeaders.Contains(required))
            .ToArray();
        // $A4:9697 contains exactly 49 authored X columns (0..48). The remaining typed
        // scratch bytes model adjacent WRAM safety, not visible Crocomire pixels.
        bool allColumnsMelted = death.MeltingColumnHeights
            .Take(49)
            .All(height => height == 48);
        if (frame == maximumFrames || state.DeathSequenceIndex != 0x0052 ||
            missingStates.Length != 0 || missingHeaders.Length != 0 ||
            !sawBridgeFragment || !sawSpikeWallPiece || !sawNonUniformMeltingScroll ||
            !allColumnsMelted || !sawItemDrop || !sawDeathMusic ||
            !loaded.IsMiniBossDefeated())
        {
            throw new InvalidDataException(
                $"Crocomire death graph failed after {frame} frames at " +
                $"state ${state.DeathSequenceIndex:X2}: missing states=" +
                $"[{string.Join(',', missingStates.Select(value => value.ToString("X2")))}], " +
                $"missing PLMs=[{string.Join(',', missingHeaders.Select(value => value.ToString("X4")))}], " +
                $"bridge={sawBridgeFragment}, spikes={sawSpikeWallPiece}, " +
                $"HDMA={sawNonUniformMeltingScroll}, columns={allColumnsMelted}, " +
                $"drop={sawItemDrop}, music={sawDeathMusic}, boss={loaded.IsMiniBossDefeated()}.");
        }
    }

    private static LoadedCrocomire Load(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var system = new Bank80SystemState();
        bool miniBossDefeated = false;
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusPoseIds.FacingRightNormalPose,
            XPosition = 0x0440,
            YPosition = 0x0078,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);

        var enemies = new RoomEnemySystem();
        enemies.Load(
            bus,
            Population,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            system.NextRandom,
            system.SetRandomNumber,
            readRandomNumber: () => system.RandomNumber,
            level: assets.LevelData,
            samus: samus,
            cameraX: 0x0400,
            isAreaMiniBossDefeated: () => miniBossDefeated,
            setAreaMiniBossDefeated: () => miniBossDefeated = true);
        return new LoadedCrocomire(
            enemies,
            samus,
            cgram,
            assets.LevelData,
            () => miniBossDefeated);
    }

    private static void Step(LoadedCrocomire loaded) =>
        loaded.Enemies.StepFrame(
            cameraX: 0x0400,
            cameraY: 0,
            timeIsFrozen: false,
            loaded.Samus,
            level: loaded.Level);

    private static CrocomireEnemyState RequireState(LoadedCrocomire loaded) =>
        loaded.Enemies.Crocomire ??
        throw new InvalidDataException("Crocomire body did not publish typed state.");

    private static (ushort X, ushort Y) AdvanceUntilHitbox(
        ISnesAddressSpace bus,
        LoadedCrocomire loaded,
        ushort shotCallback)
    {
        // The initial BADE instruction frame is empty. Wake the actor, then follow its real
        // instruction stream until one of the currently displayed components publishes the
        // requested callback; this keeps collision probes tied to cartridge-authored maps.
        Step(loaded);
        Step(loaded);
        CrocomireEnemyState state = RequireState(loaded);
        for (int frame = 0; frame < 360; frame++)
        {
            if (FindHitboxCenter(
                    bus,
                    state.Body,
                    shotCallback,
                    out ushort x,
                    out ushort y))
            {
                return (x, y);
            }
            Step(loaded);
        }

        throw new InvalidDataException(
            $"Crocomire displayed no hitbox with shot callback $A4:{shotCallback:X4}.");
    }

    private static bool FindHitboxCenter(
        ISnesAddressSpace bus,
        RoomEnemySlot actor,
        ushort shotCallback,
        out ushort x,
        out ushort y)
    {
        int bank = actor.Definition.Bank << 16;
        int map = bank | actor.SpritemapPointer;
        int componentCount = ReadWord(bus, map);
        for (int componentIndex = 0; componentIndex < componentCount; componentIndex++)
        {
            int component = map + 2 + componentIndex * 8;
            ushort componentX = unchecked((ushort)(
                actor.XPosition + ReadWord(bus, component)));
            ushort componentY = unchecked((ushort)(
                actor.YPosition + ReadWord(bus, component + 2)));
            int hitboxList = bank | ReadWord(bus, component + 6);
            int hitboxCount = ReadWord(bus, hitboxList);
            for (int hitboxIndex = 0; hitboxIndex < hitboxCount; hitboxIndex++)
            {
                int hitbox = hitboxList + 2 + hitboxIndex * 12;
                if (ReadWord(bus, hitbox + 10) != shotCallback)
                    continue;
                short left = unchecked((short)ReadWord(bus, hitbox));
                short top = unchecked((short)ReadWord(bus, hitbox + 2));
                short right = unchecked((short)ReadWord(bus, hitbox + 4));
                short bottom = unchecked((short)ReadWord(bus, hitbox + 6));
                x = unchecked((ushort)(componentX + (left + right) / 2));
                y = unchecked((ushort)(componentY + (top + bottom) / 2));
                return true;
            }
        }
        x = y = 0;
        return false;
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private sealed record LoadedCrocomire(
        RoomEnemySystem Enemies,
        SamusState Samus,
        SnesCgram Cgram,
        RoomLevelData Level,
        Func<bool> IsMiniBossDefeated);
}
