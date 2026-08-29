using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>
/// ROM-backed audit for friendly Etecoons <c>$E5BF</c>. The Green Brinstar main-shaft
/// population contains exactly the three roles consumed by the native route table, so the
/// audit retains those cartridge records and the authentic room collision map rather than
/// constructing a synthetic platform course.
/// </summary>
internal static class EtecoonAudit
{
    private const ushort EtecoonDefinition = 0xe5bf;
    private const ushort GreenBrinstarMainShaftRoom = 0x9ad9;
    private const ushort GreenBrinstarMainShaftState = 0x9ae6;
    private const ushort GreenBrinstarMainShaftPopulation = 0x997a;
    private const ushort FirstEtecoonRecord = 0x99fa;

    private static readonly EtecoonRecord[] RetailRecords =
    [
        new(0x025f, 0x0b98, 0x0000, 0x0c00, 0x0000, 0x0000, 0x0000),
        new(0x026f, 0x0b98, 0x0000, 0x0c00, 0x0000, 0x0000, 0x0001),
        new(0x027f, 0x0b98, 0x0000, 0x0c00, 0x0000, 0x0000, 0x0002),
    ];

    // Every ordinary map referenced by $A7:E81E-$E8FE belongs to the contiguous Etecoon
    // spritemap set. Checking the complete set catches a valid-looking but truncated list.
    // These are the spritemaps actually referenced by Etecoon's retail instruction
    // lists. $A7:F049 is valid Etecoon art, but no shipped instruction list selects it,
    // so requiring it here would test unreachable data rather than translated behavior.
    private static readonly HashSet<ushort> ReferencedSpritemaps =
    [
        0xeeed, 0xeefe, 0xef0a, 0xef16, 0xef27, 0xef38, 0xef49, 0xef5a,
        0xef6b, 0xef90, 0xefb5, 0xefda, 0xefff, 0xf024, 0xf06e,
        0xf089, 0xf09a, 0xf0a6, 0xf0b2, 0xf0c3, 0xf0d4, 0xf0e5, 0xf0f6,
        0xf107, 0xf12c, 0xf151, 0xf176, 0xf19b, 0xf1c0, 0xf1e5, 0xf20a,
    ];

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        VerifyDefinitionHeader(bus);
        VerifyRetailPopulation(bus);

        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, GreenBrinstarMainShaftRoom);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        if (room.State.Pointer != GreenBrinstarMainShaftState ||
            room.State.EnemyPopulationPointer != GreenBrinstarMainShaftPopulation)
        {
            throw new InvalidDataException(
                $"Green Brinstar main shaft selected state ${room.State.Pointer:X4} and " +
                $"population ${room.State.EnemyPopulationPointer:X4}.");
        }

        VerifyInitializationAnimationAndDrawing(bus, room, assets);
        VerifyWakeCountdownAndRouteTable(bus, room, assets);
        VerifyAllMovementFunctions(bus, room, assets);
        VerifyEarthquakeSuspension(bus, room, assets);
        VerifyAuthoredNonCombatBehavior(bus, room, assets);

        Console.WriteLine(
            "Etecoon audit passed: all three retail records, complete ROM animation set, " +
            "wake/flex countdown, three role routes, all 20 stored movement functions, " +
            "wall and floor collision, fixed-point gravity, quake suspension, drawing, " +
            "and authored no-contact/no-shot/no-grapple behavior were verified.");
        return 0;
    }

    private static void VerifyDefinitionHeader(SuperMetroidAddressSpace bus)
    {
        RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(bus, EtecoonDefinition);
        if (definition.TileDataSize != 0x0600 || definition.Health != 0x7fff ||
            definition.Damage != 0 || definition.XRadius != 6 || definition.YRadius != 7 ||
            definition.Bank != 0xa7 || definition.InitializationAiPointer != 0xe912 ||
            definition.MainAiPointer != 0xe940 || definition.GrappleAiPointer != 0x8000 ||
            definition.HurtAiPointer != 0x804c || definition.FrozenAiPointer != 0x8041 ||
            definition.PowerBombReactionPointer != 0x804c ||
            definition.TouchAiPointer != 0x804c || definition.ShotAiPointer != 0x804c ||
            definition.Layer != 5 || definition.VulnerabilityPointer != 0)
        {
            throw new InvalidDataException(
                "Etecoon $E5BF header disagrees with the translated family: " +
                $"size=${definition.TileDataSize:X4}, hp/damage=" +
                $"{definition.Health}/{definition.Damage}, radius=" +
                $"{definition.XRadius}/{definition.YRadius}, bank/init/main=" +
                $"${definition.Bank:X2}:${definition.InitializationAiPointer:X4}/" +
                $"${definition.MainAiPointer:X4}, grapple/hurt/frozen=" +
                $"${definition.GrappleAiPointer:X4}/${definition.HurtAiPointer:X4}/" +
                $"${definition.FrozenAiPointer:X4}, touch/shot=" +
                $"${definition.TouchAiPointer:X4}/${definition.ShotAiPointer:X4}.");
        }
    }

    private static void VerifyRetailPopulation(SuperMetroidAddressSpace bus)
    {
        var actual = new List<EtecoonRecord>();
        int cursor = 0xa10000 | GreenBrinstarMainShaftPopulation;
        for (int index = 0; index < RoomEnemySystem.MaximumEnemyCount; index++, cursor += 16)
        {
            ushort definition = ReadWord(bus, cursor);
            if (definition == 0xffff)
                break;
            if (definition != EtecoonDefinition)
                continue;

            actual.Add(new EtecoonRecord(
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
                $"Green Brinstar main shaft contained {actual.Count} Etecoon records " +
                $"instead of the three translated role records.");
        }
    }

    private static void VerifyInitializationAnimationAndDrawing(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedEtecoons loaded = Load(bus, room, assets, FirstEtecoonRecord, 3);
        if (loaded.Enemies.EnemyCount != 3)
            throw new InvalidDataException($"Etecoon prefix loaded {loaded.Enemies.EnemyCount} actors.");

        for (int index = 0; index < 3; index++)
        {
            RoomEnemySlot actor = loaded.Enemies.Slots[index];
            EtecoonEnemyState state = RequireState(loaded.Enemies, actor);
            EtecoonRecord record = RetailRecords[index];
            if (actor.EnemyDefinitionPointer != EtecoonDefinition ||
                actor.XPosition != record.X || actor.YPosition != record.Y ||
                actor.CurrentInstruction != 0xe8ce || actor.SpritemapPointer != 0x804d ||
                actor.Properties != 0x2c00 || actor.Parameter1 != 0 ||
                actor.Parameter2 != index || state.Function != EtecoonAiFunction.WaitingForSamus ||
                state.FunctionTimer != 0xffff)
            {
                throw new InvalidDataException(
                    $"Etecoon role {index} initialization failed: position=" +
                    $"(${actor.XPosition:X4},${actor.YPosition:X4}), list/map=" +
                    $"${actor.CurrentInstruction:X4}/${actor.SpritemapPointer:X4}, " +
                    $"properties=${actor.Properties:X4}, params=" +
                    $"${actor.Parameter1:X4}/${actor.Parameter2:X4}, function/timer=" +
                    $"{state.Function}/${state.FunctionTimer:X4}.");
            }
        }

        // Samus remains outside the 128-pixel activation band, so this frame exercises only
        // the initial looping list and proves AI does not wake from horizontal proximity.
        loaded.Samus.XPosition = loaded.Enemies.Slots[0].XPosition;
        loaded.Samus.YPosition = 0;
        Step(loaded, room, assets);
        if (loaded.Enemies.Slots.Take(3).Any(actor => actor.SpritemapPointer != 0xf107))
        {
            throw new InvalidDataException(
                "Etecoon initial list did not emit the ROM's facing-right idle map $F107.");
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
            throw new InvalidDataException("Etecoon ROM spritemap emitted no OBJ pieces.");
    }

    private static void VerifyWakeCountdownAndRouteTable(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedEtecoons loaded = Load(bus, room, assets, FirstEtecoonRecord, 3);
        RoomEnemySlot focus = loaded.Enemies.Slots[0];
        loaded.Samus.XPosition = 0x0270;
        loaded.Samus.YPosition = focus.YPosition;

        Step(loaded, room, assets);
        if (loaded.Enemies.LastEtecoonSoundEffect != 0x0035 ||
            loaded.Enemies.Slots.Take(3).Any(actor => actor.CurrentInstruction != 0xe8de) ||
            loaded.Enemies.EtecoonStates.Any(state => state is not null &&
                state.FunctionTimer != 256))
        {
            throw new InvalidDataException(
                "Etecoon activation failed to install flexing, its 256-frame delay, and " +
                $"sound $35: sound={loaded.Enemies.LastEtecoonSoundEffect:X4}, lists=" +
                $"[{FormatWords(loaded.Enemies.Slots.Take(3).Select(actor => actor.CurrentInstruction))}], " +
                $"timers=[{string.Join(',', loaded.Enemies.EtecoonStates.Where(state => state is not null).Select(state => state!.FunctionTimer))}].");
        }

        // The flex list and AI countdown are independent. Exactly 255 calls leave E=1;
        // the 256th decrements to zero and enters the eleven-frame pre-jump crouch.
        for (int frame = 0; frame < 255; frame++)
            Step(loaded, room, assets);
        if (RequireState(loaded.Enemies, focus).FunctionTimer != 1 ||
            RequireState(loaded.Enemies, focus).Function != EtecoonAiFunction.WaitingForSamus)
        {
            throw new InvalidDataException("Etecoon 256-frame flex countdown ended early.");
        }
        Step(loaded, room, assets);
        if (RequireState(loaded.Enemies, focus).Function != EtecoonAiFunction.PreJumpCountdown ||
            RequireState(loaded.Enemies, focus).FunctionTimer != 11)
        {
            throw new InvalidDataException("Etecoon flex countdown did not enter pre-jump crouch.");
        }

        // RouteAfterLanding is the native three-entry indirect table at $A7:EC1B. Put all
        // three authentic roles on its expiry frame and verify the literal low-byte index.
        EtecoonAiFunction[] expectedRoutes =
        [
            EtecoonAiFunction.MoveLeftToTeachingStart,
            EtecoonAiFunction.MoveRightToTeachingStart,
            EtecoonAiFunction.TeachingJump,
        ];
        for (int index = 0; index < 3; index++)
        {
            RoomEnemySlot actor = loaded.Enemies.Slots[index];
            EtecoonEnemyState state = RequireState(loaded.Enemies, actor);
            state.Function = EtecoonAiFunction.RouteAfterLanding;
            state.FunctionTimer = 1;
            actor.CurrentInstruction = index == 2 ? (ushort)0xe8b0 : (ushort)0xe854;
            actor.InstructionTimer = 1;
        }
        Step(loaded, room, assets);
        for (int index = 0; index < 3; index++)
        {
            EtecoonEnemyState state = RequireState(loaded.Enemies, loaded.Enemies.Slots[index]);
            if (state.Function != expectedRoutes[index])
            {
                throw new InvalidDataException(
                    $"Etecoon role {index} selected {state.Function}, expected {expectedRoutes[index]}.");
            }
        }
    }

    private static void VerifyAllMovementFunctions(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        var seenFunctions = new HashSet<EtecoonAiFunction>();
        var seenMaps = new HashSet<ushort>();

        // Run the complete 256-frame wake/flex countdown once before abbreviating it in
        // the route coverage below. Its late frames contain maps $F19B and $F1E5; skipping
        // the countdown would make a faithful but deliberately slow animation look dead.
        LoadedEtecoons flexing = Load(bus, room, assets, FirstEtecoonRecord, 1);
        RoomEnemySlot flexingActor = flexing.Enemies.Slots[0];
        flexing.Samus.XPosition = flexingActor.XPosition;
        flexing.Samus.YPosition = flexingActor.YPosition;
        for (int frame = 0; frame <= 256; frame++)
        {
            Step(flexing, room, assets);
            if (ReferencedSpritemaps.Contains(flexingActor.SpritemapPointer))
                seenMaps.Add(flexingActor.SpritemapPointer);
        }

        // Each role is run independently so one actor's camera/activity state cannot mask
        // another. Samus follows the actor closely enough to satisfy authored proximity
        // gates while the room's actual walls, floors, and shafts drive every collision.
        for (int role = 0; role < 3; role++)
        {
            LoadedEtecoons loaded = Load(
                bus,
                room,
                assets,
                unchecked((ushort)(FirstEtecoonRecord + role * 16)),
                retainedRecordCount: 1);
            RoomEnemySlot actor = loaded.Enemies.Slots[0];
            EtecoonEnemyState state = RequireState(loaded.Enemies, actor);

            // Preserve the real wake branch but abbreviate its long idle timer. The timer's
            // full 256-frame semantics were verified separately immediately above.
            loaded.Samus.XPosition = unchecked((ushort)(actor.XPosition + 8));
            loaded.Samus.YPosition = actor.YPosition;
            Step(loaded, room, assets);
            seenFunctions.Add(state.Function);
            if (ReferencedSpritemaps.Contains(actor.SpritemapPointer))
                seenMaps.Add(actor.SpritemapPointer);
            state.FunctionTimer = 1;

            for (int frame = 0; frame < 6000; frame++)
            {
                // Follow in world space. This supplies only Samus's authored gating input;
                // it does not alter the actor, collision layer, velocities, or animation.
                // Role two must see Samus to its left to take the shipped look-left/run-
                // right branch. The other roles follow directly underneath their actor.
                loaded.Samus.XPosition = role == 2
                    ? unchecked((ushort)(actor.XPosition - 8))
                    : actor.XPosition;
                loaded.Samus.YPosition = actor.YPosition;
                Step(loaded, room, assets);
                seenFunctions.Add(state.Function);
                if (ReferencedSpritemaps.Contains(actor.SpritemapPointer))
                    seenMaps.Add(actor.SpritemapPointer);

                // Route zero and one eventually repeat after demonstrating their four-hop
                // sequence. Route two repeats its long-run lesson. Six thousand frames is
                // deliberately generous but deterministic on the retail room geometry.
                if (seenFunctions.Count == Enum.GetValues<EtecoonAiFunction>().Length &&
                    seenMaps.SetEquals(ReferencedSpritemaps))
                {
                    break;
                }
            }
        }

        EtecoonAiFunction[] missingFunctions = Enum.GetValues<EtecoonAiFunction>()
            .Where(function => !seenFunctions.Contains(function))
            .ToArray();
        if (missingFunctions.Length != 0)
        {
            throw new InvalidDataException(
                "Etecoon retail movement did not visit: " + string.Join(
                    ", ",
                    missingFunctions.Select(function =>
                        $"{function}($A7:{(ushort)function:X4})")));
        }

        if (!seenMaps.SetEquals(ReferencedSpritemaps))
        {
            throw new InvalidDataException(
                $"Etecoon movement emitted [{FormatWords(seenMaps)}], missing " +
                $"[{FormatWords(ReferencedSpritemaps.Except(seenMaps))}].");
        }
    }

    private static void VerifyEarthquakeSuspension(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedEtecoons loaded = Load(bus, room, assets, FirstEtecoonRecord, 1);
        RoomEnemySlot actor = loaded.Enemies.Slots[0];
        EtecoonEnemyState state = RequireState(loaded.Enemies, actor);
        loaded.Samus.XPosition = actor.XPosition;
        loaded.Samus.YPosition = actor.YPosition;
        state.Function = EtecoonAiFunction.Airborne;
        state.HorizontalVelocity = 2;
        state.HorizontalSubvelocity = 0;
        state.VerticalVelocity = 0xfffd;
        state.VerticalSubvelocity = 0;
        actor.Parameter1 = 1;
        actor.Parameter2 = 2;
        actor.CurrentInstruction = 0xe898;
        actor.InstructionTimer = 5;
        loaded.Enemies.EarthquakeTimer = 1;

        Step(loaded, room, assets);
        ushort movedX = actor.XPosition;
        ushort movedY = actor.YPosition;
        if (actor.Parameter2 != 0x8002 || actor.InstructionTimer != 132 ||
            movedX == RetailRecords[0].X || movedY == RetailRecords[0].Y)
        {
            throw new InvalidDataException(
                $"Etecoon quake entry failed: parameter2=${actor.Parameter2:X4}, " +
                $"timer={actor.InstructionTimer}, position=(${movedX:X4},${movedY:X4}).");
        }

        loaded.Enemies.EarthquakeTimer = 0;
        Step(loaded, room, assets);
        if (actor.Parameter2 != 0x7f02 || actor.XPosition != movedX || actor.YPosition != movedY)
        {
            throw new InvalidDataException(
                "Etecoon quake high-byte countdown did not suspend actor AI while preserving route two.");
        }
    }

    private static void VerifyAuthoredNonCombatBehavior(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedEtecoons loaded = Load(bus, room, assets, FirstEtecoonRecord, 1);
        RoomEnemySlot actor = loaded.Enemies.Slots[0];
        loaded.Samus.XPosition = actor.XPosition;
        loaded.Samus.YPosition = actor.YPosition;
        Step(loaded, room, assets);
        ushort samusHealth = loaded.Samus.Health;
        ushort enemyHealth = actor.Health;

        if (loaded.Enemies.ResolveOrdinarySamusContact(loaded.Samus, actor.NativeIndex) ||
            loaded.Samus.Health != samusHealth)
        {
            throw new InvalidDataException("Etecoon's no-touch header damaged or displaced Samus.");
        }

        var projectiles = new SamusProjectileSystem();
        var sharedProjectiles = new SamusBombProjectileSystem();
        ArmProjectile(projectiles.Slots[0], actor);
        int hits = loaded.Enemies.ResolveOrdinaryProjectileHits(
            bus,
            projectiles,
            sharedProjectiles,
            loaded.Samus);
        int powerBombReactions = loaded.Enemies.ResolveOrdinaryPowerBombHits(
            bus,
            actor.XPosition,
            actor.YPosition,
            explosionRadius: 32);
        GrappleEnemyCollision grapple = loaded.Enemies.ResolveGrappleEndpoint(
            actor.XPosition,
            actor.YPosition);
        // The host count reports that the power-bomb dispatcher invoked Etecoon's literal
        // RTL callback. Native code still sets process-off-screen after that callback, but
        // the no-op itself must leave health, freeze state, and visibility untouched.
        if (hits != 0 || powerBombReactions != 1 || grapple.Collided ||
            actor.Health != enemyHealth || actor.FrozenTimer != 0)
        {
            throw new InvalidDataException(
                $"Etecoon authored non-combat behavior failed: hits={hits}, " +
                $"powerBomb={powerBombReactions}, grapple={grapple}, " +
                $"health={enemyHealth}->{actor.Health}, frozen={actor.FrozenTimer}.");
        }
    }

    private static LoadedEtecoons Load(
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
        samus.Kinematics.YSubacceleration = ReadWord(bus, 0x909ea1);
        samus.Kinematics.YAcceleration = ReadWord(bus, 0x909ea7);

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
        return new LoadedEtecoons(enemies, samus);
    }

    private static void Step(
        LoadedEtecoons loaded,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        RoomEnemySlot focus = loaded.Enemies.Slots[0];
        loaded.Enemies.StepFrame(
            CameraX(room, focus),
            CameraY(room, focus),
            false,
            loaded.Samus,
            level: assets.LevelData);
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

    private static EtecoonEnemyState RequireState(
        RoomEnemySystem enemies,
        RoomEnemySlot actor) =>
        enemies.EtecoonStates[actor.SlotIndex] ?? throw new InvalidDataException(
            $"Etecoon slot {actor.SlotIndex} has no typed state.");

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

    private readonly record struct EtecoonRecord(
        ushort X,
        ushort Y,
        ushort InitializationParameter,
        ushort Properties,
        ushort ExtraProperties,
        ushort Parameter1,
        ushort Parameter2);

    private readonly record struct LoadedEtecoons(
        RoomEnemySystem Enemies,
        SamusState Samus);
}
