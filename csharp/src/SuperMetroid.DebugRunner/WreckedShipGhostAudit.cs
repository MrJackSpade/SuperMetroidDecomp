using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>
/// ROM-backed behavioral audit for Wrecked Ship ghost definition $A0:E77F. It validates all
/// five authored slot-zero populations, every animation/palette table used by bank $A8, the
/// complete appear/bob/disappear cycle, OBJ output, touch damage, and ordinary shot death.
/// </summary>
internal static class WreckedShipGhostAudit
{
    private const ushort DefinitionPointer = 0xe77f;
    private const ushort AuditRoomPointer = 0xcccb;
    private const ushort AuditStatePointer = 0xccdd;
    private const ushort AuditPopulationPointer = 0xc1ed;

    private static readonly GhostPopulation[] RetailPopulations =
    [
        new(0xca52, 0xca64, 0xc6f2, 0x02d7, 0x0098),
        new(0xcaf6, 0xcb08, 0xbca0, 0x0038, 0x03d8),
        new(0xcccb, 0xccdd, 0xc1ed, 0x0088, 0x0088),
        new(0xcdf1, 0xce03, 0xc8c5, 0x0088, 0x0088),
        new(0xce8a, 0xce9c, 0xca78, 0x0088, 0x0088),
    ];

    private static readonly ushort[] InstructionWords =
    [
        0x0010, 0x9e46,
        0x0010, 0x9e5c,
        0x0010, 0x9e72,
        0x80ed, 0x9a8c,
    ];

    private static readonly ushort[] SpawnOffsetWords =
    [
        0xffc0, 0xffc0, 0x0000, 0xffc0, 0x0040, 0x0000,
        0xffc0, 0x0000, 0x0000, 0x0000, 0x0040, 0x0000,
        0xffc0, 0x0040, 0x0000, 0x0040, 0x0040, 0x0040,
    ];

    private static readonly ushort[] FlickerDurationWords =
    [
        1, 8, 1, 8, 1, 7, 1, 7, 2, 6, 2, 6, 3, 5, 3, 5, 0xffff,
    ];

    private static readonly ushort[] GhostPalette =
    [
        0x3800, 0x57ff, 0x42f7, 0x0929,
        0x00a5, 0x4f5a, 0x36b5, 0x2610,
        0x1dce, 0x01df, 0x001f, 0x0018,
        0x000a, 0x06b9, 0x00ea, 0x0045,
    ];

    private static readonly ushort[] AnimationMaps = [0x9e46, 0x9e5c, 0x9e72];

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        VerifyDefinition(bus);
        VerifyRetailPopulations(bus);
        VerifyRomTables(bus);

        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, AuditRoomPointer);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        VerifyLifecycleAnimationAndPalettes(bus, room, assets);
        VerifyDirectionalSpawnSelection(bus, room, assets);
        VerifyTouchAttackAndShotDamage(bus, room, assets);

        Console.WriteLine(
            "Wrecked Ship ghost audit passed: all five retail slot-zero populations, " +
            "three ROM animation maps, movement-triggered spawn selection, flicker/white/" +
            "ghost palette transitions, 120-frame bob, reappearance delay, 60 contact " +
            "damage, and ordinary beam hurt/death behavior were verified.");
        return 0;
    }

    private static void VerifyDirectionalSpawnSelection(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        VerifyDirectionalSpawn(
            LoadGhost(bus, room, assets),
            assets,
            deltaX: -2,
            deltaY: -2,
            expectedHorizontalClass: 0,
            expectedVerticalClass: 0,
            expectedXOffset: -64,
            expectedYOffset: -64);
        VerifyDirectionalSpawn(
            LoadGhost(bus, room, assets),
            assets,
            deltaX: 2,
            deltaY: 2,
            expectedHorizontalClass: 8,
            expectedVerticalClass: 24,
            expectedXOffset: 64,
            expectedYOffset: 64);
    }

    private static void VerifyDirectionalSpawn(
        LoadedGhost loaded,
        CartridgeRoomAssets assets,
        int deltaX,
        int deltaY,
        ushort expectedHorizontalClass,
        ushort expectedVerticalClass,
        int expectedXOffset,
        int expectedYOffset)
    {
        WreckedShipGhostEnemyState state = State(loaded.Enemies, loaded.Actor);
        while (state.Function == WreckedShipGhostAiFunction.InitialInvisibleDelay)
            Step(loaded, assets);

        // Establish an ordinary previous-position sample before asking the direction timer
        // to observe continuous motion. This prevents the native zero-filled extension from
        // making the first positive world coordinate look like an authored movement sample.
        loaded.Samus.XPosition = 0x0180;
        loaded.Samus.YPosition = 0x0180;
        Step(loaded, assets);

        int movingFrames = 0;
        while (state.Function == WreckedShipGhostAiFunction.TrackingSamusForSpawn &&
               movingFrames < 32)
        {
            loaded.Samus.XPosition = unchecked((ushort)(loaded.Samus.XPosition + deltaX));
            loaded.Samus.YPosition = unchecked((ushort)(loaded.Samus.YPosition + deltaY));
            Step(loaded, assets);
            movingFrames++;
        }

        ushort expectedX = unchecked((ushort)(loaded.Samus.XPosition + expectedXOffset));
        ushort expectedY = unchecked((ushort)(loaded.Samus.YPosition + expectedYOffset));
        if (state.Function != WreckedShipGhostAiFunction.BrighteningAndFlickering ||
            state.HorizontalMovementClass != expectedHorizontalClass ||
            state.VerticalMovementClass != expectedVerticalClass ||
            loaded.Actor.XPosition != expectedX || loaded.Actor.YPosition != expectedY)
        {
            throw new InvalidDataException(
                $"Ghost directional spawn ({deltaX},{deltaY}) mismatch after {movingFrames} " +
                $"frames: classes={state.HorizontalMovementClass}/" +
                $"{state.VerticalMovementClass}, actor=({loaded.Actor.XPosition}," +
                $"{loaded.Actor.YPosition}), expected=({expectedX},{expectedY}).");
        }
    }

    private static void VerifyDefinition(SuperMetroidAddressSpace bus)
    {
        RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(bus, DefinitionPointer);
        if (definition.TileDataSize != 0x0400 || definition.PalettePointer != 0x99ac ||
            definition.Health != 300 || definition.Damage != 60 ||
            definition.XRadius != 16 || definition.YRadius != 16 ||
            definition.Bank != 0xa8 || definition.HurtAiTime != 0 ||
            definition.HurtSoundEffect != 0x0051 || definition.BossId != 0 ||
            definition.InitializationAiPointer != 0x9aee || definition.PartCount != 1 ||
            definition.MainAiPointer != 0x9b3c || definition.GrappleAiPointer != 0x8000 ||
            definition.HurtAiPointer != 0x804c || definition.FrozenAiPointer != 0x8041 ||
            definition.TimeFrozenAiPointer != 0 || definition.DeathAnimation != 2 ||
            definition.PowerBombReactionPointer != 0 || definition.VariantIndex != 0 ||
            definition.TouchAiPointer != 0x8023 || definition.ShotAiPointer != 0x802d ||
            definition.InitialSpritemapPointer != 0 || definition.TileDataAddress != 0xb1a600 ||
            definition.Layer != 5 || definition.ItemDropChancesPointer != 0xf266 ||
            definition.VulnerabilityPointer != 0xec1c || definition.NamePointer != 0xde5b)
        {
            throw new InvalidDataException(
                "Wrecked Ship ghost header disagrees with retail definition $A0:E77F.");
        }

        // Power beam vulnerability two combines with the common handler's half-damage
        // pre-scale, leaving an ordinary 20-damage shot at exactly 20 actor health.
        if (bus.ReadByte(0xb4ec1c) != 2)
            throw new InvalidDataException("Ghost power-beam vulnerability is not retail value two.");
    }

    private static void VerifyRetailPopulations(ISnesAddressSpace bus)
    {
        foreach (GhostPopulation expected in RetailPopulations)
        {
            CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, expected.RoomPointer);
            if (room.State.Pointer != expected.StatePointer ||
                room.State.EnemyPopulationPointer != expected.PopulationPointer)
            {
                throw new InvalidDataException(
                    $"Ghost room $8F:{expected.RoomPointer:X4} selected state/population " +
                    $"${room.State.Pointer:X4}/${room.State.EnemyPopulationPointer:X4}.");
            }

            int record = 0xa10000 | expected.PopulationPointer;
            ushort[] actual = Enumerable.Range(0, 8)
                .Select(index => ReadWord(bus, record + index * 2))
                .ToArray();
            ushort[] authored =
            [
                DefinitionPointer, expected.X, expected.Y, 0,
                0x6800, 0, 0, 0,
            ];
            if (!actual.SequenceEqual(authored))
            {
                throw new InvalidDataException(
                    $"Population $A1:{expected.PopulationPointer:X4} lost its exact ghost record.");
            }
        }
    }

    private static void VerifyRomTables(ISnesAddressSpace bus)
    {
        VerifyWords(bus, 0xa89a8c, InstructionWords, "animation list");
        VerifyWords(bus, 0xa89aa8, SpawnOffsetWords, "spawn-offset table");
        VerifyWords(bus, 0xa89acc, FlickerDurationWords, "flicker-duration table");
        VerifyWords(bus, 0xa899ac, GhostPalette, "palette");

        ushort[] constants = [0x0010, 0x0040, 0x1800, 0x0001, 0x0078, 0x0078];
        VerifyWords(bus, 0xa89a9c, constants, "timing/movement constants");
        foreach (ushort map in AnimationMaps)
        {
            ushort pieceCount = ReadWord(bus, 0xa80000 | map);
            if (pieceCount is 0 or > 8)
                throw new InvalidDataException($"Ghost map $A8:{map:X4} has {pieceCount} pieces.");
        }
    }

    private static void VerifyLifecycleAnimationAndPalettes(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedGhost loaded = LoadGhost(bus, room, assets);
        RoomEnemySlot actor = loaded.Actor;
        WreckedShipGhostEnemyState state = State(loaded.Enemies, actor);
        int paletteStart = PaletteStart(actor);

        if (loaded.Enemies.EnemyCount != 9 || actor.SlotIndex != 0 ||
            actor.XPosition != 0x0088 || actor.YPosition != 0x0088 ||
            actor.Properties != 0x6d00 || actor.CurrentInstruction != 0x9a8c ||
            actor.InstructionTimer != 1 || actor.Timer != 0 ||
            state.Function != WreckedShipGhostAiFunction.InitialInvisibleDelay ||
            state.PhaseTimer != 0x0118 || state.TargetPalette.Span.ToArray().Any(word => word != 0) ||
            loaded.Cgram.Colors.Slice(paletteStart, 16).ToArray().Any(word => word != 0))
        {
            throw new InvalidDataException(
                $"Ghost initialization mismatch: slot={actor.SlotIndex}, properties=" +
                $"${actor.Properties:X4}, list=$A8:{actor.CurrentInstruction:X4}, " +
                $"function={state.Function}, timer={state.PhaseTimer}.");
        }

        var observedMaps = new HashSet<ushort>();
        int initialDelayFrames = 0;
        while (state.Function == WreckedShipGhostAiFunction.InitialInvisibleDelay &&
               initialDelayFrames < 300)
        {
            Step(loaded, assets);
            initialDelayFrames++;
            if (actor.SpritemapPointer != 0)
                observedMaps.Add(actor.SpritemapPointer);
        }
        if (initialDelayFrames != 280 ||
            state.Function != WreckedShipGhostAiFunction.TrackingSamusForSpawn ||
            state.PhaseTimer != 1 || state.FlickerTableOffset != 2 ||
            !AnimationMaps.All(observedMaps.Contains))
        {
            throw new InvalidDataException(
                $"Ghost initial wait/animation mismatch: frames={initialDelayFrames}, " +
                $"function={state.Function}, phase/flicker={state.PhaseTimer}/" +
                $"{state.FlickerTableOffset}, maps={FormatWords(observedMaps)}.");
        }

        // A stationary Samus first initializes the previous-position box, then consumes the
        // exact 64-frame stillness timer. Retail forces stationary classes 4/12 and therefore
        // materializes the ghost directly on Samus rather than choosing a directional offset.
        int trackingFrames = 0;
        while (state.Function == WreckedShipGhostAiFunction.TrackingSamusForSpawn &&
               trackingFrames < 80)
        {
            Step(loaded, assets);
            trackingFrames++;
        }
        if (trackingFrames != 65 ||
            state.Function != WreckedShipGhostAiFunction.BrighteningAndFlickering ||
            state.HorizontalMovementClass != 4 || state.VerticalMovementClass != 12 ||
            actor.XPosition != loaded.Samus.XPosition || actor.YPosition != loaded.Samus.YPosition)
        {
            throw new InvalidDataException(
                $"Ghost stationary spawn mismatch: frames={trackingFrames}, function=" +
                $"{state.Function}, classes={state.HorizontalMovementClass}/" +
                $"{state.VerticalMovementClass}, actor=({actor.XPosition},{actor.YPosition}), " +
                $"Samus=({loaded.Samus.XPosition},{loaded.Samus.YPosition}).");
        }

        bool sawHiddenFlicker = actor.Properties.HasAny(EnemyProperties.Invisible);
        bool sawVisibleFlicker = false;
        int brighteningFrames = 0;
        while (state.Function == WreckedShipGhostAiFunction.BrighteningAndFlickering &&
               brighteningFrames < 48)
        {
            Step(loaded, assets);
            brighteningFrames++;
            sawHiddenFlicker |= actor.Properties.HasAny(EnemyProperties.Invisible);
            sawVisibleFlicker |= !actor.Properties.HasAny(EnemyProperties.Invisible);
        }
        ushort[] whitePalette = Enumerable.Repeat((ushort)0x7fff, 16).ToArray();
        if (brighteningFrames != 32 ||
            state.Function != WreckedShipGhostAiFunction.FadingToGhostPalette ||
            !loaded.Cgram.Colors.Slice(paletteStart, 16).SequenceEqual(whitePalette) ||
            !state.TargetPalette.Span.SequenceEqual(GhostPalette) ||
            !sawHiddenFlicker || !sawVisibleFlicker)
        {
            throw new InvalidDataException(
                $"Ghost white-ramp/flicker mismatch: frames={brighteningFrames}, " +
                $"function={state.Function}, hidden/visible={sawHiddenFlicker}/{sawVisibleFlicker}.");
        }

        int materializationFrames = 0;
        while (state.Function == WreckedShipGhostAiFunction.FadingToGhostPalette &&
               materializationFrames < 80)
        {
            Step(loaded, assets);
            materializationFrames++;
        }
        if (state.Function != WreckedShipGhostAiFunction.BobbingWhileVisible ||
            actor.Properties != 0x6800 || state.PhaseTimer != 0x0078 ||
            state.BobbingOriginY != actor.YPosition ||
            state.VerticalVelocityWhole != 1 || state.VerticalVelocityFraction != 0 ||
            !loaded.Cgram.Colors.Slice(paletteStart, 16).SequenceEqual(GhostPalette))
        {
            throw new InvalidDataException(
                $"Ghost materialization mismatch after {materializationFrames} frames: " +
                $"function={state.Function}, properties=${actor.Properties:X4}, " +
                $"timer={state.PhaseTimer}, velocity=${state.VerticalVelocityWhole:X4}." +
                $"{state.VerticalVelocityFraction:X4}.");
        }

        // Draw each authored map through the ordinary room queue. The tiles, palette, and
        // spritemap records were all loaded from this same untouched room and cartridge.
        var oam = new OamBuffer();
        foreach (ushort map in AnimationMaps)
        {
            actor.SpritemapPointer = map;
            oam.BeginFrame();
            loaded.Enemies.DrawLayers(oam, 0, 0, 0, 7);
            oam.FinalizeFrame();
            if (oam.LastFinalizedSpriteCount == 0)
                throw new InvalidDataException($"Ghost map $A8:{map:X4} emitted no OBJ pieces.");
        }

        ushort bobbingOrigin = state.BobbingOriginY;
        var bobbingYPositions = new HashSet<ushort>();
        int visibleFrames = 0;
        while (state.Function == WreckedShipGhostAiFunction.BobbingWhileVisible &&
               visibleFrames < 140)
        {
            Step(loaded, assets);
            visibleFrames++;
            bobbingYPositions.Add(actor.YPosition);
        }
        if (visibleFrames != 120 ||
            state.Function != WreckedShipGhostAiFunction.WaitingForWhiteFade ||
            !actor.Properties.HasAny(EnemyProperties.IgnoreSamusCollision) ||
            state.TargetPalette.Span.ToArray().Any(word => word != 0x7fff) ||
            bobbingYPositions.Count < 8 ||
            !bobbingYPositions.Any(y => unchecked((short)(y - bobbingOrigin)) < 0) ||
            !bobbingYPositions.Any(y => unchecked((short)(y - bobbingOrigin)) > 0))
        {
            throw new InvalidDataException(
                $"Ghost bob mismatch: frames={visibleFrames}, function={state.Function}, " +
                $"positions={bobbingYPositions.Count}, properties=${actor.Properties:X4}.");
        }

        int whiteningFrames = 0;
        while (state.Function == WreckedShipGhostAiFunction.WaitingForWhiteFade &&
               whiteningFrames < 48)
        {
            Step(loaded, assets);
            whiteningFrames++;
        }
        if (whiteningFrames != 32 ||
            state.Function != WreckedShipGhostAiFunction.InitialInvisibleDelay ||
            state.PhaseTimer != 0x0078 ||
            !actor.Properties.HasAll(
                EnemyProperties.Invisible | EnemyProperties.IgnoreSamusCollision) ||
            !loaded.Cgram.Colors.Slice(paletteStart, 16).SequenceEqual(whitePalette))
        {
            throw new InvalidDataException(
                $"Ghost disappearance mismatch: frames={whiteningFrames}, function=" +
                $"{state.Function}, timer={state.PhaseTimer}, properties=${actor.Properties:X4}.");
        }
    }

    private static void VerifyTouchAttackAndShotDamage(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedGhost loaded = LoadGhost(bus, room, assets);
        RoomEnemySlot actor = loaded.Actor;
        WreckedShipGhostEnemyState state = State(loaded.Enemies, actor);
        while (state.Function != WreckedShipGhostAiFunction.BobbingWhileVisible)
            Step(loaded, assets);

        // The active/interactive arrays are built before actor AI. Advance once after the
        // state clears property $0400 so the next collision pass observes the native list.
        Step(loaded, assets);
        loaded.Samus.XPosition = actor.XPosition;
        loaded.Samus.YPosition = actor.YPosition;
        loaded.Samus.Health = 999;
        loaded.Samus.InvincibilityTimer = 0;
        if (!loaded.Enemies.ResolveOrdinarySamusContact(loaded.Samus, 0) ||
            loaded.Samus.Health != 939 || !loaded.Samus.KnockbackActive)
        {
            throw new InvalidDataException(
                $"Ghost touch attack mismatch: health={loaded.Samus.Health}, " +
                $"knockback={loaded.Samus.KnockbackActive}.");
        }

        var shots = new SamusProjectileSystem();
        var bombs = new SamusBombProjectileSystem();
        ArmBeam(shots.Slots[0], actor, damage: 20);
        if (loaded.Enemies.ResolveOrdinaryProjectileHits(
                bus, shots, bombs, loaded.Samus) != 1 ||
            actor.Health != 280 || actor.FlashTimer != 12 ||
            (actor.AiHandlerBits & 2) == 0)
        {
            throw new InvalidDataException(
                $"Ghost nonlethal shot mismatch: health={actor.Health}, flash=" +
                $"{actor.FlashTimer}, AI bits=${actor.AiHandlerBits:X4}.");
        }

        ArmBeam(shots.Slots[0], actor, damage: 300);
        if (loaded.Enemies.ResolveOrdinaryProjectileHits(
                bus, shots, bombs, loaded.Samus) != 1 ||
            actor.Health != 0 ||
            !actor.Properties.HasAny(EnemyProperties.Deleted) ||
            loaded.Enemies.EnemiesKilled != 1)
        {
            throw new InvalidDataException(
                $"Ghost lethal shot mismatch: health={actor.Health}, properties=" +
                $"${actor.Properties:X4}, killed={loaded.Enemies.EnemiesKilled}.");
        }
    }

    private static LoadedGhost LoadGhost(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        if (room.State.Pointer != AuditStatePointer ||
            room.State.EnemyPopulationPointer != AuditPopulationPointer)
        {
            throw new InvalidDataException(
                $"Ghost audit room selected ${room.State.Pointer:X4}/" +
                $"${room.State.EnemyPopulationPointer:X4}.");
        }

        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState();
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusState.FacingRightNormalPose,
            XPosition = 0x0080,
            YPosition = 0x0080,
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
            samus: samus);
        RoomEnemySlot actor = enemies.Slots[0];
        if (actor.EnemyDefinitionPointer != DefinitionPointer)
            throw new InvalidDataException("Ghost audit population did not load $E77F in slot zero.");

        // This room also contains eight Sbugs. Their translation has a separate focused
        // audit; delete them here so their motion/contact cannot obscure the ghost contract.
        foreach (RoomEnemySlot other in enemies.Slots.Take(enemies.EnemyCount).Skip(1))
            other.Properties = other.Properties.With(EnemyProperties.Deleted);

        return new LoadedGhost(enemies, actor, samus, cgram);
    }

    private static void Step(LoadedGhost loaded, CartridgeRoomAssets assets) =>
        loaded.Enemies.StepFrame(
            cameraX: 0,
            cameraY: 0,
            timeIsFrozen: false,
            loaded.Samus,
            level: assets.LevelData);

    private static void ArmBeam(
        SamusProjectileSlot projectile,
        RoomEnemySlot actor,
        ushort damage)
    {
        projectile.ClearFields();
        projectile.Type = 0;
        projectile.Damage = damage;
        projectile.Direction = (ushort)SamusProjectileDirection.Right;
        projectile.XPosition = actor.XPosition;
        projectile.YPosition = actor.YPosition;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    private static WreckedShipGhostEnemyState State(
        RoomEnemySystem enemies,
        RoomEnemySlot actor) =>
        enemies.WreckedShipGhostStates[actor.SlotIndex] ?? throw new InvalidDataException(
            $"Ghost slot {actor.SlotIndex} has no typed state.");

    private static int PaletteStart(RoomEnemySlot actor) =>
        128 + ((actor.PaletteIndex >> 9) & 7) * 16;

    private static void VerifyWords(
        ISnesAddressSpace bus,
        int address,
        ushort[] expected,
        string tableName)
    {
        for (int index = 0; index < expected.Length; index++)
        {
            ushort actual = ReadWord(bus, address + index * 2);
            if (actual != expected[index])
            {
                throw new InvalidDataException(
                    $"Ghost {tableName} word {index} is ${actual:X4}, " +
                    $"expected ${expected[index]:X4}.");
            }
        }
    }

    private static string FormatWords(IEnumerable<ushort> words) =>
        string.Join(',', words.Select(word => $"${word:X4}"));

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private readonly record struct GhostPopulation(
        ushort RoomPointer,
        ushort StatePointer,
        ushort PopulationPointer,
        ushort X,
        ushort Y);

    private readonly record struct LoadedGhost(
        RoomEnemySystem Enemies,
        RoomEnemySlot Actor,
        SamusState Samus,
        SnesCgram Cgram);
}
