using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>
/// ROM-backed regression for retail Boulder <c>$DFBF</c>. The four shipped populations
/// cover both roll directions, the direction-two impact/deletion variant, one-pixel and
/// large initial drops, both ground-compensation values, and the zero-height shortcut.
/// Prefix address spaces terminate only the selected consecutive Boulder records; every
/// actor word, room collision block, graphics asset, instruction list, and speed-table
/// entry remains sourced from the cartridge under test.
/// </summary>
internal static class BoulderAudit
{
    private const ushort BoulderDefinition = 0xdfbf;
    private const ushort BlueBrinstarBoulderRoom = 0xa1ad;
    private const ushort BatCaveRoom = 0xb6ee;
    private const ushort BlueBrinstarPopulation = 0x9505;
    private const ushort BatCavePopulation = 0xab80;

    private static readonly PopulationExpectation[] RetailPopulations =
    [
        new(0x9505,
        [
            new(0x0158, 0x00c0, 0x0080, 0x2000, 0x0000, 0x0200, 0xa050),
            new(0x00f0, 0x00c0, 0x0080, 0x2000, 0x0000, 0x0200, 0xa050),
            new(0x0090, 0x00c0, 0x0080, 0x2000, 0x0000, 0x0200, 0xa050),
        ]),
        new(0xab80,
        [
            new(0x0150, 0x0130, 0x0050, 0x2000, 0x0000, 0x0000, 0x0080),
            new(0x01b8, 0x01d0, 0x0050, 0x2800, 0x0000, 0x0100, 0x0080),
            new(0x0128, 0x0260, 0x0050, 0x2800, 0x0000, 0x0000, 0x0080),
        ]),
        new(0xdf63,
        [
            new(0x0190, 0x00a0, 0x0072, 0x2800, 0x0000, 0x0200, 0x7204),
            new(0x0150, 0x00c0, 0x0098, 0x2800, 0x0000, 0x0200, 0xa204),
            new(0x00d0, 0x00d0, 0x00c0, 0x2800, 0x0000, 0x0200, 0xa204),
        ]),
        new(0xdf96,
        [
            new(0x01d0, 0x0090, 0x0050, 0x2800, 0x0000, 0x0200, 0x6204),
            new(0x00b0, 0x0140, 0x0080, 0x2800, 0x0000, 0x0200, 0xa004),
            new(0x00f0, 0x0160, 0x00f0, 0x2800, 0x0000, 0x0200, 0xf004),
            new(0x0030, 0x0090, 0x0040, 0x2800, 0x0000, 0x0200, 0x5204),
        ]),
    ];

    private static readonly HashSet<ushort> BoulderMaps =
    [
        0x8a59, 0x8a6f, 0x8a85, 0x8a9b,
        0x8ab1, 0x8ac7, 0x8add, 0x8af3,
    ];

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        VerifyDefinitionHeader(bus);
        VerifyAllRetailPopulationRecords(bus);

        CartridgeRoomHeader blueRoom = CartridgeRoomHeader.Load(bus, BlueBrinstarBoulderRoom);
        CartridgeRoomAssets blueAssets = CartridgeRoomAssets.Load(bus, blueRoom);
        LoadedBoulders blue = LoadPrefix(
            bus,
            blueRoom,
            blueAssets,
            BlueBrinstarPopulation,
            retainedRecordCount: 3);
        VerifyInitializationAndDrawing(blueRoom, blueAssets, blue);
        VerifyStrictTriggerAndImpactLifecycle(blueAssets, blue);

        CartridgeRoomHeader batRoom = CartridgeRoomHeader.Load(bus, BatCaveRoom);
        CartridgeRoomAssets batAssets = CartridgeRoomAssets.Load(bus, batRoom);
        LoadedBoulders right = LoadPrefix(
            bus,
            batRoom,
            batAssets,
            unchecked((ushort)(BatCavePopulation + 5 * 16)),
            retainedRecordCount: 3);
        VerifyBothDirectionsAndZeroHeightShortcut(batAssets, right);

        VerifyCommonCombat(bus, blueRoom, blueAssets);

        Console.WriteLine(
            "Boulder audit passed: all 13 retail records matched ROM; both animation " +
            "directions emitted the eight authored maps; strict proximity triggering, " +
            "quadratic fall/rebound/roll states, collision dust and sounds, zero-height " +
            "shortcut, contact damage, ROM vulnerability reactions, and grapple " +
            "cancel were verified.");
        return 0;
    }

    private static void VerifyDefinitionHeader(SuperMetroidAddressSpace bus)
    {
        RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(bus, BoulderDefinition);
        if (definition.Bank != 0xa6 || definition.InitializationAiPointer != 0x86f5 ||
            definition.MainAiPointer != 0x8793 || definition.TouchAiPointer != 0x8023 ||
            definition.ShotAiPointer != 0x802d || definition.Health != 20 ||
            definition.Damage != 40 || definition.NamePointer != 0xe0b5)
        {
            throw new InvalidDataException(
                "Boulder $DFBF header disagrees with the translated dispatcher: " +
                $"bank=${definition.Bank:X2}, init=${definition.InitializationAiPointer:X4}, " +
                $"main=${definition.MainAiPointer:X4}, touch=${definition.TouchAiPointer:X4}, " +
                $"shot=${definition.ShotAiPointer:X4}, health/damage=" +
                $"{definition.Health}/{definition.Damage}, name=${definition.NamePointer:X4}.");
        }
    }

    private static void VerifyAllRetailPopulationRecords(SuperMetroidAddressSpace bus)
    {
        foreach (PopulationExpectation population in RetailPopulations)
        {
            var actual = new List<BoulderRecord>();
            int cursor = 0xa10000 | population.Pointer;
            while (true)
            {
                ushort definition = ReadWord(bus, cursor);
                if (definition == 0xffff)
                    break;
                if (definition == BoulderDefinition)
                {
                    actual.Add(new BoulderRecord(
                        ReadWord(bus, cursor + 2),
                        ReadWord(bus, cursor + 4),
                        ReadWord(bus, cursor + 6),
                        ReadWord(bus, cursor + 8),
                        ReadWord(bus, cursor + 10),
                        ReadWord(bus, cursor + 12),
                        ReadWord(bus, cursor + 14)));
                }
                cursor = 0xa10000 | unchecked((ushort)(cursor + 16));
            }

            if (!actual.SequenceEqual(population.Records))
            {
                throw new InvalidDataException(
                    $"Boulder population $A1:{population.Pointer:X4} contained " +
                    $"{actual.Count} translated records instead of {population.Records.Length}.");
            }
        }
    }

    private static void VerifyInitializationAndDrawing(
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        LoadedBoulders loaded)
    {
        if (room.State.Pointer != 0xa1ba || loaded.Enemies.EnemyCount != 3)
        {
            throw new InvalidDataException(
                $"Blue Brinstar Boulder room selected state ${room.State.Pointer:X4} " +
                $"with {loaded.Enemies.EnemyCount} actors.");
        }

        for (int index = 0; index < 3; index++)
        {
            RoomEnemySlot actor = loaded.Enemies.Slots[index];
            BoulderEnemyState state = RequireState(loaded.Enemies, actor);
            ushort expectedX = index switch { 0 => 0x0158, 1 => 0x00f0, _ => 0x0090 };
            if (actor.EnemyDefinitionPointer != BoulderDefinition ||
                actor.XPosition != expectedX || actor.YPosition != 0x0020 ||
                actor.CurrentInstruction != 0x86a7 ||
                state.Function != BoulderAiFunction.WaitingForSamus ||
                state.HorizontalSpeedAccumulator != 0 ||
                state.VerticalSpeedAccumulator != 0 || state.BounceCounter != 2 ||
                state.DirectionSelector != 2 || state.InitialYPosition != 0x00c0 ||
                state.HorizontalTriggerLimit != -0x50 || state.VerticalTriggerLimit != 0x80 ||
                state.VerticalCompensation != 2 || state.StartsRollingWithoutBounce)
            {
                throw new InvalidDataException(
                    $"Blue Brinstar Boulder {index} initialization failed: position=" +
                    $"(${actor.XPosition:X4},${actor.YPosition:X4}), list=" +
                    $"${actor.CurrentInstruction:X4}, function={state.Function}, speed=" +
                    $"${state.HorizontalSpeedAccumulator:X4}/${state.VerticalSpeedAccumulator:X4}, " +
                    $"direction={state.DirectionSelector}, trigger=" +
                    $"{state.HorizontalTriggerLimit}/{state.VerticalTriggerLimit}, " +
                    $"compensation={state.VerticalCompensation}.");
            }
        }

        RoomEnemySlot first = loaded.Enemies.Slots[0];
        loaded.Samus.XPosition = 0;
        loaded.Samus.YPosition = 0;
        var maps = new HashSet<ushort>();
        for (int frame = 0; frame < 80; frame++)
        {
            StepCentered(loaded, assets, first);
            maps.Add(first.SpritemapPointer);
        }
        if (!maps.SetEquals(BoulderMaps))
        {
            throw new InvalidDataException(
                $"Boulder left list emitted [{FormatWords(maps)}], expected " +
                $"[{FormatWords(BoulderMaps)}].");
        }

        var oam = new OamBuffer();
        oam.BeginFrame();
        loaded.Enemies.DrawLayers(oam, CameraFor(room, first), 0, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException("Waiting Boulders emitted no ROM-authored OBJ pieces.");
    }

    private static void VerifyStrictTriggerAndImpactLifecycle(
        CartridgeRoomAssets assets,
        LoadedBoulders loaded)
    {
        RoomEnemySlot actor = loaded.Enemies.Slots[0];
        BoulderEnemyState state = RequireState(loaded.Enemies, actor);

        // The negative-direction window includes its lower bound but excludes the upper
        // bound; the vertical window excludes its upper bound. Exercise one pixel beyond
        // the lower edge plus both excluded equalities before entering the valid interval.
        loaded.Samus.XPosition = unchecked((ushort)(actor.XPosition - 0x51));
        loaded.Samus.YPosition = unchecked((ushort)(actor.YPosition + 1));
        StepCentered(loaded, assets, actor);
        loaded.Samus.XPosition = actor.XPosition;
        StepCentered(loaded, assets, actor);
        loaded.Samus.XPosition = unchecked((ushort)(actor.XPosition - 1));
        loaded.Samus.YPosition = unchecked((ushort)(actor.YPosition + 0x80));
        StepCentered(loaded, assets, actor);
        if (state.Function != BoulderAiFunction.WaitingForSamus)
            throw new InvalidDataException("Boulder entered motion at a strict trigger boundary.");

        loaded.Samus.YPosition = unchecked((ushort)(actor.YPosition + 1));
        StepCentered(loaded, assets, actor);
        if (state.Function != BoulderAiFunction.InitialFall)
            throw new InvalidDataException($"Boulder open trigger selected {state.Function}.");

        var seen = new HashSet<BoulderAiFunction> { state.Function };
        bool sawDust = false;
        bool sawBreakSound = false;
        for (int frame = 0; frame < 900 &&
            !actor.Properties.HasAny(EnemyProperties.Deleted); frame++)
        {
            StepCentered(loaded, assets, actor);
            seen.Add(state.Function);
            sawDust |= loaded.Enemies.EnemyProjectiles.Any(projectile =>
                projectile.Kind == RoomEnemyProjectileKind.MiscDustExplosion);
            sawBreakSound |= loaded.Enemies.LastBoulderSoundEffect == 0x0043;
        }

        BoulderAiFunction[] required =
        [
            BoulderAiFunction.InitialFall,
            BoulderAiFunction.Rebound,
            BoulderAiFunction.Falling,
        ];
        if (required.Any(function => !seen.Contains(function)) ||
            !actor.Properties.HasAny(EnemyProperties.Deleted) || !sawDust || !sawBreakSound)
        {
            throw new InvalidDataException(
                $"Direction-two Boulder lifecycle failed: states=" +
                $"[{string.Join(',', seen)}], properties=${actor.Properties:X4}, " +
                $"dust={sawDust}, breakSound={sawBreakSound}.");
        }
    }

    private static void VerifyBothDirectionsAndZeroHeightShortcut(
        CartridgeRoomAssets assets,
        LoadedBoulders loaded)
    {
        RoomEnemySlot right = loaded.Enemies.Slots[0];
        RoomEnemySlot left = loaded.Enemies.Slots[1];
        BoulderEnemyState rightState = RequireState(loaded.Enemies, right);
        BoulderEnemyState leftState = RequireState(loaded.Enemies, left);
        if (right.CurrentInstruction != 0x86cb || rightState.DirectionSelector != 0 ||
            rightState.HorizontalTriggerLimit != 0x0080 ||
            !rightState.StartsRollingWithoutBounce || right.YPosition != 0x012f ||
            left.CurrentInstruction != 0x86a7 || leftState.DirectionSelector != 1 ||
            leftState.HorizontalTriggerLimit != -0x80 ||
            !leftState.StartsRollingWithoutBounce || left.YPosition != 0x01cf)
        {
            throw new InvalidDataException(
                $"Bat Cave direction initialization failed: right list/function/trigger=" +
                $"${right.CurrentInstruction:X4}/{rightState.Function}/" +
                $"{rightState.HorizontalTriggerLimit}, left=${left.CurrentInstruction:X4}/" +
                $"{leftState.Function}/{leftState.HorizontalTriggerLimit}.");
        }

        var rightMaps = new HashSet<ushort>();
        loaded.Samus.XPosition = 0;
        loaded.Samus.YPosition = 0;
        for (int frame = 0; frame < 80; frame++)
        {
            StepCentered(loaded, assets, right);
            rightMaps.Add(right.SpritemapPointer);
        }
        if (!rightMaps.SetEquals(BoulderMaps))
        {
            throw new InvalidDataException(
                $"Boulder right list emitted [{FormatWords(rightMaps)}], expected " +
                $"[{FormatWords(BoulderMaps)}].");
        }

        // The high byte of parameter two is zero for all three Bat Cave Boulders. Native
        // substitutes a one-pixel initial offset and jumps straight to rolling after the
        // normal strict proximity test instead of entering the bounce chain.
        loaded.Samus.XPosition = unchecked((ushort)(right.XPosition + 1));
        loaded.Samus.YPosition = unchecked((ushort)(right.YPosition + 1));
        StepCentered(loaded, assets, right);
        loaded.Samus.XPosition = unchecked((ushort)(left.XPosition - 1));
        loaded.Samus.YPosition = unchecked((ushort)(left.YPosition + 1));
        StepCentered(loaded, assets, left);
        if (rightState.Function != BoulderAiFunction.Rolling ||
            leftState.Function != BoulderAiFunction.Rolling)
        {
            throw new InvalidDataException(
                $"Zero-height Boulders selected {rightState.Function}/{leftState.Function}, " +
                "expected direct rolling.");
        }
    }

    private static void VerifyCommonCombat(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedBoulders contact = LoadPrefix(bus, room, assets, BlueBrinstarPopulation, 1);
        RoomEnemySlot actor = contact.Enemies.Slots[0];
        StepCentered(contact, assets, actor);
        contact.Samus.XPosition = actor.XPosition;
        contact.Samus.YPosition = actor.YPosition;
        ushort health = contact.Samus.Health;
        if (!contact.Enemies.ResolveOrdinarySamusContact(contact.Samus, 0) ||
            contact.Samus.Health != health - 40 || !contact.Samus.KnockbackActive)
        {
            throw new InvalidDataException(
                $"Boulder contact failed: health={health}->{contact.Samus.Health}, " +
                $"knockback={contact.Samus.KnockbackActive}.");
        }

        LoadedBoulders shot = LoadPrefix(bus, room, assets, BlueBrinstarPopulation, 1);
        actor = shot.Enemies.Slots[0];
        StepCentered(shot, assets, actor);
        var projectiles = new SamusProjectileSystem();
        var shared = new SamusBombProjectileSystem();
        ArmProjectile(projectiles.Slots[0], actor, type: 0x0100, damage: 20);
        byte missileVulnerability = ReadVulnerability(bus, actor, 12);
        int missileDamage = missileVulnerability == 0xff
            ? 0
            : (20 >> 1) * (missileVulnerability & 0x7f);
        ushort expectedMissileHealth = unchecked((ushort)Math.Max(0, 20 - missileDamage));
        int hits = shot.Enemies.ResolveOrdinaryProjectileHits(
            bus,
            projectiles,
            shared,
            shot.Samus);
        if (hits != 1 || actor.Health != expectedMissileHealth ||
            actor.Properties.HasAny(EnemyProperties.Deleted) != (expectedMissileHealth == 0))
        {
            throw new InvalidDataException(
                $"Boulder missile failed: hits={hits}, health={actor.Health}, " +
                $"vulnerability=${missileVulnerability:X2}, properties=${actor.Properties:X4}.");
        }

        LoadedBoulders frozen = LoadPrefix(bus, room, assets, BlueBrinstarPopulation, 1);
        actor = frozen.Enemies.Slots[0];
        StepCentered(frozen, assets, actor);
        frozen.Samus.EquippedBeams = 2;
        ArmProjectile(projectiles.Slots[0], actor, type: 0x0002, damage: 2);
        byte iceVulnerability = ReadVulnerability(bus, actor, 2);
        frozen.Enemies.ResolveOrdinaryProjectileHits(bus, projectiles, shared, frozen.Samus);
        int iceDamage = iceVulnerability == 0xff
            ? 0
            : (2 >> 1) * (iceVulnerability & 0x7f);
        ushort expectedIceHealth = unchecked((ushort)Math.Max(0, 20 - iceDamage));
        bool expectedFrozen = iceVulnerability == 0xff;
        if ((actor.FrozenTimer != 0) != expectedFrozen ||
            ((actor.AiHandlerBits & 4) != 0) != expectedFrozen ||
            actor.Health != expectedIceHealth)
        {
            throw new InvalidDataException(
                $"Boulder ice reaction disagreed with vulnerability ${iceVulnerability:X2}: " +
                $"health={actor.Health}, frozen={actor.FrozenTimer}, AI=${actor.AiHandlerBits:X4}.");
        }

        LoadedBoulders powerBomb = LoadPrefix(bus, room, assets, BlueBrinstarPopulation, 1);
        actor = powerBomb.Enemies.Slots[0];
        StepCentered(powerBomb, assets, actor);
        byte powerBombVulnerability = ReadVulnerability(bus, actor, 14);
        int reactions = powerBomb.Enemies.ResolveOrdinaryPowerBombHits(
            bus,
            actor.XPosition,
            actor.YPosition,
            explosionRadius: 32);
        bool admitted = (powerBombVulnerability & 0x7f) != 0;
        int powerBombDamage = powerBombVulnerability == 0xff
            ? 0
            : 100 * (powerBombVulnerability & 0x7f);
        ushort expectedPowerBombHealth = unchecked((ushort)Math.Max(0, 20 - powerBombDamage));
        if (reactions != (admitted ? 1 : 0) || actor.Health != expectedPowerBombHealth ||
            actor.Properties.HasAny(EnemyProperties.Deleted) !=
                (admitted && expectedPowerBombHealth == 0))
        {
            throw new InvalidDataException(
                $"Boulder power bomb failed: reactions={reactions}, health={actor.Health}, " +
                $"vulnerability=${powerBombVulnerability:X2}, properties=${actor.Properties:X4}.");
        }

        LoadedBoulders grapple = LoadPrefix(bus, room, assets, BlueBrinstarPopulation, 1);
        actor = grapple.Enemies.Slots[0];
        StepCentered(grapple, assets, actor);
        GrappleEnemyCollision collision = grapple.Enemies.ResolveGrappleEndpoint(
            actor.XPosition,
            actor.YPosition);
        if (!collision.Collided || collision.Reaction != GrappleEnemyReaction.Cancel ||
            actor.AiHandlerBits != 1)
        {
            throw new InvalidDataException(
                $"Boulder grapple selected {collision.Reaction} with handler " +
                $"${actor.AiHandlerBits:X4}.");
        }
    }

    private static LoadedBoulders LoadPrefix(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        ushort populationPointer,
        int retainedRecordCount)
    {
        var prefixBus = new PopulationPrefixAddressSpace(
            bus,
            populationPointer,
            retainedRecordCount,
            deathQuota: 0);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState();
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusState.FacingRightNormalPose,
        };
        samus.RefreshCollisionRadii(prefixBus);
        samus.InitializeAnimation(prefixBus);
        var enemies = new RoomEnemySystem();
        enemies.Load(
            prefixBus,
            populationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            random.NextRandom,
            random.SetRandomNumber,
            readRandomNumber: () => random.RandomNumber,
            level: assets.LevelData,
            samus: samus);
        return new LoadedBoulders(enemies, samus, room);
    }

    private static void StepCentered(
        LoadedBoulders loaded,
        CartridgeRoomAssets assets,
        RoomEnemySlot actor)
    {
        loaded.Enemies.StepFrame(
            CameraFor(loaded.Room, actor),
            CameraYFor(loaded.Room, actor),
            false,
            loaded.Samus,
            level: assets.LevelData);
    }

    private static ushort CameraFor(CartridgeRoomHeader room, RoomEnemySlot actor) =>
        unchecked((ushort)Math.Clamp(
            actor.XPosition - 128,
            0,
            Math.Max(0, room.WidthInScreens * 256 - 256)));

    private static ushort CameraYFor(CartridgeRoomHeader room, RoomEnemySlot actor) =>
        unchecked((ushort)Math.Clamp(
            actor.YPosition - 128,
            0,
            Math.Max(0, room.HeightInScreens * 256 - 256)));

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

    private static BoulderEnemyState RequireState(RoomEnemySystem enemies, RoomEnemySlot actor) =>
        enemies.BoulderStates[actor.SlotIndex] ?? throw new InvalidDataException(
            $"Boulder slot {actor.SlotIndex} has no typed state.");

    private static ushort ReadWord(SuperMetroidAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private static byte ReadVulnerability(
        SuperMetroidAddressSpace bus,
        RoomEnemySlot actor,
        int byteOffset)
    {
        ushort pointer = actor.Definition.VulnerabilityPointer != 0
            ? actor.Definition.VulnerabilityPointer
            : (ushort)0xec1c;
        return bus.ReadByte(0xb40000 | unchecked((ushort)(pointer + byteOffset)));
    }

    private static string FormatWords(IEnumerable<ushort> words) =>
        string.Join(',', words.Order().Select(word => $"${word:X4}"));

    private readonly record struct PopulationExpectation(
        ushort Pointer,
        BoulderRecord[] Records);

    private readonly record struct BoulderRecord(
        ushort X,
        ushort Y,
        ushort InitializationParameter,
        ushort Properties,
        ushort ExtraProperties,
        ushort Parameter1,
        ushort Parameter2);

    private readonly record struct LoadedBoulders(
        RoomEnemySystem Enemies,
        SamusState Samus,
        CartridgeRoomHeader Room);
}
