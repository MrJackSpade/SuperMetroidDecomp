using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

internal static class DachoraAudit
{
    private const ushort DachoraDefinition = 0xe5ff;
    private const ushort DachoraRoomHeader = 0x9cb3;
    private const ushort DachoraRoomState = 0x9cc0;
    private const ushort DachoraPopulation = 0x9d5c;
    private const ushort FirstDachoraRecord = 0x9ddc;

    private static readonly DachoraRecord[] RetailRecords =
    [
        new(0x0060, 0x06a8, 0x0000, 0x0c00, 0x0000, 0x0001, 0x0000),
        new(0x0060, 0x06a8, 0x0000, 0x0d00, 0x0000, 0x8001, 0x0000),
        new(0x0060, 0x06a8, 0x0000, 0x0d00, 0x0000, 0x8001, 0x0000),
        new(0x0060, 0x06a8, 0x0000, 0x0d00, 0x0000, 0x8001, 0x0000),
        new(0x0060, 0x06a8, 0x0000, 0x0d00, 0x0000, 0x8001, 0x0000),
    ];

    // Spritemaps reached by the shipped right-facing body and its four right-facing echoes.
    // Maps 6/7 and 8-E belong to the unused left-charge/left-echo and non-retail left-idle
    // variants. The translation retains those lists, but this retail-population audit does
    // not pretend unreachable parameter combinations are runtime evidence.
    private static readonly HashSet<ushort> RetailReachableSpritemaps =
    [
        0xf9c4, 0xf9f3, 0xfa22, 0xfa5b, 0xfa8a, 0xfab9,
        0xfca3, 0xfcd2, 0xfd01, 0xfd3a, 0xfd69, 0xfd98,
        0xfdd1, 0xfdfb, 0xfe2f, 0xfe5e, 0xfe92, 0xfec6,
        0xfef5, 0xff24, 0xff53,
    ];

    private static readonly ushort[] SpeedPalettes = [0xf245, 0xf265, 0xf285, 0xf2a5];
    private static readonly ushort[] ShinePalettes = [0xf2c5, 0xf2e5, 0xf305, 0xf325];

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        VerifyDefinitionHeader(bus);
        VerifyRetailPopulation(bus);

        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, DachoraRoomHeader);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        if (room.State.Pointer != DachoraRoomState ||
            room.State.EnemyPopulationPointer != DachoraPopulation)
        {
            throw new InvalidDataException(
                $"Dachora room selected state ${room.State.Pointer:X4} and " +
                $"population ${room.State.EnemyPopulationPointer:X4}.");
        }

        VerifyInitializationIdleAndDrawing(bus, room, assets);
        VerifyCompleteRetailLifecycle(bus, room, assets);
        VerifyAuthoredNonCombatBehavior(bus, room, assets);

        Console.WriteLine(
            "Dachora audit passed: the five retail body/echo records, idle and warning " +
            "animation, fixed-point acceleration, three run speeds, speed/shine palettes, " +
            "slope alignment, stored-shine launch, ceiling reversal, fall, four-slot echo " +
            "pipeline, alternating visibility, drawing, and authored no-combat behavior " +
            "were verified from ROM data.");
        return 0;
    }

    private static void VerifyDefinitionHeader(SuperMetroidAddressSpace bus)
    {
        RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(bus, DachoraDefinition);
        if (definition.TileDataSize != 0x0c00 || definition.PalettePointer != 0xf225 ||
            definition.Health != 0x7fff || definition.Damage != 0 ||
            definition.XRadius != 8 || definition.YRadius != 0x18 ||
            definition.Bank != 0xa7 || definition.InitializationAiPointer != 0xf4dd ||
            definition.PartCount != 1 || definition.MainAiPointer != 0xf52e ||
            definition.GrappleAiPointer != 0x8000 || definition.HurtAiPointer != 0x804c ||
            definition.FrozenAiPointer != 0x8041 || definition.DeathAnimation != 0 ||
            definition.PowerBombReactionPointer != 0x804c ||
            definition.TouchAiPointer != 0x804c || definition.ShotAiPointer != 0x804c ||
            definition.Layer != 5 || definition.VulnerabilityPointer != 0)
        {
            throw new InvalidDataException(
                "Dachora $E5FF header disagrees with the translated family: " +
                $"size/palette=${definition.TileDataSize:X4}/${definition.PalettePointer:X4}, " +
                $"hp/damage={definition.Health}/{definition.Damage}, " +
                $"radius={definition.XRadius}/{definition.YRadius}, " +
                $"bank/init/main=${definition.Bank:X2}:${definition.InitializationAiPointer:X4}/" +
                $"${definition.MainAiPointer:X4}, touch/shot=" +
                $"${definition.TouchAiPointer:X4}/${definition.ShotAiPointer:X4}.");
        }
    }

    private static void VerifyRetailPopulation(SuperMetroidAddressSpace bus)
    {
        var actual = new List<DachoraRecord>();
        int cursor = 0xa10000 | DachoraPopulation;
        for (int index = 0; index < RoomEnemySystem.MaximumEnemyCount; index++, cursor += 16)
        {
            ushort definition = ReadWord(bus, cursor);
            if (definition == 0xffff)
                break;
            if (definition != DachoraDefinition)
                continue;

            actual.Add(new DachoraRecord(
                ReadWord(bus, cursor + 2),
                ReadWord(bus, cursor + 4),
                ReadWord(bus, cursor + 6),
                ReadWord(bus, cursor + 8),
                ReadWord(bus, cursor + 10),
                ReadWord(bus, cursor + 12),
                ReadWord(bus, cursor + 14)));
        }

        if (!actual.SequenceEqual(RetailRecords))
        {
            throw new InvalidDataException(
                $"Dachora room contained {actual.Count} body/echo records instead of " +
                "the exact five-record retail composite.");
        }
    }

    private static void VerifyInitializationIdleAndDrawing(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedDachora loaded = Load(bus, room, assets, retainedRecordCount: 5);
        if (loaded.Enemies.EnemyCount != 5)
            throw new InvalidDataException($"Dachora prefix loaded {loaded.Enemies.EnemyCount} actors.");

        for (int index = 0; index < RetailRecords.Length; index++)
        {
            RoomEnemySlot actor = loaded.Enemies.Slots[index];
            DachoraEnemyState state = RequireState(loaded.Enemies, actor);
            DachoraRecord record = RetailRecords[index];
            ushort expectedProperties = index == 0 ? (ushort)0x2c00 : (ushort)0x2d00;
            ushort expectedList = index == 0 ? (ushort)0xf45b : (ushort)0xf4b9;
            DachoraAiFunction expectedFunction = index == 0
                ? DachoraAiFunction.WaitingForSamus
                : DachoraAiFunction.Echo;
            if (actor.XPosition != record.X || actor.YPosition != record.Y ||
                actor.CurrentInstruction != expectedList || actor.SpritemapPointer != 0x804d ||
                actor.Properties != expectedProperties || actor.Parameter1 != record.Parameter1 ||
                state.Function != expectedFunction)
            {
                throw new InvalidDataException(
                    $"Dachora record {index} initialization failed: position=" +
                    $"(${actor.XPosition:X4},${actor.YPosition:X4}), list/map=" +
                    $"${actor.CurrentInstruction:X4}/${actor.SpritemapPointer:X4}, " +
                    $"properties=${actor.Properties:X4}, parameter1=${actor.Parameter1:X4}, " +
                    $"function={state.Function}.");
            }
        }

        // Keep Samus outside both strict proximity bands and let the complete shipped idle
        // list play. This proves long-duration facial frames instead of sampling only frame 0.
        loaded.Samus.XPosition = 0x0200;
        loaded.Samus.YPosition = 0;
        var idleMaps = new HashSet<ushort>();
        for (int frame = 0; frame < 132; frame++)
        {
            Step(loaded, room, assets, frame);
            idleMaps.Add(loaded.Enemies.Slots[0].SpritemapPointer);
        }
        ushort[] expectedIdle = [0xfe2f, 0xfec6, 0xfef5, 0xff24, 0xff53];
        if (expectedIdle.Any(map => !idleMaps.Contains(map)))
        {
            throw new InvalidDataException(
                $"Dachora right idle emitted [{FormatWords(idleMaps)}], missing " +
                $"[{FormatWords(expectedIdle.Except(idleMaps))}].");
        }
        if (loaded.Enemies.Slots.Skip(1).Take(4).Any(
                echo => !echo.Properties.HasAny(EnemyProperties.Invisible)))
        {
            throw new InvalidDataException("Dormant Dachora echoes became visible with zero lifetime.");
        }

        var oam = new OamBuffer();
        oam.BeginFrame();
        loaded.Enemies.DrawLayers(
            oam,
            CameraX(room, loaded.Enemies.Slots[0]),
            CameraY(room, loaded.Enemies.Slots[0]),
            0,
            7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException("Dachora's live ROM spritemap emitted no OBJ pieces.");
    }

    private static void VerifyCompleteRetailLifecycle(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedDachora loaded = Load(bus, room, assets, retainedRecordCount: 5);
        RoomEnemySlot body = loaded.Enemies.Slots[0];
        DachoraEnemyState state = RequireState(loaded.Enemies, body);
        int frame = 0;

        // Keep Samus outside both activation axes long enough to observe one complete
        // 32-frame-per-cel idle loop. This proves the retail idle spritemaps through the
        // same interpreter path used by the remainder of the natural lifecycle audit.
        var seenMaps = new HashSet<ushort>();
        loaded.Samus.XPosition = 0x0200;
        loaded.Samus.YPosition = 0;
        for (; frame < 132; frame++)
        {
            Step(loaded, room, assets, frame);
            if (RetailReachableSpritemaps.Contains(body.SpritemapPointer))
                seenMaps.Add(body.SpritemapPointer);
        }

        loaded.Samus.XPosition = body.XPosition;
        loaded.Samus.YPosition = body.YPosition;

        Step(loaded, room, assets, frame++);
        if (state.Function != DachoraAiFunction.BlinkingBeforeRun ||
            state.SpeedOrTimer != 0x78 || loaded.Enemies.LastDachoraSoundEffect != 0x001d)
        {
            throw new InvalidDataException(
                $"Dachora activation failed: function={state.Function}, " +
                $"timer={state.SpeedOrTimer}, sound={loaded.Enemies.LastDachoraSoundEffect:X4}.");
        }

        var seenFunctions = new HashSet<DachoraAiFunction>
        {
            DachoraAiFunction.WaitingForSamus,
            state.Function,
        };
        if (RetailReachableSpritemaps.Contains(body.SpritemapPointer))
            seenMaps.Add(body.SpritemapPointer);
        var seenSpeedPalettes = new HashSet<ushort>();
        var seenShinePalettes = new HashSet<ushort>();
        bool sawSpeedFourList = false;
        bool sawSpeedEightList = false;
        bool sawAllEchoSlots = false;
        bool sawAlternatingEchoVisibility = false;
        ushort highestY = body.YPosition;
        ushort lowestY = body.YPosition;

        for (; frame < 4000; frame++)
        {
            loaded.Samus.XPosition = body.XPosition;
            loaded.Samus.YPosition = body.YPosition;
            Step(loaded, room, assets, frame);
            seenFunctions.Add(state.Function);
            foreach (RoomEnemySlot actor in loaded.Enemies.Slots.Take(5))
            {
                seenFunctions.Add(RequireState(loaded.Enemies, actor).Function);
                if (RetailReachableSpritemaps.Contains(actor.SpritemapPointer))
                    seenMaps.Add(actor.SpritemapPointer);
            }

            if (state.SpeedOrTimer == 4 && state.Subspeed == 0 &&
                body.CurrentInstruction is >= 0xf423 and <= 0xf43f)
            {
                sawSpeedFourList = true;
            }
            if (state.SpeedOrTimer == 8 && state.Subspeed == 0 &&
                body.CurrentInstruction is >= 0xf43f and <= 0xf45b)
            {
                sawSpeedEightList = true;
            }

            foreach (ushort palette in SpeedPalettes)
            {
                if (PaletteMatches(loaded.Cgram, body, bus, palette))
                    seenSpeedPalettes.Add(palette);
            }
            foreach (ushort palette in ShinePalettes)
            {
                if (PaletteMatches(loaded.Cgram, body, bus, palette))
                    seenShinePalettes.Add(palette);
            }

            RoomEnemySlot[] echoes = loaded.Enemies.Slots.Skip(1).Take(4).ToArray();
            sawAllEchoSlots |= echoes.All(echo =>
                RequireState(loaded.Enemies, echo).VerticalSubaccelerationOrLifetime != 0);
            if (echoes.Any(echo => !echo.Properties.HasAny(EnemyProperties.Invisible)) &&
                echoes.Any(echo => echo.Properties.HasAny(EnemyProperties.Invisible)))
            {
                sawAlternatingEchoVisibility = true;
            }

            highestY = Math.Min(highestY, body.YPosition);
            lowestY = Math.Max(lowestY, body.YPosition);

            // One complete cycle is proven after ceiling impact, landing, left traversal,
            // and the hard-coded X=$60 reversal back into the right-running state.
            bool completedCycle = seenFunctions.Contains(DachoraAiFunction.Falling) &&
                state.Function == DachoraAiFunction.RunningRight &&
                body.XPosition <= 0x0068;
            if (completedCycle && seenMaps.SetEquals(RetailReachableSpritemaps) &&
                seenSpeedPalettes.Count == 4 && seenShinePalettes.Count == 4 &&
                sawAllEchoSlots && sawAlternatingEchoVisibility)
            {
                break;
            }
        }

        DachoraAiFunction[] missingFunctions = Enum.GetValues<DachoraAiFunction>()
            .Where(function => !seenFunctions.Contains(function))
            .ToArray();
        if (missingFunctions.Length != 0 || !seenMaps.SetEquals(RetailReachableSpritemaps) ||
            seenSpeedPalettes.Count != 4 || seenShinePalettes.Count != 4 ||
            !sawSpeedFourList || !sawSpeedEightList || !sawAllEchoSlots ||
            !sawAlternatingEchoVisibility || highestY >= 0x0600 || lowestY < 0x0600)
        {
            throw new InvalidDataException(
                "Dachora lifecycle coverage failed: missingFunctions=" +
                $"[{string.Join(',', missingFunctions)}], maps=[{FormatWords(seenMaps)}], " +
                $"missingMaps=[{FormatWords(RetailReachableSpritemaps.Except(seenMaps))}], " +
                $"speedPalettes={seenSpeedPalettes.Count}/4, shinePalettes={seenShinePalettes.Count}/4, " +
                $"speed4/8={sawSpeedFourList}/{sawSpeedEightList}, " +
                $"echoes={sawAllEchoSlots}/{sawAlternatingEchoVisibility}, " +
                $"Y=${highestY:X4}..${lowestY:X4}, final={state.Function} " +
                $"at (${body.XPosition:X4},${body.YPosition:X4}).");
        }
    }

    private static void VerifyAuthoredNonCombatBehavior(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedDachora loaded = Load(bus, room, assets, retainedRecordCount: 1);
        RoomEnemySlot body = loaded.Enemies.Slots[0];
        ushort health = body.Health;
        ushort samusHealth = loaded.Samus.Health;
        if (loaded.Enemies.ResolveOrdinarySamusContact(loaded.Samus, body.NativeIndex) ||
            loaded.Samus.Health != samusHealth)
        {
            throw new InvalidDataException("Dachora's no-touch callback damaged Samus.");
        }

        var projectiles = new SamusProjectileSystem();
        var sharedProjectiles = new SamusBombProjectileSystem();
        ArmProjectile(projectiles.Slots[0], body);
        int shots = loaded.Enemies.ResolveOrdinaryProjectileHits(
            bus,
            projectiles,
            sharedProjectiles,
            loaded.Samus);
        int powerBombs = loaded.Enemies.ResolveOrdinaryPowerBombHits(
            bus,
            body.XPosition,
            body.YPosition,
            explosionRadius: 32);
        GrappleEnemyCollision grapple = loaded.Enemies.ResolveGrappleEndpoint(
            body.XPosition,
            body.YPosition);
        if (shots != 0 || powerBombs != 1 || grapple.Collided || body.Health != health)
        {
            throw new InvalidDataException(
                $"Dachora no-combat behavior failed: shots={shots}, powerBombs={powerBombs}, " +
                $"grapple={grapple}, health={health}->{body.Health}.");
        }
    }

    private static LoadedDachora Load(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        int retainedRecordCount)
    {
        var prefixBus = new PopulationPrefixAddressSpace(
            bus,
            FirstDachoraRecord,
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
            Pose = SamusPoseIds.FacingRightNormalPose,
        };
        samus.RefreshCollisionRadii(prefixBus);
        samus.InitializeAnimation(prefixBus);
        samus.Kinematics.YSubacceleration = ReadWord(bus, 0x909ea1);
        samus.Kinematics.YAcceleration = ReadWord(bus, 0x909ea7);

        var enemies = new RoomEnemySystem();
        enemies.Load(
            prefixBus,
            FirstDachoraRecord,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            random.NextRandom,
            random.SetRandomNumber,
            readRandomNumber: () => random.RandomNumber,
            level: assets.LevelData,
            samus: samus);
        return new LoadedDachora(enemies, samus, cgram);
    }

    private static void Step(
        LoadedDachora loaded,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        int frame)
    {
        RoomEnemySlot body = loaded.Enemies.Slots[0];
        loaded.Enemies.StepFrame(
            CameraX(room, body),
            CameraY(room, body),
            false,
            loaded.Samus,
            level: assets.LevelData,
            nmiFrameCounter8: unchecked((byte)frame));
    }

    private static bool PaletteMatches(
        SnesCgram cgram,
        RoomEnemySlot body,
        ISnesAddressSpace bus,
        ushort sourcePointer)
    {
        int destination = 128 + ((body.PaletteIndex >> 9) & 7) * 16;
        for (int color = 0; color < 16; color++)
        {
            if (cgram.Colors[destination + color] !=
                ReadWord(bus, 0xa70000 | unchecked((ushort)(sourcePointer + color * 2))))
            {
                return false;
            }
        }
        return true;
    }

    private static ushort CameraX(CartridgeRoomHeader room, RoomEnemySlot actor) =>
        unchecked((ushort)Math.Clamp(
            actor.XPosition - 128,
            0,
            Math.Max(0, room.WidthInScreens * 256 - 256)));

    private static ushort CameraY(CartridgeRoomHeader room, RoomEnemySlot actor) =>
        unchecked((ushort)Math.Clamp(
            actor.YPosition - 112,
            0,
            Math.Max(0, room.HeightInScreens * 256 - 224)));

    private static DachoraEnemyState RequireState(
        RoomEnemySystem enemies,
        RoomEnemySlot actor) =>
        enemies.DachoraStates[actor.SlotIndex] ?? throw new InvalidDataException(
            $"Dachora slot {actor.SlotIndex} has no typed state.");

    private static void ArmProjectile(SamusProjectileSlot projectile, RoomEnemySlot target)
    {
        projectile.ClearFields();
        projectile.Type = 0x0100;
        projectile.Damage = 100;
        projectile.Direction = 2;
        projectile.XPosition = target.XPosition;
        projectile.YPosition = target.YPosition;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private static string FormatWords(IEnumerable<ushort> words) =>
        string.Join(',', words.Order().Select(word => $"${word:X4}"));

    private readonly record struct DachoraRecord(
        ushort X,
        ushort Y,
        ushort InitializationParameter,
        ushort Properties,
        ushort ExtraProperties,
        ushort Parameter1,
        ushort Parameter2);

    private sealed record LoadedDachora(
        RoomEnemySystem Enemies,
        SamusState Samus,
        SnesCgram Cgram);
}
