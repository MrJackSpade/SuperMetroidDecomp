using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>
/// ROM-backed audit for the paired morph-ball eye mount/body and its room-global HDMA beam.
/// The assertions intentionally cover all three retail populations because their parameter
/// differences select both body facings and all four possible mount orientations.
/// </summary>
internal static class MorphBallEyeAudit
{
    private const ushort Definition = 0xe6bf;
    private const ushort FinalMissileRoom = 0x9a90;
    private const ushort FinalMissileState = 0x9aa2;
    private const ushort FinalMissilePopulation = 0x8586;
    private const ushort MorphBallRoom = 0x9e9f;
    private const ushort MorphBallState = 0x9eb1;
    private const ushort MorphBallPopulation = 0x93ac;
    private const ushort BlueBrinstarETankRoom = 0x9f64;
    private const ushort BlueBrinstarETankState = 0x9f76;
    private const ushort BlueBrinstarETankPopulation = 0x966f;

    private static readonly EyeRecord[] FinalMissileRecords =
    [
        new(Definition, 0x0032, 0x0078, 0, 0x2c00, 0, 0, 0x8000),
        new(Definition, 0x0032, 0x0078, 0, 0x2c00, 0, 0, 0x0000),
    ];

    private static readonly EyeRecord[] MorphBallRecords =
    [
        new(Definition, 0x0408, 0x0248, 0, 0x2c00, 0, 0, 0x8000),
        new(Definition, 0x0408, 0x0248, 0, 0x2c00, 0, 1, 0x0000),
    ];

    private static readonly EyeRecord[] BlueBrinstarETankRecords =
    [
        new(Definition, 0x0228, 0x0268, 0, 0x2c00, 0, 0, 0x8001),
        new(Definition, 0x0228, 0x0268, 0, 0x2c00, 0, 0, 0x0000),
    ];

    // Every map reachable from $A8:8FAC-$904E: fourteen directional eye frames, four
    // closed/transition composites, and four two-piece mounts.
    private static readonly HashSet<ushort> ReferencedSpritemaps =
    [
        0x91df, 0x91e6, 0x91ed, 0x91f4, 0x91fb, 0x9202, 0x9209,
        0x9210, 0x9217, 0x921e, 0x9225, 0x922c, 0x9233, 0x923a,
        0x9241, 0x9257, 0x926d, 0x9283,
        0x9299, 0x92a5, 0x92b1, 0x92bd,
    ];

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        VerifyDefinition(bus);
        VerifyRoomPopulation(bus, FinalMissileRoom, FinalMissileState,
            FinalMissilePopulation, FinalMissileRecords);
        VerifyRoomPopulation(bus, MorphBallRoom, MorphBallState,
            MorphBallPopulation, MorphBallRecords);
        VerifyRoomPopulation(bus, BlueBrinstarETankRoom, BlueBrinstarETankState,
            BlueBrinstarETankPopulation, BlueBrinstarETankRecords);
        VerifyInstructionListsAndSpritemaps(bus);
        VerifyCollectedItemPersistence();

        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, FinalMissileRoom);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        VerifyInitializationAndOwnershipGate(bus, room, assets);
        VerifyActivationTrackingBeamAndDeactivation(bus, room, assets);
        VerifyIntentionalNoCombat(bus, room, assets);

        Console.WriteLine(
            "Morph-ball eye audit passed: all three retail mount/body populations, all 22 " +
            "ROM spritemaps, collected-item gating, strict proximity boundaries, activation/" +
            "tracking/deactivation animation, bank-$88 widening/color/fade lifetime, OBJ " +
            "drawing, and intentional indestructible/no-damage behavior were verified.");
        return 0;
    }

    private static void VerifyDefinition(ISnesAddressSpace bus)
    {
        RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(bus, Definition);
        if (definition.TileDataSize != 0x0400 || definition.PalettePointer != 0x8f8c ||
            definition.Health != 20 || definition.Damage != 0 ||
            definition.XRadius != 8 || definition.YRadius != 8 ||
            definition.Bank != 0xa8 || definition.HurtAiTime != 0 || definition.BossId != 0 ||
            definition.InitializationAiPointer != 0x9058 || definition.PartCount != 2 ||
            definition.MainAiPointer != 0x90e2 || definition.GrappleAiPointer != 0x800f ||
            definition.HurtAiPointer != 0x804c || definition.FrozenAiPointer != 0x8041 ||
            definition.TimeFrozenAiPointer != 0 || definition.DeathAnimation != 0 ||
            definition.PowerBombReactionPointer != 0 || definition.TouchAiPointer != 0x8023 ||
            definition.ShotAiPointer != 0x804c || definition.InitialSpritemapPointer != 0 ||
            definition.Layer != 5 || definition.NamePointer != 0xde31)
        {
            throw new InvalidDataException(
                "Morph-ball eye $E6BF header disagrees with the translated bank-$A8 family: " +
                $"size/palette=${definition.TileDataSize:X4}/${definition.PalettePointer:X4}, " +
                $"hp/damage={definition.Health}/{definition.Damage}, " +
                $"radius={definition.XRadius}/{definition.YRadius}, " +
                $"bank/init/main=${definition.Bank:X2}:${definition.InitializationAiPointer:X4}/" +
                $"${definition.MainAiPointer:X4}, parts={definition.PartCount}, " +
                $"grapple/hurt/frozen=${definition.GrappleAiPointer:X4}/" +
                $"${definition.HurtAiPointer:X4}/${definition.FrozenAiPointer:X4}, " +
                $"touch/shot/name=${definition.TouchAiPointer:X4}/" +
                $"${definition.ShotAiPointer:X4}/${definition.NamePointer:X4}.");
        }

        // The eye is intentionally indestructible. Confirm the vulnerability table's first
        // sixteen projectile-family bytes directly instead of inferring that from HP alone.
        int vulnerability = 0xb40000 | definition.VulnerabilityPointer;
        for (int index = 0; index < 16; index++)
        {
            if (bus.ReadByte(vulnerability + index) != 0)
            {
                throw new InvalidDataException(
                    $"Morph-ball eye vulnerability byte {index} is not retail-indestructible zero.");
            }
        }
    }

    private static void VerifyRoomPopulation(
        ISnesAddressSpace bus,
        ushort roomPointer,
        ushort statePointer,
        ushort populationPointer,
        IReadOnlyList<EyeRecord> expected)
    {
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, roomPointer);
        if (room.State.Pointer != statePointer ||
            room.State.EnemyPopulationPointer != populationPointer)
        {
            throw new InvalidDataException(
                $"Eye room ${roomPointer:X4} selected state/population " +
                $"${room.State.Pointer:X4}/${room.State.EnemyPopulationPointer:X4}.");
        }

        var actual = new List<EyeRecord>();
        int cursor = 0xa10000 | populationPointer;
        for (int index = 0; index < expected.Count; index++, cursor += 16)
        {
            actual.Add(new EyeRecord(
                ReadWord(bus, cursor), ReadWord(bus, cursor + 2),
                ReadWord(bus, cursor + 4), ReadWord(bus, cursor + 6),
                ReadWord(bus, cursor + 8), ReadWord(bus, cursor + 10),
                ReadWord(bus, cursor + 12), ReadWord(bus, cursor + 14)));
        }

        if (!actual.SequenceEqual(expected))
        {
            throw new InvalidDataException(
                $"Population $A1:{populationPointer:X4} does not begin with its exact retail eye pair.");
        }
    }

    private static void VerifyInstructionListsAndSpritemaps(ISnesAddressSpace bus)
    {
        var actualMaps = new HashSet<ushort>();

        // The active list is a sixteen-record table followed by common goto. Duplicate
        // cardinal entries are intentional; the resulting unique set contains fourteen maps.
        for (int index = 0; index < 16; index++)
        {
            int record = 0xa88fac + index * 4;
            if (ReadWord(bus, record) != 0x000a)
                throw new InvalidDataException($"Eye active record {index} lost duration ten.");
            actualMaps.Add(ReadWord(bus, record + 2));
        }
        if (ReadWord(bus, 0xa88fec) != 0x80ed || ReadWord(bus, 0xa88fee) != 0x8fac)
            throw new InvalidDataException("Eye active table lost its common-goto loop.");

        // These are the non-active duration/map records in the seven body lists and four
        // mount lists. Reading each pointer from ROM keeps this audit independent of runtime
        // constants while still proving the complete authored animation surface is covered.
        int[] mapWordAddresses =
        [
            0xa88ff2, 0xa88ff6, 0xa88ffa, 0xa88ffe,
            0xa89004, 0xa89008, 0xa8900c, 0xa89010,
            0xa89016, 0xa8901a, 0xa8901e, 0xa89022,
            0xa89028, 0xa8902c, 0xa89030, 0xa89034,
            0xa8903a, 0xa89040, 0xa89046, 0xa8904c,
        ];
        foreach (int address in mapWordAddresses)
            actualMaps.Add(ReadWord(bus, address));

        if (!actualMaps.SetEquals(ReferencedSpritemaps))
        {
            throw new InvalidDataException(
                "Eye instruction lists reference an unexpected map set: missing=[" +
                FormatWords(ReferencedSpritemaps.Except(actualMaps)) + "], extra=[" +
                FormatWords(actualMaps.Except(ReferencedSpritemaps)) + "].");
        }

        foreach (ushort map in ReferencedSpritemaps)
        {
            ushort pieceCount = ReadWord(bus, 0xa80000 | map);
            if (pieceCount is 0 or > 4)
                throw new InvalidDataException($"Eye map $A8:{map:X4} has invalid piece count {pieceCount}.");
        }
    }

    private static void VerifyCollectedItemPersistence()
    {
        var restored = new SamusState();
        var slot = new SuperMetroidSaveSlot(
            Slot: 0,
            EquippedItems: 0,
            CollectedItems: 0x0004,
            EquippedBeams: 0,
            CollectedBeams: 0,
            ReserveMode: 0,
            Health: 99,
            MaxHealth: 99,
            Missiles: 0,
            MaxMissiles: 0,
            SuperMissiles: 0,
            MaxSuperMissiles: 0,
            PowerBombs: 0,
            MaxPowerBombs: 0,
            HudItem: 0,
            MaxReserveEnergy: 0,
            ReserveEnergy: 0,
            GameTimeFrames: 0,
            GameTimeSeconds: 0,
            GameTimeMinutes: 0,
            GameTimeHours: 0,
            EventBytes: new byte[Bank80SystemState.EventByteCount],
            BossBytes: new byte[Bank80SystemState.AreaCount],
            RoomChozoBytes: new byte[Bank80SystemState.RoomChozoBitByteCount],
            CollectedItemBytes: new byte[Bank80SystemState.ItemBitByteCount],
            SaveStation: 0,
            Area: 0);
        slot.ApplyTo(restored);
        SuperMetroidSaveSnapshot captured = SuperMetroidSaveSnapshot.Capture(
            restored, new Bank80SystemState(), area: 0, saveStation: 0);
        if (restored.EquippedItems != 0 || restored.CollectedItems != 0x0004 ||
            captured.EquippedItems != 0 || captured.CollectedItems != 0x0004)
        {
            throw new InvalidDataException(
                "Save restore/capture collapsed collected items back into equipped items.");
        }
    }

    private static void VerifyInitializationAndOwnershipGate(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedEye loaded = Load(bus, room, assets);
        RoomEnemySlot mount = loaded.Enemies.Slots[0];
        RoomEnemySlot body = loaded.Enemies.Slots[1];
        MorphBallEyeEnemyState mountState = RequireState(loaded.Enemies, mount);
        MorphBallEyeEnemyState bodyState = RequireState(loaded.Enemies, body);

        if (loaded.Enemies.EnemyCount != 2 ||
            mountState.Function != MorphBallEyeAiFunction.MountNoOp ||
            bodyState.Function != MorphBallEyeAiFunction.WaitForSamus ||
            mount.XPosition != FinalMissileRecords[0].X - 8 ||
            mount.YPosition != FinalMissileRecords[0].Y ||
            mount.CurrentInstruction != 0x9044 || body.CurrentInstruction != 0x8ffc ||
            !mount.Properties.HasAny(EnemyProperties.ProcessInstructions) ||
            !body.Properties.HasAny(EnemyProperties.ProcessInstructions) ||
            loaded.Enemies.MorphBallEyeBeam.Phase != MorphBallEyeBeamPhase.Inactive)
        {
            throw new InvalidDataException(
                "Morph-ball eye two-slot initialization did not reproduce mount/body roles.");
        }

        // Samus is inside both activation axes, but neither an equipped-only Morph Ball nor
        // no item at all can satisfy MainAI_Eye's collected_items test.
        loaded.Samus.XPosition = body.XPosition;
        loaded.Samus.YPosition = body.YPosition;
        loaded.Samus.EquippedItems = 0x0004;
        loaded.Samus.CollectedItems = 0;
        Step(loaded, room, assets, frame: 0);
        if (bodyState.Function != MorphBallEyeAiFunction.WaitForSamus)
            throw new InvalidDataException("Equipped-only Morph Ball incorrectly activated the eye.");

        // Equality is outside because both bank-$A0 proximity helpers use strict less-than.
        loaded.Samus.EquippedItems = 0;
        loaded.Samus.CollectedItems = 0x0004;
        loaded.Samus.XPosition = unchecked((ushort)(body.XPosition + 0x0080));
        Step(loaded, room, assets, frame: 1);
        if (bodyState.Function != MorphBallEyeAiFunction.WaitForSamus)
            throw new InvalidDataException("Eye activated at the strict 128-pixel X boundary.");

        loaded.Samus.XPosition = unchecked((ushort)(body.XPosition + 0x007f));
        Step(loaded, room, assets, frame: 2);
        if (bodyState.Function != MorphBallEyeAiFunction.Activating ||
            bodyState.FunctionTimer != 0x20 || body.CurrentInstruction != 0x9018 ||
            body.SpritemapPointer != 0x9241)
        {
            throw new InvalidDataException(
                "Collected-but-unequipped Morph Ball did not start the right-facing activation list.");
        }

        var oam = new OamBuffer();
        foreach (ushort map in ReferencedSpritemaps)
        {
            body.SpritemapPointer = map;
            oam.BeginFrame();
            loaded.Enemies.DrawLayers(oam, CameraX(room, body), CameraY(room, body), 0, 7);
            oam.FinalizeFrame();
            if (oam.LastFinalizedSpriteCount == 0)
                throw new InvalidDataException($"Eye map $A8:{map:X4} emitted no OBJ pieces.");
        }
    }

    private static void VerifyActivationTrackingBeamAndDeactivation(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedEye loaded = Load(bus, room, assets);
        RoomEnemySlot body = loaded.Enemies.Slots[1];
        MorphBallEyeEnemyState state = RequireState(loaded.Enemies, body);
        loaded.Samus.CollectedItems = 0x0004;
        loaded.Samus.XPosition = unchecked((ushort)(body.XPosition + 0x0040));
        loaded.Samus.YPosition = body.YPosition;

        Step(loaded, room, assets, frame: 0);
        for (int frame = 1; frame <= 32; frame++)
            Step(loaded, room, assets, frame);

        if (state.Function != MorphBallEyeAiFunction.Active || state.Angle != 0x0040 ||
            loaded.Enemies.LastMorphBallEyeSoundEffect != 0x0017 ||
            loaded.Enemies.MorphBallEyeBeam.Phase !=
                MorphBallEyeBeamPhase.PendingInitialization)
        {
            throw new InvalidDataException(
                $"Eye activation expiry failed: function={state.Function}, angle=${state.Angle:X4}, " +
                $"sound={loaded.Enemies.LastMorphBallEyeSoundEffect}, " +
                $"beam={loaded.Enemies.MorphBallEyeBeam.Phase}.");
        }

        Step(loaded, room, assets, frame: 33);
        if (state.ActivatedFlag != 1 ||
            loaded.Enemies.MorphBallEyeBeam.Phase != MorphBallEyeBeamPhase.Widening ||
            body.CurrentInstruction != 0x8fc0 || body.SpritemapPointer != 0x9225)
        {
            throw new InvalidDataException(
                "Eye beam initialization/tracking did not preserve native pre-AI ordering.");
        }

        int wideningFrames = 0;
        while (loaded.Enemies.MorphBallEyeBeam.Phase == MorphBallEyeBeamPhase.Widening &&
               wideningFrames < 16)
        {
            Step(loaded, room, assets, frame: 34 + wideningFrames);
            wideningFrames++;
        }
        if (loaded.Enemies.MorphBallEyeBeam.Phase != MorphBallEyeBeamPhase.Full ||
            loaded.Enemies.MorphBallEyeBeam.AngularWidth != 4 || wideningFrames != 6)
        {
            throw new InvalidDataException(
                $"Eye beam widening stopped after {wideningFrames} frames at " +
                $"{loaded.Enemies.MorphBallEyeBeam.AngularWidth}:" +
                $"{loaded.Enemies.MorphBallEyeBeam.AngularSubwidth:X4}.");
        }

        Step(loaded, room, assets, frame: 40);
        if (loaded.Enemies.MorphBallEyeBeam.Red != 0x30 ||
            loaded.Enemies.MorphBallEyeBeam.Green != 0x50 ||
            loaded.Enemies.MorphBallEyeBeam.ColorIndex != 1)
        {
            throw new InvalidDataException("Eye full-beam color cycle did not begin at ROM entry zero.");
        }

        // Exact deactivation X equality is outside. The HDMA pass runs first and therefore
        // remains Full during the frame in which body AI clears ActivatedFlag.
        loaded.Samus.XPosition = unchecked((ushort)(body.XPosition + 0x00b0));
        Step(loaded, room, assets, frame: 41);
        if (state.Function != MorphBallEyeAiFunction.Deactivating ||
            state.ActivatedFlag != 0 || state.FunctionTimer != 0x20 ||
            body.CurrentInstruction != 0x8ff4 || body.SpritemapPointer != 0x9257 ||
            loaded.Enemies.LastMorphBallEyeSoundEffect != 0x0071 ||
            loaded.Enemies.MorphBallEyeBeam.Phase != MorphBallEyeBeamPhase.Full)
        {
            throw new InvalidDataException("Eye body/beam deactivation order diverged from the cartridge.");
        }

        Step(loaded, room, assets, frame: 42);
        if (loaded.Enemies.MorphBallEyeBeam.Phase != MorphBallEyeBeamPhase.Deactivating)
            throw new InvalidDataException("Eye beam did not observe the cleared body flag one frame later.");

        int fadeFrames = 0;
        while (loaded.Enemies.MorphBallEyeBeam.Phase != MorphBallEyeBeamPhase.Inactive &&
               fadeFrames < 32)
        {
            Step(loaded, room, assets, frame: 43 + fadeFrames);
            fadeFrames++;
        }
        if (loaded.Enemies.MorphBallEyeBeam.Phase != MorphBallEyeBeamPhase.Inactive ||
            loaded.Enemies.MorphBallEyeBeam.AngularWidth != 0 ||
            loaded.Enemies.MorphBallEyeBeam.Red != 0x20 ||
            loaded.Enemies.MorphBallEyeBeam.Green != 0x40 ||
            loaded.Enemies.MorphBallEyeBeam.Blue != 0x80)
        {
            throw new InvalidDataException("Eye beam fade did not delete/reset its HDMA object.");
        }
    }

    private static void VerifyIntentionalNoCombat(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedEye loaded = Load(bus, room, assets);
        RoomEnemySlot body = loaded.Enemies.Slots[1];
        loaded.Samus.XPosition = body.XPosition;
        loaded.Samus.YPosition = body.YPosition;
        ushort samusHealth = loaded.Samus.Health;
        Step(loaded, room, assets, frame: 0);
        if (loaded.Enemies.ResolveOrdinarySamusContact(loaded.Samus, 0) ||
            loaded.Samus.Health != samusHealth)
        {
            throw new InvalidDataException("Morph-ball eye's ignored collision unexpectedly hurt Samus.");
        }

        var projectiles = new SamusProjectileSystem();
        SamusProjectileSlot shot = projectiles.Slots[0];
        shot.Type = 0x0100;
        shot.Damage = 1000;
        shot.Direction = 2;
        shot.XPosition = body.XPosition;
        shot.YPosition = body.YPosition;
        shot.XRadius = 4;
        shot.YRadius = 4;
        shot.InstructionPointer = 0x9000;
        shot.InstructionTimer = 1;
        ushort enemyHealth = body.Health;
        int hits = loaded.Enemies.ResolveOrdinaryProjectileHits(
            bus, projectiles, new SamusBombProjectileSystem(), loaded.Samus);
        if (hits != 0 || body.Health != enemyHealth ||
            body.Properties.HasAny(EnemyProperties.Deleted))
        {
            throw new InvalidDataException("Morph-ball eye incorrectly accepted projectile damage.");
        }
    }

    private static LoadedEye Load(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        var pairBus = new PopulationPrefixAddressSpace(
            bus, FinalMissilePopulation, retainedRecordCount: 2, deathQuota: 0);
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
        samus.RefreshCollisionRadii(pairBus);
        samus.InitializeAnimation(pairBus);

        var enemies = new RoomEnemySystem();
        enemies.Load(
            pairBus,
            FinalMissilePopulation,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            random.NextRandom,
            random.SetRandomNumber,
            readRandomNumber: () => random.RandomNumber,
            level: assets.LevelData,
            samus: samus);
        return new LoadedEye(enemies, samus);
    }

    private static void Step(
        LoadedEye loaded,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        int frame)
    {
        RoomEnemySlot body = loaded.Enemies.Slots[1];
        loaded.Enemies.StepFrame(
            CameraX(room, body),
            CameraY(room, body),
            timeIsFrozen: false,
            loaded.Samus,
            level: assets.LevelData,
            nmiFrameCounter8: unchecked((byte)frame));
    }

    private static MorphBallEyeEnemyState RequireState(
        RoomEnemySystem enemies,
        RoomEnemySlot slot) =>
        enemies.MorphBallEyeStates[slot.SlotIndex] ?? throw new InvalidDataException(
            $"Morph-ball eye slot {slot.SlotIndex} has no typed state.");

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

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private static string FormatWords(IEnumerable<ushort> words) =>
        string.Join(',', words.Order().Select(word => $"${word:X4}"));

    private readonly record struct EyeRecord(
        ushort Definition,
        ushort X,
        ushort Y,
        ushort InitializationParameter,
        ushort Properties,
        ushort ExtraProperties,
        ushort Parameter1,
        ushort Parameter2);

    private sealed record LoadedEye(RoomEnemySystem Enemies, SamusState Samus);
}
