using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>
/// ROM-backed end-to-end audit for the four untouched Boyons in Alpha Power Bomb Room.
/// This room contains no second enemy family, so loading, animation, movement, and combat
/// cannot accidentally pass through an unrelated translated actor.
/// </summary>
internal static class BoyonAudit
{
    private const ushort RoomPointer = 0xa3ae;
    private const ushort DefinitionPointer = 0xcebf;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, RoomPointer);
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
            .Where(slot => slot.EnemyDefinitionPointer == DefinitionPointer)
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
        if (!enemies.ResolveOrdinarySamusContact(samus, 0) ||
            samus.Health != 989 || !samus.KnockbackActive)
        {
            throw new InvalidDataException(
                $"Boyon contact attack failed: health={samus.Health}, " +
                $"knockback={samus.KnockbackActive}.");
        }

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
            !audited.Properties.HasAny(EnemyProperties.Deleted) || enemies.EnemiesKilled != 1)
        {
            throw new InvalidDataException(
                $"Boyon death failed: health={audited.Health}, " +
                $"properties=${audited.Properties:X4}, killed={enemies.EnemiesKilled}.");
        }

        Console.WriteLine(
            "Boyon audit passed: four untouched Alpha Power Bomb actors loaded; the exact " +
            $"curve produced a {arcFrames}-frame arc over Y ${minimumY:X4}-${0x00a8:X4}; " +
            "idle/bounce bytecode covered all seven ROM maps and off-screen flags; body " +
            "contact, beam immunity, Ice freeze, Super Missile/power-bomb damage, Grapple " +
            $"cancellation, death, and {oam.LastFinalizedSpriteCount} OBJ pieces passed.");
        return 0;
    }

    private static void VerifyHeader(ISnesAddressSpace bus)
    {
        RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(bus, DefinitionPointer);
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
