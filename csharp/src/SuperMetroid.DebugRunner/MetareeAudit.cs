using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Retail Dachora-room regression for Metaree load, bytecode, flight, burrowing, debris,
/// rendering, contact damage, vulnerability damage, and special shot death behavior.
/// </summary>
internal static class MetareeAudit
{
    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace retailBus =
            SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(retailBus, 0x9cb3);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(retailBus, room);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);

        // Dachora's real population begins with two translated Zeelas and then all three
        // retail Metarees. Later records are Rio/Dachora families that remain separate
        // translation tasks. This one-word audit overlay terminates after record five; every
        // retained record, header, instruction, collision block, palette, and tile still
        // comes from its original cartridge address and executes the production loader.
        const int retainedPopulationRecords = 5;
        var auditBus = new PopulationTerminatingAddressSpace(
            retailBus,
            room.State.EnemyPopulationPointer,
            retainedPopulationRecords,
            deathQuota: 3);

        var enemies = new RoomEnemySystem();
        var random = new Bank80SystemState();
        enemies.Load(
            auditBus,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            random.NextRandom,
            random.SetRandomNumber);

        const ushort metareeDefinition = 0xd67f;
        RoomEnemySlot[] metarees = enemies.Slots
            .Take(enemies.EnemyCount)
            .Where(slot => slot.EnemyDefinitionPointer == metareeDefinition)
            .ToArray();
        if (room.State.Pointer != 0x9cc0 || enemies.EnemyCount != 5 ||
            enemies.Slots.Take(2).Any(slot => slot.EnemyDefinitionPointer != 0xdc7f) ||
            metarees.Length != 3 ||
            metarees.Any(slot => enemies.MetareeStates[slot.SlotIndex] is null))
        {
            throw new InvalidDataException(
                $"Dachora selected state ${room.State.Pointer:X4} with " +
                $"{enemies.EnemyCount} retained enemies and {metarees.Length} Metarees.");
        }

        RoomEnemySlot diving = metarees[0];
        MetareeEnemyState divingState = enemies.MetareeStates[diving.SlotIndex]
            ?? throw new InvalidDataException("First Dachora Metaree has no typed state.");
        if (diving.SlotIndex != 2 || diving.XPosition != 0x062d ||
            diving.YPosition != 0x006c || diving.Health != 50 ||
            diving.Definition.Damage != 50 || diving.XRadius != 8 ||
            diving.YRadius != 12 || diving.CurrentInstruction != 0x8910 ||
            divingState.Function != MetareeEnemyFunction.Idling ||
            divingState.RequestedInstructionListIndex != 0 ||
            divingState.InstalledInstructionListIndex != 0)
        {
            throw new InvalidDataException(
                $"First Dachora Metaree init failed: slot={diving.SlotIndex}, " +
                $"position=({diving.XPosition:X4},{diving.YPosition:X4}), " +
                $"health/damage={diving.Health}/{diving.Definition.Damage}, " +
                $"radius={diving.XRadius}/{diving.YRadius}, " +
                $"list=${diving.CurrentInstruction:X4}, " +
                $"function=$A3:{(ushort)divingState.Function:X4}.");
        }

        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusState.FacingRightNormalPose,
            XPosition = unchecked((ushort)(diving.XPosition - 0x0100)),
            YPosition = unchecked((ushort)(diving.YPosition + 0x0060)),
        };
        samus.RefreshCollisionRadii(auditBus);
        samus.InitializeAnimation(auditBus);

        ushort cameraX = unchecked((ushort)(diving.XPosition - 0x0080));
        const ushort cameraY = 0;
        var animationMaps = new HashSet<ushort>();

        // Forty idle frames traverse the complete four-map $A3:8910 loop while Samus stays
        // outside the native 72-pixel horizontal trigger. This proves real bytecode before
        // changing the actor's state for the attack half of the audit.
        for (int frame = 0; frame < 40; frame++)
        {
            enemies.StepFrame(cameraX, cameraY, false, samus, level: assets.LevelData);
            animationMaps.Add(diving.SpritemapPointer);
        }
        if (divingState.Function != MetareeEnemyFunction.Idling ||
            animationMaps.Count != 4)
        {
            throw new InvalidDataException(
                $"Metaree idle bytecode produced {animationMaps.Count} maps and " +
                $"function $A3:{(ushort)divingState.Function:X4}.");
        }

        var oam = new OamBuffer();
        oam.BeginFrame();
        enemies.DrawLayers(oam, cameraX, cameraY, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException("Dachora Metaree emitted no live ROM OBJ.");
        ushort packedSpawnGraphics = unchecked((ushort)(
            diving.VramTilesIndex | diving.PaletteIndex));

        // Use the other two still-idling retail actors for combat before putting Samus near
        // the first one. Otherwise their native 72-pixel trigger can legitimately launch
        // them in parallel and consume the intended fixtures during the longer dive audit.
        // No extra scheduler step is needed: the idle loop above already built the current
        // interactive list and installed a live spritemap for all three actors.
        RoomEnemySlot contactTarget = metarees[1];
        samus.XPosition = contactTarget.XPosition;
        samus.YPosition = contactTarget.YPosition;
        samus.Health = 999;
        samus.InvincibilityTimer = 0;
        if (!enemies.ResolveOrdinarySamusContact(samus, 0) || samus.Health != 949)
        {
            throw new InvalidDataException(
                $"Metaree contact left Samus at {samus.Health}, expected 949.");
        }

        // Metaree's vulnerability table ignores ordinary power beam but gives the super-
        // missile family multiplier two. A 20-damage projectile therefore removes exactly
        // 20 from the third actor's 50 health through the production special-shot dispatch.
        RoomEnemySlot shotTarget = metarees[2];
        var projectiles = new SamusProjectileSystem();
        var sharedProjectiles = new SamusBombProjectileSystem();
        ArmProjectile(projectiles.Slots[0], shotTarget, type: 0x0200, damage: 20);
        int nonlethalHitCount = enemies.ResolveOrdinaryProjectileHits(
            auditBus,
            projectiles,
            sharedProjectiles,
            samus);
        if (nonlethalHitCount != 1 || shotTarget.Health != 30)
        {
            throw new InvalidDataException(
                $"Metaree vulnerability hit count={nonlethalHitCount}; third actor health " +
                $"is {shotTarget.Health}, expected 30.");
        }

        int debrisBeforeDeath = enemies.ActiveEnemyProjectileCount;
        var lethalProjectiles = new SamusProjectileSystem();
        ArmProjectile(lethalProjectiles.Slots[0], shotTarget, type: 0x0200, damage: 1000);
        if (enemies.ResolveOrdinaryProjectileHits(
                auditBus,
                lethalProjectiles,
                sharedProjectiles,
                samus) != 1 || shotTarget.Health != 0 ||
            !shotTarget.Properties.HasAny(EnemyProperties.Deleted) ||
            enemies.EnemiesKilled != 1 || shotTarget.VramTilesIndex != 0 ||
            shotTarget.PaletteIndex != 0 ||
            enemies.ActiveEnemyProjectileCount != debrisBeforeDeath + 4)
        {
            throw new InvalidDataException(
                $"Metaree special shot death failed: health={shotTarget.Health}, " +
                $"properties=${shotTarget.Properties:X4}, killed={enemies.EnemiesKilled}, " +
                $"graphics=${shotTarget.PaletteIndex:X4}/${shotTarget.VramTilesIndex:X4}, " +
                $"debris={debrisBeforeDeath}->{enemies.ActiveEnemyProjectileCount}.");
        }

        RoomEnemyProjectileSlot[] shotDeathDebris = enemies.EnemyProjectiles
            .Where(projectile => projectile.IsActive)
            .ToArray();
        if (shotDeathDebris.Length != 4 ||
            shotDeathDebris.Select(projectile => projectile.Kind).Distinct().Count() != 4 ||
            shotDeathDebris.Any(projectile =>
                projectile.Kind is < RoomEnemyProjectileKind.MetareeParticleDownRight or
                    > RoomEnemyProjectileKind.MetareeParticleUpLeft ||
                projectile.InstructionPointer != 0x8ac5 ||
                projectile.GraphicsIndex != packedSpawnGraphics))
        {
            throw new InvalidDataException(
                "Metaree special shot death did not create all four metal ROM projectiles.");
        }

        enemies.StepEnemyProjectiles(
            assets.LevelData,
            samus,
            cameraX: cameraX,
            cameraY: cameraY);
        if (shotDeathDebris.Any(projectile => projectile.SpritemapPointer != 0x8435))
        {
            throw new InvalidDataException(
                "Metaree special shot death debris did not select spritemap $8D:8435.");
        }

        // Advance only bank-$86 until the shot-death burst leaves the camera. The later
        // burrow assertion can then prove a fresh four-particle producer rather than merely
        // observing debris left over from this independent combat fixture.
        for (int frame = 0; frame < 160 && enemies.ActiveEnemyProjectileCount != 0; frame++)
        {
            enemies.StepEnemyProjectiles(
                assets.LevelData,
                samus,
                cameraX: cameraX,
                cameraY: cameraY);
        }
        if (enemies.ActiveEnemyProjectileCount != 0)
        {
            throw new InvalidDataException(
                $"Metaree shot-death debris retained {enemies.ActiveEnemyProjectileCount} actors.");
        }

        // A 96-pixel vertical difference divided by the NTSC constant 24, plus four,
        // produces exactly eight pixels per frame. Keeping this arithmetic asserted catches
        // accidental signed conversion or host-physics substitution immediately.
        samus.XPosition = diving.XPosition;
        ushort requestedSamusY = unchecked((ushort)(diving.YPosition + 0x0060));
        samus.YPosition = requestedSamusY;
        enemies.StepFrame(cameraX, cameraY, false, samus, level: assets.LevelData);
        animationMaps.Add(diving.SpritemapPointer);
        if (divingState.Function != MetareeEnemyFunction.PreparingAttack ||
            divingState.YVelocity != 8 ||
            divingState.RequestedInstructionListIndex != 1 ||
            diving.CurrentInstruction != 0x8928)
        {
            throw new InvalidDataException(
                $"Metaree activation failed: function=$A3:{(ushort)divingState.Function:X4}, " +
                $"Y velocity={divingState.YVelocity}, " +
                $"list={divingState.RequestedInstructionListIndex}/" +
                $"${diving.CurrentInstruction:X4}.");
        }

        ushort startX = diving.XPosition;
        ushort startY = diving.YPosition;
        bool sawAttackReadyInstruction = false;
        bool sawLaunch = false;
        bool sawBurrow = false;
        bool sawLaunchSound = false;
        bool sawBurrowSound = false;
        bool sawMetalDebris = false;
        ushort maximumY = diving.YPosition;

        for (int frame = 0; frame < 160 &&
             !diving.Properties.HasAny(EnemyProperties.Deleted); frame++)
        {
            enemies.StepFrame(cameraX, cameraY, false, samus, level: assets.LevelData);
            animationMaps.Add(diving.SpritemapPointer);
            maximumY = Math.Max(maximumY, diving.YPosition);
            sawAttackReadyInstruction |= divingState.AttackReady;
            sawLaunch |= divingState.Function == MetareeEnemyFunction.LaunchedAttack;
            sawBurrow |= divingState.Function == MetareeEnemyFunction.Burrowing;
            sawLaunchSound |= enemies.LastMetareeSoundEffect == 0x005b;
            sawBurrowSound |= enemies.LastMetareeSoundEffect == 0x005c;

            enemies.StepEnemyProjectiles(
                assets.LevelData,
                samus,
                cameraX: cameraX,
                cameraY: cameraY);
            sawMetalDebris |= enemies.EnemyProjectiles.Count(projectile =>
                projectile.Kind is >= RoomEnemyProjectileKind.MetareeParticleDownRight and
                    <= RoomEnemyProjectileKind.MetareeParticleUpLeft &&
                projectile.SpritemapPointer == 0x8435) == 4;
        }

        if (!sawAttackReadyInstruction || !sawLaunch || !sawBurrow ||
            !sawLaunchSound || !sawBurrowSound || !sawMetalDebris ||
            maximumY <= startY || diving.XPosition == startX ||
            animationMaps.Count < 5 ||
            !diving.Properties.HasAny(EnemyProperties.Deleted) ||
            diving.PaletteIndex != 0x0a00 || diving.VramTilesIndex != 0 ||
            diving.Spawn.VramTilesIndex != packedSpawnGraphics)
        {
            throw new InvalidDataException(
                $"Metaree attack/burrow failed: ready={sawAttackReadyInstruction}, " +
                $"launch/burrow={sawLaunch}/{sawBurrow}, " +
                $"sounds={sawLaunchSound}/{sawBurrowSound}, debris={sawMetalDebris}, " +
                $"position=({startX:X4},{startY:X4})->" +
                $"({diving.XPosition:X4},{maximumY:X4}), maps={animationMaps.Count}, " +
                $"properties=${diving.Properties:X4}, graphics=" +
                $"${diving.PaletteIndex:X4}/${diving.VramTilesIndex:X4}/" +
                $"${diving.Spawn.VramTilesIndex:X4}.");
        }

        // Prove the retail above-Samus bug in a fresh production loader. `$A3:89AC` feeds
        // wrapped unsigned $FFFF to the hardware divider for a one-pixel-above target:
        // $FFFF / 24 + 4 = $0AAE. A seemingly reasonable signed clamp would fail this guard.
        var bugEnemies = new RoomEnemySystem();
        bugEnemies.Load(
            auditBus,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            new SnesVram(),
            new SnesCgram(),
            random.NextRandom,
            random.SetRandomNumber);
        RoomEnemySlot bugMetaree = bugEnemies.Slots[2];
        MetareeEnemyState bugState = bugEnemies.MetareeStates[2]
            ?? throw new InvalidDataException("Bug-fixture Metaree has no typed state.");
        var aboveSamus = new SamusState
        {
            Pose = SamusState.FacingRightNormalPose,
            XPosition = bugMetaree.XPosition,
            YPosition = unchecked((ushort)(bugMetaree.YPosition - 1)),
        };
        aboveSamus.RefreshCollisionRadii(auditBus);
        aboveSamus.InitializeAnimation(auditBus);
        bugEnemies.StepFrame(cameraX, cameraY, false, aboveSamus, level: assets.LevelData);
        if (bugState.Function != MetareeEnemyFunction.PreparingAttack ||
            bugState.YVelocity != 0x0aae)
        {
            throw new InvalidDataException(
                $"Metaree unsigned-divider bug produced function " +
                $"$A3:{(ushort)bugState.Function:X4} and Y velocity " +
                $"${bugState.YVelocity:X4}, expected $A3:89D4/$0AAE.");
        }

        Console.WriteLine(
            "Dachora Metaree audit passed: three retail actors loaded after two Zeelas; " +
            $"idle/prepare/dive/burrow bytecode produced {animationMaps.Count} maps, " +
            $"flight moved ({startX:X4},{startY:X4})->" +
            $"({diving.XPosition:X4},{maximumY:X4}), both sounds fired, metal debris used " +
            "$8D:8435, touch dealt 50, vulnerability damage/death and the $0AAE " +
            "unsigned-divider bug resolved, and " +
            $"{oam.LastFinalizedSpriteCount} OBJ pieces rendered.");
        return 0;
    }

    private static void ArmProjectile(
        SamusProjectileSlot projectile,
        RoomEnemySlot target,
        ushort type,
        ushort damage)
    {
        projectile.ClearFields();
        projectile.Type = type;
        projectile.Damage = damage;
        projectile.Direction = 2;
        projectile.XPosition = target.XPosition;
        projectile.YPosition = target.YPosition;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    /// <summary>
    /// Read-only cartridge decorator that replaces exactly one future population record
    /// with the native three-byte terminator. It exists only in this audit assembly and
    /// cannot alter production room loading or hide untranslated actors during gameplay.
    /// </summary>
    private sealed class PopulationTerminatingAddressSpace : ISnesAddressSpace
    {
        private readonly ISnesAddressSpace _inner;
        private readonly int _terminatorAddress;
        private readonly byte _deathQuota;

        public PopulationTerminatingAddressSpace(
            ISnesAddressSpace inner,
            ushort populationPointer,
            int retainedRecordCount,
            byte deathQuota)
        {
            _inner = inner;
            _terminatorAddress = 0xa10000 |
                unchecked((ushort)(populationPointer + retainedRecordCount * 16));
            _deathQuota = deathQuota;
        }

        public byte ReadByte(int address)
        {
            if (address == _terminatorAddress || address == _terminatorAddress + 1)
                return 0xff;
            if (address == _terminatorAddress + 2)
                return _deathQuota;
            return _inner.ReadByte(address);
        }

        public void WriteByte(int address, byte value) =>
            _inner.WriteByte(address, value);
    }
}
