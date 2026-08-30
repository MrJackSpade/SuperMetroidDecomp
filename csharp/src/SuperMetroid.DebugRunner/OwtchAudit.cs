using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Untouched-ROM audit for Owtch $D03F. Pseudo Plasma Spark supplies a real right-moving
/// actor alongside already-translated Choot and Skultera families, so successful loading
/// proves this test has not replaced the room population with a convenient synthetic one.
/// </summary>
internal static class OwtchAudit
{
    private const ushort RoomPointer = 0xd1dd;
    private const ushort DefinitionPointer = 0xd03f;
    private const ushort CameraX = 0x0100;
    private const ushort CameraY = 0x0200;
    private const int BombVulnerabilityAddress = 0xb4ee92;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, RoomPointer);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        VerifyHeader(bus);

        RoomEnemySlot? randomActor = null;
        bool requestBurial = false;
        ushort currentRandom = 0x0080;
        ushort NextRandom()
        {
            // $A2:A55B adds the already-incremented actor frame counter to the new RNG
            // word and keeps only its low byte. Cancel that counter to force either zero
            // (burial) or $80 (suppression) without weakening the production comparison.
            ushort frame = randomActor?.FrameCounter ?? 0;
            currentRandom = requestBurial
                ? unchecked((ushort)(0 - frame))
                : unchecked((ushort)(0x0080 - frame));
            return currentRandom;
        }

        var samus = CreateSamus(bus);
        var enemies = new RoomEnemySystem();
        enemies.Load(
            bus,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            NextRandom,
            readRandomNumber: () => currentRandom,
            level: assets.LevelData,
            samus: samus);

        RoomEnemySlot[] owtches = enemies.Slots
            .Take(enemies.EnemyCount)
            .Where(slot => slot.EnemyDefinitionPointer == DefinitionPointer)
            .ToArray();
        if (room.State.Pointer != 0xd1ea ||
            room.State.EnemyPopulationPointer != 0xd75b ||
            room.State.EnemyTilesetPointer != 0x8ed6 ||
            enemies.EnemyCount != 9 || owtches.Length != 1 ||
            enemies.OwtchStates[owtches[0].SlotIndex] is null)
        {
            throw new InvalidDataException(
                $"Pseudo Plasma Spark state=${room.State.Pointer:X4}, population=" +
                $"${room.State.EnemyPopulationPointer:X4}, set=" +
                $"${room.State.EnemyTilesetPointer:X4}, count={enemies.EnemyCount}, " +
                $"Owtches={owtches.Length}.");
        }

        RoomEnemySlot actor = owtches[0];
        OwtchEnemyState state = State(enemies, actor);
        randomActor = actor;
        if (actor.XPosition != 0x0180 || actor.YPosition != 0x02b8 ||
            actor.Parameter1 != 0x0101 || actor.Parameter2 != 0x0208 ||
            actor.CurrentInstruction != 0xa3bd || actor.SpritemapPointer != 0x804d ||
            actor.Health != 20 || state.Behavior != OwtchBehaviorState.MovingRight ||
            state.SinkYOffset != 0 || state.UndergroundTimer != 0x0040 ||
            state.MinimumXPosition != 0x0150 || state.MaximumXPosition != 0x01b0 ||
            state.RightVelocity != 0 || state.RightSubvelocity != 0x8000 ||
            state.LeftVelocity != 0xffff || state.LeftSubvelocity != 0x8000)
        {
            throw new InvalidDataException(
                $"Owtch initialization mismatch: position=(${actor.XPosition:X4}," +
                $"${actor.YPosition:X4}), params=${actor.Parameter1:X4}/" +
                $"${actor.Parameter2:X4}, bounds=${state.MinimumXPosition:X4}-" +
                $"${state.MaximumXPosition:X4}, speed=${state.LeftVelocity:X4}." +
                $"{state.LeftSubvelocity:X4}/${state.RightVelocity:X4}." +
                $"{state.RightSubvelocity:X4}, state={state.Behavior}. ");
        }

        // Remove unrelated actors only after proving the untouched nine-record population.
        // This keeps their independently translated AI from consuming the deterministic RNG
        // while the following assertions isolate Owtch's exact state transitions.
        foreach (RoomEnemySlot other in enemies.Slots.Take(enemies.EnemyCount))
        {
            if (!ReferenceEquals(other, actor))
                other.Properties = other.Properties.With(EnemyProperties.Deleted);
        }

        var maps = new HashSet<ushort>();
        for (int frame = 0; frame < 26; frame++)
        {
            enemies.StepFrame(CameraX, CameraY, false, samus, level: assets.LevelData);
            maps.Add(actor.SpritemapPointer);
        }
        ushort[] expectedMaps = [0xa589, 0xa590, 0xa597];
        if (!expectedMaps.All(maps.Contains))
        {
            throw new InvalidDataException(
                "Owtch's three-frame ROM animation was incomplete: " +
                string.Join(',', maps.Order()));
        }

        // Right movement adds $0000.8000 to $FFFF.F000, retaining carry into the whole
        // position, overshooting the maximum, and selecting state zero without clamping.
        actor.XPosition = state.MaximumXPosition;
        actor.XSubposition = 0xf000;
        actor.CurrentInstruction = 0xa3c3;
        actor.InstructionTimer = 8;
        state.Behavior = OwtchBehaviorState.MovingRight;
        enemies.StepFrame(CameraX, CameraY, false, samus, level: assets.LevelData);
        if (actor.XPosition != state.MaximumXPosition + 1 ||
            actor.XSubposition != 0x7000 ||
            state.Behavior != OwtchBehaviorState.MovingLeft)
        {
            throw new InvalidDataException(
                $"Owtch right bound failed: X=${actor.XPosition:X4}." +
                $"{actor.XSubposition:X4}, state={state.Behavior}.");
        }

        // The retail left routine contains DEC rather than INC. Crossing minimum zeroes no
        // coordinate: it leaves an overshoot and state $FFFF. The next indirect dispatch
        // indexes $A3D1, installs the right list, and its command finally writes state one.
        actor.XPosition = state.MinimumXPosition;
        actor.XSubposition = 0;
        actor.CurrentInstruction = 0xa3b1;
        actor.InstructionTimer = 8;
        state.Behavior = OwtchBehaviorState.MovingLeft;
        enemies.StepFrame(CameraX, CameraY, false, samus, level: assets.LevelData);
        if (actor.XPosition != state.MinimumXPosition - 1 || actor.XSubposition != 0x8000 ||
            state.Behavior != OwtchBehaviorState.LeftBoundaryUnderflow)
        {
            throw new InvalidDataException(
                $"Owtch left underflow failed: X=${actor.XPosition:X4}." +
                $"{actor.XSubposition:X4}, state=${(ushort)state.Behavior:X4}.");
        }
        enemies.StepFrame(CameraX, CameraY, false, samus, level: assets.LevelData);
        if (state.Behavior != OwtchBehaviorState.MovingRight ||
            actor.CurrentInstruction != 0xa3c3 || actor.SpritemapPointer != 0xa597)
        {
            throw new InvalidDataException(
                $"Owtch's native table-underflow recovery failed: state={state.Behavior}, " +
                $"instruction=$A2:{actor.CurrentInstruction:X4}, map=$A2:" +
                $"{actor.SpritemapPointer:X4}.");
        }

        // Force the six-in-256 gate, then cover every vertical state with the exact native
        // 16-pixel depth and parameter-selected 64-frame underground timer.
        ushort surfaceY = actor.YPosition;
        actor.XPosition = 0x0180;
        actor.XSubposition = 0;
        actor.CurrentInstruction = 0xa3bf;
        actor.InstructionTimer = 8;
        actor.FrameCounter = 0;
        state.Behavior = OwtchBehaviorState.MovingRight;
        state.SinkYOffset = 0;
        requestBurial = true;
        enemies.StepFrame(CameraX, CameraY, false, samus, level: assets.LevelData);
        requestBurial = false;
        if (state.Behavior != OwtchBehaviorState.Sinking || actor.YPosition != surfaceY)
            throw new InvalidDataException("Owtch's deterministic burial gate did not select sinking.");

        for (int frame = 0; frame < 16; frame++)
            enemies.StepFrame(CameraX, CameraY, false, samus, level: assets.LevelData);
        if (state.Behavior != OwtchBehaviorState.Underground ||
            state.SinkYOffset != 16 || actor.YPosition != surfaceY + 16 ||
            state.UndergroundTimer != 64)
        {
            throw new InvalidDataException(
                $"Owtch sinking failed: Y=${actor.YPosition:X4}, depth=" +
                $"{state.SinkYOffset}, timer={state.UndergroundTimer}, state={state.Behavior}.");
        }

        for (int frame = 0; frame < 64; frame++)
            enemies.StepFrame(CameraX, CameraY, false, samus, level: assets.LevelData);
        if (state.Behavior != OwtchBehaviorState.Rising || state.UndergroundTimer != 0)
            throw new InvalidDataException("Owtch did not leave its 64-frame underground wait.");

        // Rising samples the existing RNG high byte and must not advance it. Publish bit
        // eight here so the surfaced actor chooses right; the callback returns this exact
        // word on every read and would expose an accidental NextRandom call in the result.
        currentRandom = 0x0100;
        for (int frame = 0; frame < 16; frame++)
            enemies.StepFrame(CameraX, CameraY, false, samus, level: assets.LevelData);
        if (state.Behavior != OwtchBehaviorState.MovingRight ||
            state.SinkYOffset != 0 || actor.YPosition != surfaceY || currentRandom != 0x0100)
        {
            throw new InvalidDataException(
                $"Owtch rising failed: Y=${actor.YPosition:X4}, depth={state.SinkYOffset}, " +
                $"state={state.Behavior}, RNG=${currentRandom:X4}.");
        }

        var oam = new OamBuffer();
        oam.BeginFrame();
        enemies.DrawLayers(oam, CameraX, CameraY, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount != 1)
            throw new InvalidDataException("Owtch's one-piece ROM map did not emit one OBJ.");

        VerifyCombat(bus, room, assets);
        VerifyNormalBombCallbackGate(bus, room, assets);
        VerifyGrappleCancel(bus, room, assets);

        Console.WriteLine(
            "Owtch audit passed: untouched Pseudo Plasma Spark loaded one actor with its " +
            "retail neighbors; split 16.16 patrol, native left-table underflow, three ROM " +
            "maps, random sinking, 16-pixel burial/rise, 64-frame wait, non-advancing RNG " +
            "facing, 100-damage contact, right-state shot immunity, left-state plasma " +
            "death, state-gated normal-bomb callback, Grapple cancel, and OBJ output were " +
            "verified.");
        return 0;
    }

    private static void VerifyCombat(
        ISnesAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        RoomEnemySystem enemies = LoadIsolated(bus, room, assets, out RoomEnemySlot actor);
        OwtchEnemyState state = State(enemies, actor);
        var samus = CreateSamus(bus);
        enemies.StepFrame(CameraX, CameraY, false, samus, level: assets.LevelData);

        samus.XPosition = actor.XPosition;
        samus.YPosition = actor.YPosition;
        samus.Health = 999;
        samus.InvincibilityTimer = 0;
        samus.KnockbackActive = false;
        if (!enemies.ResolveOrdinarySamusContact(samus, 0) ||
            samus.Health != 899 || !samus.KnockbackActive)
        {
            throw new InvalidDataException("Owtch body contact did not deal header damage 100.");
        }

        var shots = new SamusProjectileSystem();
        var bombs = new SamusBombProjectileSystem();
        state.Behavior = OwtchBehaviorState.MovingRight;
        ArmPlasma(shots.Slots[0], actor);
        if (enemies.ResolveOrdinaryProjectileHits(bus, shots, bombs, samus) != 1 ||
            actor.Health != 20 || actor.Properties.HasAny(EnemyProperties.Deleted) ||
            shots.Slots[0].Type != 0x0008 || shots.Slots[0].Direction !=
                (ushort)SamusProjectileDirection.Right)
        {
            throw new InvalidDataException(
                "Owtch moving-right shot callback did not return before common damage/impact.");
        }

        state.Behavior = OwtchBehaviorState.MovingLeft;
        ArmPlasma(shots.Slots[0], actor);
        if (enemies.ResolveOrdinaryProjectileHits(bus, shots, bombs, samus) != 1 ||
            actor.Health != 0 || !actor.Properties.HasAny(EnemyProperties.Deleted) ||
            enemies.EnemiesKilled != 1)
        {
            throw new InvalidDataException("Owtch moving-left plasma/death path failed.");
        }
    }

    private static void VerifyGrappleCancel(
        ISnesAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        RoomEnemySystem enemies = LoadIsolated(bus, room, assets, out RoomEnemySlot actor);
        var samus = CreateSamus(bus);
        enemies.StepFrame(CameraX, CameraY, false, samus, level: assets.LevelData);
        GrappleEnemyCollision collision = enemies.ResolveGrappleEndpoint(
            actor.XPosition,
            actor.YPosition);
        if (!collision.Collided || collision.Reaction != GrappleEnemyReaction.Cancel)
            throw new InvalidDataException("Owtch did not select common Grapple-cancel AI $800F.");
        enemies.StepFrame(CameraX, CameraY, false, samus, level: assets.LevelData);
        if ((actor.AiHandlerBits & 4) == 0 || actor.Properties.HasAny(EnemyProperties.Deleted))
            throw new InvalidDataException("Owtch Grapple cancel did not hand off to frozen AI.");
    }

    private static void VerifyNormalBombCallbackGate(
        ISnesAddressSpace retailBus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        // Owtch's shipped vulnerability record stores zero for family $0500. Consequently,
        // an accepted bomb reaches common shot AI but deals no damage, while a rejected bomb
        // returns before common shot AI; both paths merely retain bank $A0's collision mark.
        // Assert that cartridge fact before using a one-byte diagnostic overlay to make the
        // otherwise-unobservable callback branch produce different health outcomes.
        if (retailBus.ReadByte(BombVulnerabilityAddress) != 0)
        {
            throw new InvalidDataException(
                "Retail Owtch bomb vulnerability byte at $B4:EE92 is not zero.");
        }

        var diagnosticBus = new OwtchBombVulnerabilityAddressSpace(retailBus);
        RoomEnemySystem rightEnemies = LoadIsolated(
            diagnosticBus,
            room,
            assets,
            out RoomEnemySlot rightActor);
        OwtchEnemyState rightState = State(rightEnemies, rightActor);
        SamusState rightSamus = CreateSamus(diagnosticBus);
        rightEnemies.StepFrame(
            CameraX,
            CameraY,
            timeIsFrozen: false,
            rightSamus,
            level: assets.LevelData);
        rightState.Behavior = OwtchBehaviorState.MovingRight;
        var rightBombs = new SamusBombProjectileSystem();
        var rightShots = new SamusProjectileSystem();
        SamusBombProjectileSlot rightBomb =
            EnemyProjectileAuditAssertions.ArmExplodingNormalBomb(
                rightBombs,
                rightActor.XPosition,
                rightActor.YPosition,
                damage: 20);
        if (rightEnemies.ResolveOrdinaryBombHits(rightBombs, rightShots, rightSamus) != 1 ||
            (rightBomb.Direction & 0x0010) == 0 || rightActor.Health != 20 ||
            rightActor.Properties.HasAny(EnemyProperties.Deleted) ||
            rightEnemies.EnemiesKilled != 0)
        {
            throw new InvalidDataException(
                $"Owtch moving-right normal-bomb callback did not return before common " +
                $"damage: health={rightActor.Health}, deleted=" +
                $"{rightActor.Properties.HasAny(EnemyProperties.Deleted)}, kills=" +
                $"{rightEnemies.EnemiesKilled}, direction=${rightBomb.Direction:X4}, " +
                $"state={rightState.Behavior}.");
        }

        RoomEnemySystem leftEnemies = LoadIsolated(
            diagnosticBus,
            room,
            assets,
            out RoomEnemySlot leftActor);
        OwtchEnemyState leftState = State(leftEnemies, leftActor);
        SamusState leftSamus = CreateSamus(diagnosticBus);
        leftEnemies.StepFrame(
            CameraX,
            CameraY,
            timeIsFrozen: false,
            leftSamus,
            level: assets.LevelData);
        leftState.Behavior = OwtchBehaviorState.MovingLeft;
        var leftBombs = new SamusBombProjectileSystem();
        var leftShots = new SamusProjectileSystem();
        SamusBombProjectileSlot leftBomb =
            EnemyProjectileAuditAssertions.ArmExplodingNormalBomb(
                leftBombs,
                leftActor.XPosition,
                leftActor.YPosition,
                damage: 20);
        if (leftEnemies.ResolveOrdinaryBombHits(leftBombs, leftShots, leftSamus) != 1 ||
            (leftBomb.Direction & 0x0010) == 0 || leftActor.Health != 0 ||
            !leftActor.Properties.HasAny(EnemyProperties.Deleted) ||
            leftEnemies.EnemiesKilled != 1)
        {
            throw new InvalidDataException(
                $"Owtch moving-left normal-bomb callback did not enter common damage/death: " +
                $"health={leftActor.Health}, deleted=" +
                $"{leftActor.Properties.HasAny(EnemyProperties.Deleted)}, kills=" +
                $"{leftEnemies.EnemiesKilled}, direction=${leftBomb.Direction:X4}, " +
                $"state={leftState.Behavior}.");
        }
    }

    private static RoomEnemySystem LoadIsolated(
        ISnesAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        out RoomEnemySlot actor)
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var enemies = new RoomEnemySystem();
        enemies.Load(
            bus,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            () => 0x0080,
            readRandomNumber: () => 0,
            level: assets.LevelData);
        actor = enemies.Slots
            .Take(enemies.EnemyCount)
            .Single(slot => slot.EnemyDefinitionPointer == DefinitionPointer);
        foreach (RoomEnemySlot other in enemies.Slots.Take(enemies.EnemyCount))
        {
            if (!ReferenceEquals(other, actor))
                other.Properties = other.Properties.With(EnemyProperties.Deleted);
        }
        return enemies;
    }

    private static void VerifyHeader(ISnesAddressSpace bus)
    {
        RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(bus, DefinitionPointer);
        if (definition.TileDataSize != 0x0400 || definition.PalettePointer != 0xa38b ||
            definition.Health != 20 || definition.Damage != 100 ||
            definition.XRadius != 8 || definition.YRadius != 8 || definition.Bank != 0xa2 ||
            definition.HurtSoundEffect != 0x003e ||
            definition.InitializationAiPointer != 0xa3f9 || definition.PartCount != 1 ||
            definition.MainAiPointer != 0xa47e || definition.GrappleAiPointer != 0x800f ||
            definition.HurtAiPointer != 0x804c || definition.FrozenAiPointer != 0x8041 ||
            definition.DeathAnimation != 0 || definition.PowerBombReactionPointer != 0 ||
            definition.TouchAiPointer != 0x8023 || definition.ShotAiPointer != 0xa579 ||
            definition.InitialSpritemapPointer != 0 ||
            definition.TileDataAddress != 0xacea00 || definition.Layer != 5 ||
            definition.ItemDropChancesPointer != 0xf34a ||
            definition.VulnerabilityPointer != 0xee84 || definition.NamePointer != 0xe16b)
        {
            throw new InvalidDataException("Retail Owtch header words do not match $A0:D03F.");
        }
    }

    private static OwtchEnemyState State(RoomEnemySystem enemies, RoomEnemySlot actor) =>
        enemies.OwtchStates[actor.SlotIndex] ?? throw new InvalidDataException(
            $"Owtch slot {actor.SlotIndex} has no typed state.");

    private static SamusState CreateSamus(ISnesAddressSpace bus)
    {
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusState.FacingRightNormalPose,
            XPosition = 0,
            YPosition = 0,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        return samus;
    }

    private static void ArmPlasma(SamusProjectileSlot projectile, RoomEnemySlot target)
    {
        projectile.ClearFields();
        // Owtch's vulnerability record accepts only the four plasma combinations. Type
        // eight is uncharged plasma with multiplier two; base damage 20 therefore deals
        // the exact 20 HP required after the common half-damage convention.
        projectile.Type = 0x0008;
        projectile.Damage = 20;
        projectile.Direction = (ushort)SamusProjectileDirection.Right;
        projectile.XPosition = target.XPosition;
        projectile.YPosition = target.YPosition;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    /// <summary>
    /// Read-only diagnostic overlay for Owtch's family-$0500 vulnerability byte. Production
    /// code never sees this wrapper; the focused audit uses multiplier two solely to expose
    /// whether private callback $A2:A579 did or did not jump to common shot AI.
    /// </summary>
    private sealed class OwtchBombVulnerabilityAddressSpace : ISnesAddressSpace
    {
        private readonly ISnesAddressSpace _inner;

        public OwtchBombVulnerabilityAddressSpace(ISnesAddressSpace inner) =>
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));

        public byte ReadByte(int address) =>
            address == BombVulnerabilityAddress ? (byte)2 : _inner.ReadByte(address);

        public void WriteByte(int address, byte value) => _inner.WriteByte(address, value);
    }
}
