using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>Retail cartridge identities used by the Alpha Power Bomb Boyon audit.</summary>
internal static class BoyonAuditDefinitions
{
    /// <summary>Crateria Super room <c>$00/$1D</c> header at <c>$8F:99F9</c>.</summary>
    public const ushort CrateriaSuperRoomHeader = 0x99f9;

    /// <summary>Alpha Power Bomb room <c>$01/$26</c> header at <c>$8F:A3AE</c>.</summary>
    public const ushort AlphaPowerBombRoomHeader = 0xa3ae;

    /// <summary>Boyon enemy definition at <c>$A0:CEBF</c>.</summary>
    public const ushort BoyonEnemyDefinition = 0xcebf;
}

/// <summary>
/// ROM-backed end-to-end audit for the four untouched Boyons in Alpha Power Bomb Room.
/// This room contains no second enemy family, so loading, animation, movement, and combat
/// cannot accidentally pass through an unrelated translated actor.
/// </summary>
internal static class BoyonAudit
{
    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(
            bus,
            BoyonAuditDefinitions.AlphaPowerBombRoomHeader);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);

        VerifyHeader(bus);

        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusPoseIds.FacingRightNormalPose,
            XPosition = 0,
            YPosition = 0x00a8,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);

        var random = new Bank80SystemState();
        var enemies = new RoomEnemySystem();
        enemies.Load(
            bus,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            random.NextRandom,
            random.SetRandomNumber,
            level: assets.LevelData,
            samus: samus);

        RoomEnemySlot[] boyons = enemies.Slots
            .Take(enemies.EnemyCount)
            .Where(slot => slot.EnemyDefinitionPointer == BoyonAuditDefinitions.BoyonEnemyDefinition)
            .ToArray();
        if (room.State.Pointer != 0xa3bb || enemies.EnemyCount != 4 || boyons.Length != 4 ||
            boyons.Any(slot => enemies.BoyonStates[slot.SlotIndex] is null))
        {
            throw new InvalidDataException(
                $"Alpha Power Bomb state=${room.State.Pointer:X4}, count={enemies.EnemyCount}, " +
                $"Boyons={boyons.Length}.");
        }

        ushort[] expectedX = [0x0258, 0x0268, 0x01f8, 0x0208];
        for (int index = 0; index < boyons.Length; index++)
        {
            RoomEnemySlot actor = boyons[index];
            BoyonEnemyState state = State(enemies, actor);
            if (actor.XPosition != expectedX[index] || actor.YPosition != 0x00a8 ||
                actor.Parameter1 != 0x0003 || actor.Parameter2 != 0x0020 ||
                actor.CurrentInstruction != 0x86a7 || actor.SpritemapPointer != 0x804d ||
                actor.Health != 1000 || actor.Properties != 0x2000 ||
                state.SpeedMultiplier != 8 || state.JumpHeight != 0x3000 ||
                state.BounceMovement != BoyonBounceMovement.Rising || state.Bouncing ||
                state.BounceSpeedCalculated)
            {
                throw new InvalidDataException(
                    $"Boyon {index} initialization mismatch at " +
                    $"(${actor.XPosition:X4},${actor.YPosition:X4}), params=" +
                    $"${actor.Parameter1:X4}/${actor.Parameter2:X4}, list=" +
                    $"${actor.CurrentInstruction:X4}, multiplier/height=" +
                    $"${state.SpeedMultiplier:X4}/${state.JumpHeight:X4}.");
            }
        }

        RoomEnemySlot audited = boyons[0];
        BoyonEnemyState auditedState = State(enemies, audited);
        ushort cameraX = unchecked((ushort)(audited.XPosition - 0x0080));
        const ushort cameraY = 0;

        // The first main-AI frame performs only the native curve integration. Instruction
        // processing then installs the first idle map and clears off-screen processing.
        enemies.StepFrame(cameraX, cameraY, false, samus, level: assets.LevelData);
        if (!auditedState.BounceSpeedCalculated ||
            auditedState.InitialBounceSpeedTableIndex != 21 ||
            auditedState.SpeedTableIndex != 21 ||
            auditedState.DistanceAccumulator != 0x3020 ||
            !auditedState.BounceDisabled || !auditedState.IdleDisabled ||
            audited.SpritemapPointer != 0x88da ||
            audited.Properties.HasAny(EnemyProperties.ProcessOffScreen))
        {
            throw new InvalidDataException(
                $"Boyon curve setup mismatch: index={auditedState.SpeedTableIndex}/" +
                $"{auditedState.InitialBounceSpeedTableIndex}, distance=" +
                $"${auditedState.DistanceAccumulator:X4}, map=${audited.SpritemapPointer:X4}, " +
                $"flags={auditedState.BounceDisabled}/{auditedState.IdleDisabled}.");
        }

        var idleMaps = new HashSet<ushort> { audited.SpritemapPointer };
        for (int frame = 0; frame < 50; frame++)
        {
            enemies.StepFrame(cameraX, cameraY, false, samus, level: assets.LevelData);
            idleMaps.Add(audited.SpritemapPointer);
        }
        ushort[] expectedIdleMaps = [0x88da, 0x88e1, 0x88e8];
        if (!expectedIdleMaps.All(idleMaps.Contains) || audited.YPosition != 0x00a8)
        {
            throw new InvalidDataException(
                $"Boyon idle animation/motion mismatch: Y=${audited.YPosition:X4}, maps=" +
                string.Join(',', idleMaps.Order()));
        }

        // Crossing the strict horizontal threshold starts one complete arc. Move Samus away
        // immediately afterward to prove the cartridge finishes an in-flight bounce before
        // returning to idle rather than snapping to the spawn baseline.
        samus.XPosition = audited.XPosition;
        samus.YPosition = audited.YPosition;
        var bounceMaps = new HashSet<ushort>();
        enemies.StepFrame(cameraX, cameraY, false, samus, level: assets.LevelData);
        bounceMaps.Add(audited.SpritemapPointer);
        if (audited.YPosition != 0x00a1 ||
            enemies.LastBoyonSoundEffect != 0x000e ||
            !audited.Properties.HasAny(EnemyProperties.ProcessOffScreen) ||
            !auditedState.Bouncing || auditedState.BounceDisabled)
        {
            throw new InvalidDataException(
                $"Boyon bounce start mismatch: Y=${audited.YPosition:X4}, " +
                $"sound=${enemies.LastBoyonSoundEffect:X4}, properties=" +
                $"${audited.Properties:X4}, flags=" +
                $"{auditedState.Bouncing}/{auditedState.BounceDisabled}.");
        }

        samus.XPosition = 0;
        ushort minimumY = audited.YPosition;
        int arcFrames = 1;
        while (!(auditedState.BounceDisabled && !auditedState.Bouncing &&
                 auditedState.BounceMovement == BoyonBounceMovement.Rising))
        {
            if (++arcFrames > 96)
                throw new InvalidDataException("Boyon did not finish one ROM bounce within 96 frames.");
            enemies.StepFrame(cameraX, cameraY, false, samus, level: assets.LevelData);
            bounceMaps.Add(audited.SpritemapPointer);
            minimumY = Math.Min(minimumY, audited.YPosition);
        }

        ushort[] expectedBounceMaps = [0x88ef, 0x88f6, 0x88fd, 0x8904];
        if (!expectedBounceMaps.All(bounceMaps.Contains) || audited.YPosition != 0x00a8 ||
            minimumY >= 0x0080)
        {
            throw new InvalidDataException(
                $"Boyon arc mismatch after {arcFrames} frames: " +
                $"Y=${minimumY:X4}-${audited.YPosition:X4}, maps=" +
                string.Join(',', bounceMaps.Order()));
        }

        enemies.StepFrame(cameraX, cameraY, false, samus, level: assets.LevelData);
        if (audited.CurrentInstruction is < 0x86ab or > 0x86bd ||
            audited.Properties.HasAny(EnemyProperties.ProcessOffScreen))
        {
            throw new InvalidDataException(
                $"Boyon did not return to idle: list=${audited.CurrentInstruction:X4}, " +
                $"properties=${audited.Properties:X4}.");
        }

        var oam = new OamBuffer();
        oam.BeginFrame();
        enemies.DrawLayers(oam, cameraX, cameraY, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException("Alpha Power Bomb Boyons emitted no ROM-backed OBJ pieces.");

        // Common body contact is Boyon's only attack and must use the ten-point header
        // damage, including Samus's shared knockback/invincibility setup.
        samus.XPosition = audited.XPosition;
        samus.YPosition = audited.YPosition;
        samus.Health = 999;
        samus.InvincibilityTimer = 0;
        var beforeContact = EnemyContactAuditAssertions.Capture(samus);
        if (!enemies.ResolveOrdinarySamusContact(samus, 0) ||
            samus.Health != 989)
        {
            throw new InvalidDataException(
                $"Boyon contact attack failed: health={samus.Health}, " +
                $"knockback={samus.KnockbackActive}.");
        }
        EnemyContactAuditAssertions.VerifyStandingAirHit(bus, samus, beforeContact, 10, 1, "Boyon body contact");

        // Power Beam's vulnerability byte is zero: collision consumes the shot but causes
        // neither damage nor hurt flash. Super Missiles use multiplier two, which the common
        // handler combines with its half-damage convention to preserve the nominal 300.
        var sharedProjectiles = new SamusBombProjectileSystem();
        var immuneBeam = new SamusProjectileSystem();
        ArmProjectile(immuneBeam.Slots[0], audited, type: 0x0000, damage: 20);
        if (enemies.ResolveOrdinaryProjectileHits(
                bus, immuneBeam, sharedProjectiles, samus) != 1 || audited.Health != 1000)
        {
            throw new InvalidDataException("Boyon did not reject its immune Power Beam hit.");
        }

        var superMissile = new SamusProjectileSystem();
        ArmProjectile(superMissile.Slots[0], audited, type: 0x0200, damage: 300);
        if (enemies.ResolveOrdinaryProjectileHits(
                bus, superMissile, sharedProjectiles, samus) != 1 || audited.Health != 700 ||
            audited.FlashTimer == 0)
        {
            throw new InvalidDataException(
                $"Boyon Super Missile damage failed: health={audited.Health}, " +
                $"flash={audited.FlashTimer}.");
        }

        // Ice Beam is the $FF freeze sentinel. It preserves health while installing the
        // common 400-frame frozen handler on a separate untouched room actor.
        RoomEnemySlot frozen = boyons[1];
        var iceBeam = new SamusProjectileSystem();
        ArmProjectile(iceBeam.Slots[0], frozen, type: 0x0002, damage: 20);
        if (enemies.ResolveOrdinaryProjectileHits(
                bus, iceBeam, sharedProjectiles, samus) != 1 || frozen.Health != 1000 ||
            frozen.FrozenTimer != 400 || (frozen.AiHandlerBits & 0x0004) == 0)
        {
            throw new InvalidDataException(
                $"Boyon Ice Beam freeze failed: health={frozen.Health}, " +
                $"timer={frozen.FrozenTimer}, AI=${frozen.AiHandlerBits:X4}.");
        }

        // The power-bomb byte is multiplier two, yielding 200 damage. A twelve-pixel test
        // radius isolates the third actor from its neighbor sixteen pixels away.
        RoomEnemySlot bombed = boyons[2];
        if (enemies.ResolveOrdinaryPowerBombHits(
                bus, bombed.XPosition, bombed.YPosition, explosionRadius: 12) != 1 ||
            bombed.Health != 800 || bombed.InvincibilityTimer != 48 ||
            !bombed.Properties.HasAny(EnemyProperties.ProcessOffScreen))
        {
            throw new InvalidDataException(
                $"Boyon power-bomb reaction failed: health={bombed.Health}, " +
                $"invincibility={bombed.InvincibilityTimer}, " +
                $"properties=${bombed.Properties:X4}.");
        }

        RoomEnemySlot grappled = boyons[3];
        GrappleEnemyCollision grapple = enemies.ResolveGrappleEndpoint(
            grappled.XPosition,
            grappled.YPosition);
        if (!grapple.Collided || grapple.Reaction != GrappleEnemyReaction.Cancel ||
            grapple.EnemyNativeIndex != grappled.NativeIndex)
        {
            throw new InvalidDataException("Boyon did not cancel the Grapple Beam.");
        }

        // Finish with an independently armed lethal Super Missile to prove the normal death
        // path removes the actor and increments the room counter exactly once.
        var lethal = new SamusProjectileSystem();
        ArmProjectile(lethal.Slots[0], audited, type: 0x0200, damage: 1000);
        if (enemies.ResolveOrdinaryProjectileHits(
                bus, lethal, sharedProjectiles, samus) != 1 || audited.Health != 0 ||
            audited.EnemyDefinitionPointer != 0 || audited.Properties != 0 ||
            enemies.EnemiesKilled != 1)
        {
            throw new InvalidDataException(
                $"Boyon death failed: health={audited.Health}, " +
                $"definition=${audited.EnemyDefinitionPointer:X4}, " +
                $"properties=${audited.Properties:X4}, killed={enemies.EnemiesKilled}.");
        }

        VerifyIntegratedRoomActivation(bus);
        VerifyCrateriaSuperFreezePuzzle(bus);

        Console.WriteLine(
            "Boyon audit passed: four untouched Alpha Power Bomb actors loaded; the exact " +
            $"curve produced a {arcFrames}-frame arc over Y ${minimumY:X4}-${0x00a8:X4}; " +
            "idle/bounce bytecode covered all seven ROM maps and off-screen flags; body " +
            "contact, beam immunity, exact $00/$1D Ice projectile/freeze timing, " +
            "Super Missile/power-bomb damage, Grapple " +
            $"cancellation, death, and {oam.LastFinalizedSpriteCount} OBJ pieces passed.");
        return 0;
    }

    /// <summary>
    /// Resolves issue #294 against the reported retail room rather than inferring its
    /// behavior from the mechanically similar Alpha Power Bomb population used above.
    /// </summary>
    private static void VerifyCrateriaSuperFreezePuzzle(SuperMetroidAddressSpace bus)
    {
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(
            bus,
            BoyonAuditDefinitions.CrateriaSuperRoomHeader);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);

        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusPoseIds.FacingRightNormalPose,
            EquippedBeams = (ushort)SamusBeamFlags.Ice,
            CollectedBeams = (ushort)SamusBeamFlags.Ice,
            XPosition = 0,
            YPosition = 0x07a8,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);

        var random = new Bank80SystemState();
        var enemies = new RoomEnemySystem();
        enemies.Load(
            bus,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            random.NextRandom,
            random.SetRandomNumber,
            level: assets.LevelData,
            samus: samus);
        RoomEnemySlot[] boyons = enemies.Slots
            .Take(enemies.EnemyCount)
            .Where(slot => slot.EnemyDefinitionPointer == BoyonAuditDefinitions.BoyonEnemyDefinition)
            .ToArray();
        if (room.AreaIndex != 0 || room.RoomIndex != 0x1d ||
            room.State.Pointer != 0x9a06 || boyons.Length != 4 ||
            boyons.Any(slot => slot.Parameter1 != 0x0103 || slot.Parameter2 != 0x0020))
        {
            throw new InvalidDataException(
                $"Crateria Super fixture diverged: identity=${room.AreaIndex:X2}/" +
                $"${room.RoomIndex:X2}, state=${room.State.Pointer:X4}, Boyons=" +
                $"{boyons.Length}, params=" +
                string.Join(',', boyons.Select(slot =>
                    $"${slot.Parameter1:X4}/${slot.Parameter2:X4}")));
        }

        RoomEnemySlot target = boyons[0];
        const ushort cameraX = 0x0200;
        const ushort cameraY = 0x0700;

        // The first actor frame performs Boyon's one-time curve integration. At exactly
        // 32 pixels the cartridge's CMP/BPL rejects proximity; 31 pixels starts the bounce.
        enemies.StepFrame(cameraX, cameraY, false, samus, level: assets.LevelData);
        BoyonEnemyState state = State(enemies, target);
        samus.XPosition = unchecked((ushort)(target.XPosition + 32));
        enemies.StepFrame(cameraX, cameraY, false, samus, level: assets.LevelData);
        if (state.Bouncing || !state.BounceDisabled)
            throw new InvalidDataException("Crateria Super Boyon activated at the excluded 32-pixel boundary.");
        samus.XPosition = unchecked((ushort)(target.XPosition + 31));
        enemies.StepFrame(cameraX, cameraY, false, samus, level: assets.LevelData);
        if (!state.Bouncing || state.BounceDisabled)
            throw new InvalidDataException("Crateria Super Boyon did not activate inside its 32-pixel threshold.");

        // Sample the complete player projectile producer in empty terrain first. This
        // captures the standing-pose muzzle offset plus the same-frame four-pixel movement,
        // then lets the second produced shot end exactly on the real room actor without
        // replacing its ROM-derived speed, radius, type, or instruction list by hand.
        const int emptyBlocksWide = 64;
        const int emptyBlocksHigh = 128;
        var empty = new RoomLevelData(
            emptyBlocksWide,
            emptyBlocksHigh,
            new ushort[emptyBlocksWide * emptyBlocksHigh],
            new byte[emptyBlocksWide * emptyBlocksHigh],
            new ushort[emptyBlocksWide * emptyBlocksHigh],
            []);
        var sampleSamus = new SamusState
        {
            Pose = SamusPoseIds.FacingRightNormalPose,
            EquippedBeams = (ushort)SamusBeamFlags.Ice,
            XPosition = 0x0100,
            YPosition = 0x0100,
        };
        sampleSamus.RefreshCollisionRadii(bus);
        var sampleShots = new SamusProjectileSystem();
        var sampleBombs = new SamusBombProjectileSystem();
        SamusProjectileFrameResult sampleResult = sampleShots.StepFrame(
            bus,
            empty,
            sampleSamus,
            (ushort)SuperMetroid.Core.Input.SnesButton.X,
            (ushort)SuperMetroid.Core.Input.SnesButton.X,
            0,
            0,
            sampleBombs);
        SamusProjectileSlot sample = sampleShots.Slots[
            sampleResult.FiredSlot ?? throw new InvalidDataException("Ice shot sampler did not allocate a projectile.")];
        SamusProjectileSpawnSnapshot initialShot = sampleShots.LastFiredProjectileSnapshot ??
            throw new InvalidDataException("Ice shot sampler did not publish its initial trajectory.");
        short shotXOffset = unchecked((short)(sample.XPosition - sampleSamus.XPosition));
        short shotYOffset = unchecked((short)(sample.YPosition - sampleSamus.YPosition));
        if (sample.PackedType.BeamCombinationIndex != 2 || sample.Damage != 30 ||
            initialShot.XVelocity != 0x0400 || initialShot.YVelocity != 0 ||
            sample.XVelocity != 0x0410 || sample.YVelocity != 0 ||
            sample.XRadius != 8 || sample.YRadius != 8)
        {
            throw new InvalidDataException(
                $"Produced Ice shot diverged: type=${sample.Type:X4}, damage={sample.Damage}, " +
                $"velocity=${initialShot.XVelocity:X4}->${sample.XVelocity:X4}/" +
                $"${initialShot.YVelocity:X4}->${sample.YVelocity:X4}, " +
                $"radii={sample.XRadius}/{sample.YRadius}.");
        }

        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.XPosition = unchecked((ushort)(target.XPosition - shotXOffset));
        samus.YPosition = unchecked((ushort)(target.YPosition - shotYOffset));
        samus.RefreshCollisionRadii(bus);
        var shots = new SamusProjectileSystem();
        var bombs = new SamusBombProjectileSystem();
        SamusProjectileFrameResult fired = shots.StepFrame(
            bus,
            empty,
            samus,
            (ushort)SuperMetroid.Core.Input.SnesButton.X,
            (ushort)SuperMetroid.Core.Input.SnesButton.X,
            cameraX,
            cameraY,
            bombs);
        SamusProjectileSlot ice = shots.Slots[
            fired.FiredSlot ?? throw new InvalidDataException("Centered Ice shot did not allocate a projectile.")];
        if (ice.XPosition != target.XPosition || ice.YPosition != target.YPosition ||
            enemies.ResolveOrdinaryProjectileHits(bus, shots, bombs, samus) != 1 ||
            target.FrozenTimer != 400)
        {
            throw new InvalidDataException(
                $"Crateria Super centered Ice collision diverged: shot=" +
                $"({ice.XPosition:X4},{ice.YPosition:X4}), target=" +
                $"({target.XPosition:X4},{target.YPosition:X4}), freeze={target.FrozenTimer}.");
        }

        // The native handler tests zero before decrementing. The 400th call reaches
        // zero but retains the frozen handler; the following call performs thawing.
        ushort frozenX = target.XPosition, frozenY = target.YPosition;
        ushort frozenSubX = target.XSubposition, frozenSubY = target.YSubposition;
        for (int frame = 1; frame <= 400; frame++)
        {
            enemies.StepFrame(cameraX, cameraY, false, samus, level: assets.LevelData);
            if (target.FrozenTimer != 400 - frame || (target.AiHandlerBits & 0x0004) == 0 ||
                target.XPosition != frozenX || target.YPosition != frozenY ||
                target.XSubposition != frozenSubX || target.YSubposition != frozenSubY)
                throw new InvalidDataException($"Crateria Super frozen countdown/position diverged on frame {frame}: timer={target.FrozenTimer}.");
        }
        enemies.StepFrame(cameraX, cameraY, false, samus, level: assets.LevelData);
        if (target.FrozenTimer != 0 || (target.AiHandlerBits & 0x0004) != 0)
            throw new InvalidDataException($"Crateria Super freeze did not thaw on frame 401: {target.FrozenTimer}.");
    }

    private static void VerifyHeader(ISnesAddressSpace bus)
    {
        RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(
            bus,
            BoyonAuditDefinitions.BoyonEnemyDefinition);
        if (definition.TileDataSize != 0x0400 || definition.PalettePointer != 0x8687 ||
            definition.Health != 1000 || definition.Damage != 10 ||
            definition.XRadius != 8 || definition.YRadius != 8 || definition.Bank != 0xa2 ||
            definition.InitializationAiPointer != 0x871c || definition.PartCount != 1 ||
            definition.MainAiPointer != 0x879c || definition.GrappleAiPointer != 0x800f ||
            definition.HurtAiPointer != 0x804c || definition.FrozenAiPointer != 0x8041 ||
            definition.DeathAnimation != 0 || definition.PowerBombReactionPointer != 0 ||
            definition.TouchAiPointer != 0x8023 || definition.ShotAiPointer != 0x802d ||
            definition.InitialSpritemapPointer != 0 ||
            definition.TileDataAddress != 0xacb600 || definition.Layer != 5 ||
            definition.ItemDropChancesPointer != 0xf320 ||
            definition.VulnerabilityPointer != 0xeda8)
        {
            throw new InvalidDataException("Retail Boyon header words do not match $A0:CEBF.");
        }
    }

    /// <summary>
    /// Reproduces issue #287 through the production room/runtime seam. Direct enemy tests
    /// cannot catch a missing Samus handoff, skipped room population, or absent frame draw.
    /// </summary>
    private static void VerifyIntegratedRoomActivation(SuperMetroidAddressSpace bus)
    {
        var runtime = new SuperMetroid.Core.Runtime.SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(
            BoyonAuditDefinitions.AlphaPowerBombRoomHeader,
            cameraX: 0x0180,
            cameraY: 0);
        SamusState integratedSamus = runtime.Samus
            ?? throw new InvalidDataException("Integrated Boyon room did not retain Samus.");
        RoomEnemySlot[] integratedBoyons = runtime.Enemies.Slots
            .Where(slot => slot.EnemyDefinitionPointer == BoyonAuditDefinitions.BoyonEnemyDefinition)
            .ToArray();
        if (integratedBoyons.Length != 4)
            throw new InvalidDataException($"Integrated Boyon room loaded {integratedBoyons.Length}/4 actors.");

        RoomEnemySlot target = integratedBoyons[0];
        ushort baselineY = target.YPosition;
        integratedSamus.XPosition = target.XPosition;
        integratedSamus.YPosition = target.YPosition;
        runtime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);
        bool moved = false;
        bool sounded = false;
        bool rendered = false;
        for (int frame = 0; frame < 12; frame++)
        {
            runtime.StepFrame(controller1Input: 0);
            moved |= target.YPosition < baselineY;
            sounded |= runtime.Enemies.LastBoyonSoundEffect == 0x000e;
            rendered |= runtime.DisplayedOam.LastFinalizedSpriteCount > 0;
        }
        BoyonEnemyState state = runtime.Enemies.BoyonStates[target.SlotIndex]
            ?? throw new InvalidDataException("Integrated Boyon target lost its typed state.");
        if (!moved || !sounded || !rendered || !state.Bouncing || state.BounceDisabled)
        {
            throw new InvalidDataException(
                $"Integrated Boyon did not visibly activate: moved={moved}, sounded={sounded}, " +
                $"rendered={rendered}, bouncing={state.Bouncing}, disabled={state.BounceDisabled}, " +
                $"Y=${baselineY:X4}->${target.YPosition:X4}.");
        }
    }

    private static BoyonEnemyState State(RoomEnemySystem enemies, RoomEnemySlot actor) =>
        enemies.BoyonStates[actor.SlotIndex] ?? throw new InvalidDataException(
            $"Boyon slot {actor.SlotIndex} has no typed state.");

    private static void ArmProjectile(
        SamusProjectileSlot projectile,
        RoomEnemySlot target,
        ushort type,
        ushort damage)
    {
        projectile.ClearFields();
        projectile.Type = type;
        projectile.Damage = damage;
        projectile.Direction = (ushort)SamusProjectileDirection.Right;
        projectile.XPosition = target.XPosition;
        projectile.YPosition = target.YPosition;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }
}
