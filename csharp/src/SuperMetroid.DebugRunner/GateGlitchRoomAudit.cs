using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>Measures stationary launch windows without confusing gate crossing with switch activation.</summary>
internal static class GateGlitchRoomAudit
{
    public static int InspectRetailRoom(string rom, ushort roomPointer)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(roomPointer);

        RoomLevelData level = runtime.LevelData!;
        Console.WriteLine($"room=${roomPointer:X4} area=${(byte)runtime.ActiveRoom!.AreaIndex:X2} size={level.WidthInBlocks}x{level.HeightInBlocks} state=${runtime.ActiveRoom.State.Pointer:X4} level=${runtime.ActiveRoom.State.CompressedLevelDataAddress:X6} population=${runtime.ActiveRoom.State.PlmPointer:X4}");
        foreach (var slot in runtime.Plms.PopulationSlots)
        {
            int x = slot.BlockIndex % level.WidthInBlocks;
            int y = slot.BlockIndex / level.WidthInBlocks;
            Console.WriteLine($"plm slot={slot.NativeSlotIndex:X2} header=${slot.HeaderPointer:X4} block=${slot.BlockIndex:X4} ({x},{y}) arg=${slot.RoomArgument:X4} list=${slot.InstructionPointer:X4}");
        }

        foreach (var enemy in runtime.Enemies.Slots.Where(candidate => candidate.EnemyDefinitionPointer != 0))
            Console.WriteLine($"enemy native={enemy.NativeIndex:X2} id=${enemy.EnemyDefinitionPointer:X4} pos=({enemy.XPosition},{enemy.YPosition}) radius=({enemy.XRadius},{enemy.YRadius}) ai=${enemy.CurrentInstruction:X4}");
        var gateSlot = runtime.Plms.PopulationSlots.Single(slot =>
            slot.HeaderPointer == RoomPlmHeaders.DownwardGate);
        int gateBlockX = gateSlot.BlockIndex % level.WidthInBlocks;
        int gateBlockY = gateSlot.BlockIndex / level.WidthInBlocks;
        for (int y = Math.Max(0, gateBlockY - 2); y <= Math.Min(level.HeightInBlocks - 1, gateBlockY + 7); y++)
        {
            Console.Write($"row {y:D2}:");
            for (int x = Math.Max(0, gateBlockX - 8); x <= Math.Min(level.WidthInBlocks - 1, gateBlockX + 4); x++)
            {
                RoomCollisionBlock block = level.GetCollisionBlockByIndex(level.GetBlockIndex(x, y));
                Console.Write($" {x:D2}={block.LevelWord:X4}/{block.Behavior:X2}");
            }
            Console.WriteLine();
        }
        return 0;
    }

    public static int SweepRightFacingRetailGate(string rom, ushort roomPointer)
    {
        ISnesAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(roomPointer);
        RoomLevelData original = runtime.LevelData!;
        var residentGate = runtime.Plms.PopulationSlots.Single(
            slot => slot.HeaderPointer == RoomPlmHeaders.DownwardGate);
        int gateX = residentGate.BlockIndex % original.WidthInBlocks * 16;
        int gateY = residentGate.BlockIndex / original.WidthInBlocks * 16;
        int cameraX = Math.Max(0, gateX - 128);
        int cameraY = Math.Max(0, gateY - 96);
        Console.WriteLine($"room=${roomPointer:X4},gate=({gateX},{gateY}),camera=({cameraX},{cameraY})");
        Console.WriteLine("x,y,hitFrame,shotX,shotY,shotVelocity,linkX");
        int successes = 0;
        int maximumX = roomPointer == RoomHeaderPointers.EastTunnel ? gateX + 24 : gateX - 1;
        for (int x = gateX - 192; x <= maximumX; x++)
        for (int y = gateY - 32; y <= gateY + 96; y++)
        {
            var level = new RoomLevelData(
                original.WidthInBlocks,
                original.HeightInBlocks,
                original.ForegroundEntries.Span,
                original.BehaviorBytes.Span,
                original.BackgroundEntries.Span,
                original.BlockDefinitions.Span);
            var samus = new SamusState
            {
                XPosition = unchecked((ushort)x),
                YPosition = unchecked((ushort)y),
                PoseId = roomPointer == RoomHeaderPointers.EastTunnel
                    ? SamusPoseId.NormalJumpAimDiagonalUpRightPose
                    : SamusPoseId.FacingRightNormalPose,
                SelectedHudItem = 2,
                SuperMissiles = 10,
                MaxSuperMissiles = 10,
            };
            var plms = new RoomPlmSystem();
            var streamer = level.CreateBackgroundStreamer(0);
            plms.LoadRoomPopulation(
                bus,
                level,
                streamer,
                new SnesVram(),
                runtime.ActiveRoom!.State.PlmPointer,
                new Bank80SystemState(),
                runtime.ActiveRoom.AreaIndex,
                () => samus,
                () => false);
            for (int warm = 0; warm < 2; warm++)
                plms.Step(bus, level, streamer, (ushort)cameraX, (ushort)cameraY, 0);
            plms.TakeDownwardGateProjectileRequests();

            var shots = new SamusProjectileSystem();
            var bombs = new SamusBombProjectileSystem();
            for (int frame = 0; frame < 30; frame++)
            {
                ushort input = frame == 0 ? (ushort)SnesButton.X : (ushort)0;
                bombs.StepFrame(bus, level, samus, input, input);
                shots.StepFrame(
                    bus,
                    level,
                    samus,
                    input,
                    input,
                    (ushort)cameraX,
                    (ushort)cameraY,
                    bombs,
                    roomPlms: plms);
                var gate = plms.PopulationSlots.Single(
                    slot => slot.HeaderPointer == RoomPlmHeaders.DownwardGate);
                if (gate.LoopTimer != 0)
                {
                    SamusProjectileSlot owner = shots.Slots[0];
                    SamusProjectileSlot? link = shots.Slots.FirstOrDefault(slot =>
                        slot.PackedType.HasPlainFamilyPayload(SamusProjectileFamily.SuperMissile) &&
                        slot.Damage == 0);
                    Console.WriteLine($"{x},{y},{frame},{owner.XPosition},{owner.YPosition},{owner.XVelocity},{link?.XPosition ?? 0}");
                    successes++;
                    break;
                }
                plms.Step(bus, level, streamer, (ushort)cameraX, (ushort)cameraY, 0);
            }
        }
        Console.WriteLine($"successes={successes}");
        return 0;
    }

    public static int SearchEastTunnelFrozenSetup(string rom)
    {
        ISnesAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        int attempts = 0;
        int successes = 0;
        ushort maximumSamusX = 0;
        string maximumRecord = string.Empty;
        for (int frozenX = 360; frozenX <= 370; frozenX++)
        for (int frozenY = 139; frozenY <= 139; frozenY++)
        for (int shootFrame = 0; shootFrame <= 20; shootFrame++)
        {
            attempts++;
            var runtime = new SuperMetroidRuntime(bus);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.EastTunnel);
            var samus = runtime.Samus!;
            samus.InputLocked = false;
            samus.PoseId = SamusPoseId.FacingRightNormalPose;
            samus.XPosition = 238;
            samus.Kinematics.XSubposition = 8191;
            samus.YPosition = 139;
            samus.EquippedItems = (ushort)SamusEquipmentFlags.HiJumpBoots;
            samus.EquippedBeams = (ushort)(SamusBeamFlags.Ice | SamusBeamFlags.Wave);
            samus.SelectedHudItem = 2;
            samus.SuperMissiles = samus.MaxSuperMissiles = 10;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            runtime.Camera!.SetPosition(128, 0);

            RoomEnemySlot frozen = runtime.Enemies.Slots.First(slot =>
                slot.EnemyDefinitionPointer == BoyonAuditDefinitions.BoyonEnemyDefinition);
            foreach (RoomEnemySlot enemy in runtime.Enemies.Slots)
            {
                if (ReferenceEquals(enemy, frozen))
                    continue;
                enemy.EnemyDefinitionPointer = 0;
            }
            frozen.XPosition = unchecked((ushort)frozenX);
            frozen.YPosition = unchecked((ushort)frozenY);
            frozen.FrozenTimer = 1000;
            frozen.AiHandlerBits |= 4;

            // Pause darkening continues state-eight gameplay for thirty frames. The
            // published setup holds right+dash throughout that fade, freezes at black,
            // then introduces jump+dash+angle on the first resumed gameplay sample.
            for (int runFrame = 0; runFrame < 31; runFrame++)
                runtime.StepFrame((ushort)(SnesButton.Right | SnesButton.B));

            for (int frame = 0; frame < 40; frame++)
            {
                SnesButton input = SnesButton.A | SnesButton.B | SnesButton.R;
                if (frame == shootFrame)
                    input |= SnesButton.X;
                runtime.StepFrame((ushort)input);
                if (samus.XPosition > maximumSamusX)
                {
                    maximumSamusX = samus.XPosition;
                    maximumRecord = $"enemy=({frozenX},{frozenY}) shoot={shootFrame} frame={frame} " +
                        $"samus=({samus.XPosition}.{samus.Kinematics.XSubposition:D5}," +
                        $"{samus.YPosition}.{samus.Kinematics.YSubposition:D5}) pose=${samus.Pose:X2} " +
                        $"support={string.Join('/', samus.Kinematics.SolidEnemyCollisionIndexes.Select(value => value.ToString("X4")))}";
                }

                bool opened = runtime.Plms.PopulationSlots.Any(slot =>
                    slot.HeaderPointer == RoomPlmHeaders.DownwardGate && slot.LoopTimer != 0);
                if (opened)
                {
                    Console.WriteLine(
                        $"EAST success attempt={attempts} enemy=({frozenX},{frozenY}) " +
                        $"shoot={shootFrame} frame={frame} " +
                        $"samus=({samus.XPosition}.{samus.Kinematics.XSubposition:D5}," +
                        $"{samus.YPosition}.{samus.Kinematics.YSubposition:D5}) pose=${samus.Pose:X2} " +
                        $"enemy=({frozen.XPosition},{frozen.YPosition}) frozen={frozen.FrozenTimer}");
                    successes++;
                    break;
                }
            }
        }
        Console.WriteLine($"EAST {successes} successes across {attempts} pause-handoff frozen-enemy candidates; max {maximumRecord}.");
        return successes == 0 ? 1 : 0;
    }

    public static int WriteEastTunnelNativeSeed(string rom, string path)
    {
        ISnesAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.EastTunnel);
        SamusState samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.PoseId = SamusPoseId.FacingRightNormalPose;
        samus.XPosition = 238;
        samus.Kinematics.XSubposition = 8191;
        samus.YPosition = 139;
        samus.EquippedItems = (ushort)SamusEquipmentFlags.HiJumpBoots;
        samus.EquippedBeams = (ushort)(SamusBeamFlags.Ice | SamusBeamFlags.Wave);
        samus.SelectedHudItem = 2;
        samus.SuperMissiles = samus.MaxSuperMissiles = 10;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        runtime.Camera!.SetPosition(128, 0);
        RoomMovementSeedExporter.Write(runtime, path);
        Console.WriteLine($"Wrote East Tunnel native movement seed to {Path.GetFullPath(path)}.");
        return 0;
    }

    public static int RunJump(string rom, int shootFrame, int aimFrame = 0, bool releaseJump = false, bool releaseLeft = false)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.KronicBoost);
        var samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.PoseId = SamusPoseId.StandingAimDiagonalUpLeftPose;
        samus.XPosition = 140;
        samus.YPosition = 379;
        samus.EquippedItems = samus.EquippedBeams = 0;
        samus.SelectedHudItem = 1;
        samus.Missiles = samus.MaxMissiles = 10;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        runtime.Camera!.SetPosition(0, 224);
        Console.WriteLine("frame,input,x,subx,y,pose,gateTimer,gateInstruction");
        for (int frame = -60; frame < 80; frame++)
        {
            ushort input = frame < 0 ? (ushort)0 : (ushort)(SnesButton.A | SnesButton.Left);
            if (aimFrame > 0 && frame == -1) input = (ushort)SnesButton.Left;
            if (frame >= aimFrame) input |= (ushort)SnesButton.R;
            if (releaseJump && frame >= aimFrame) input &= unchecked((ushort)~(ushort)SnesButton.A);
            if (releaseLeft && frame >= aimFrame) input &= unchecked((ushort)~(ushort)SnesButton.Left);
            if (frame == shootFrame) input |= (ushort)SnesButton.X;
            runtime.StepFrame(input);
            var gate = runtime.Plms.PopulationSlots.Single(s => s.HeaderPointer == RoomPlmHeaders.DownwardGate);
            Console.WriteLine($"{frame},{input:X4},{samus.XPosition},{samus.Kinematics.XSubposition},{samus.YPosition},{samus.Pose:X2},{gate.LoopTimer},{gate.InstructionPointer:X4}");
        }
        return 0;
    }

    public static int Run(string rom, byte? gateArgument = null, string? nativeTrace = null)
    {
        var actualRecords = new List<string>();
        ISnesAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        if (gateArgument.HasValue) bus = new GateVariantBus(bus, gateArgument.Value);
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.KronicBoost);
        var original = runtime.LevelData!;
        var gate = runtime.Plms.PopulationSlots.Single(s => s.HeaderPointer == RoomPlmHeaders.DownwardGate);
        int gateX = gate.BlockIndex % original.WidthInBlocks * 16;
        int gateY = gate.BlockIndex / original.WidthInBlocks * 16;
        Console.WriteLine($"Kronic Boost {original.WidthInBlocks}x{original.HeightInBlocks}; gate={gate.BlockIndex:X4} pixel={gateX},{gateY}");
        Console.WriteLine("item,x,y,hitFrame,shotX,shotY");
        for (ushort item = 0; item <= 2; item++)
        {
            int successes = 0;
            for (int x = gateX + 16; x <= gateX + 40; x++)
            for (int y = gateY - 16; y <= gateY + 64; y++)
            {
                var level = new RoomLevelData(original.WidthInBlocks, original.HeightInBlocks,
                    original.ForegroundEntries.Span, original.BehaviorBytes.Span,
                    original.BackgroundEntries.Span, original.BlockDefinitions.Span);
                var samus = new SamusState { XPosition = (ushort)x, YPosition = (ushort)y,
                    PoseId = SamusPoseId.NormalJumpAimDiagonalUpLeftPose, SelectedHudItem = item,
                    Missiles = 10, MaxMissiles = 10, SuperMissiles = 10, MaxSuperMissiles = 10 };
                var plms = new RoomPlmSystem();
                var streamer = level.CreateBackgroundStreamer(0);
                plms.LoadRoomPopulation(bus, level, streamer, new SnesVram(),
                    runtime.ActiveRoom!.State.PlmPointer, new Bank80SystemState(),
                    runtime.ActiveRoom.AreaIndex, () => samus, () => false);
                for (int warm = 0; warm < 2; warm++) plms.Step(bus, level, streamer, 0, 0, 0);
                plms.TakeDownwardGateProjectileRequests();
                var shots = new SamusProjectileSystem();
                var bombs = new SamusBombProjectileSystem();
                for (int frame = 0; frame < 24; frame++)
                {
                    ushort input = frame == 0 ? (ushort)SnesButton.X : (ushort)0;
                    bombs.StepFrame(bus, level, samus, input, input);
                    shots.StepFrame(bus, level, samus, input, input, 0,
                        (ushort)Math.Max(0, gateY - 96), bombs, roomPlms: plms);
                    bool hit = plms.PopulationSlots.Any(s => s.NativeSlotIndex == gate.NativeSlotIndex && s.LoopTimer != 0);
                    if (hit)
                    {
                        var shot = shots.Slots[0];
                        string record = $"{item},{x},{y},{frame},{shot.XPosition},{shot.YPosition}";
                        actualRecords.Add(record);
                        Console.WriteLine(record);
                        successes++;
                        break;
                    }
                    plms.Step(bus, level, streamer, 0, (ushort)Math.Max(0, gateY - 96), 0);
                }
            }
            Console.WriteLine($"Result item={item}: {successes}/2025 stationary origins activate the switch.");
        }
        if (nativeTrace is not null)
        {
            var expected = File.ReadLines(nativeTrace).Skip(1).ToArray();
            if (!expected.SequenceEqual(actualRecords))
                throw new InvalidDataException($"Gate variant {gateArgument}: native activation window differs.");
            Console.WriteLine($"PASS native gate variant {gateArgument}: all 6075 origins, {expected.Length} exact positive records.");
        }
        return 0;
    }

    /// <summary>Changes only the authored shot-block argument; all setup tables remain retail.</summary>
    private sealed class GateVariantBus : ISnesAddressSpace
    {
        private readonly ISnesAddressSpace _inner;
        private readonly int _argumentAddress;
        private readonly byte _argument;

        public GateVariantBus(ISnesAddressSpace inner, byte argument)
        {
            if (argument is not (0 or 2 or 8 or 10))
                throw new ArgumentOutOfRangeException(nameof(argument), "Use blue/green left/right table offsets 0,2,8,10.");
            _inner = inner;
            _argument = argument;
            // Kronic Boost's pinned room state: population word at state +20.
            int entry = 0x8f0000 | inner.ReadByte(0x8fae95) | inner.ReadByte(0x8fae96) << 8;
            while (true)
            {
                int header = inner.ReadByte(entry) | inner.ReadByte(entry + 1) << 8;
                if (header == 0) throw new InvalidDataException("Kronic shot-block actor missing.");
                if (header == RoomPlmHeaders.DownwardGateShotBlock) { _argumentAddress = entry + 4; break; }
                entry += 6;
            }
        }
        public byte ReadByte(int address) => address == _argumentAddress ? _argument : _inner.ReadByte(address);
        public void WriteByte(int address, byte value) => _inner.WriteByte(address, value);
    }
}
