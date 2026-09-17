using System.Buffers.Binary;
using System.Globalization;
using System.Reflection;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using M = EtecoonFloorClipMemory;

/// <summary>Compares the native issue-638 reserve-pause floor crossing with production code.</summary>
internal static class EtecoonFloorClipNativeProbe
{
    private const int SeedFrame = 4835;
    private const int FinalFrame = 5200;

    public static int Run(string romPath, string directory)
    {
        byte[] seed = ReadCheckpoint(directory, SeedFrame);
        ushort[] inputs = File.ReadLines(Path.Combine(directory, "inputs-green.csv"))
            .Skip(1)
            .Select(line => ushort.Parse(line.Split(',')[1], NumberStyles.HexNumber))
            .ToArray();
        var (game, runtime) = CreateGame(romPath, seed, inputs);
        RoomLevelData level = runtime.LevelData ?? throw new InvalidDataException(
            "The floor-clip fixture did not retain its native room collision layer.");
        RoomCollisionBlock authoredFloor = level.GetCollisionBlockByIndex(
            M.FloorBlockY * level.WidthInBlocks + M.FloorBlockX);
        if (authoredFloor.CollisionType != RoomCollisionType.ShootableBlock ||
            !authoredFloor.Bts.RequiresPowerBombReaction)
        {
            throw new InvalidDataException(
                "The floor-clip fixture no longer targets the cartridge's shootable " +
                $"power-bomb floor block: {authoredFloor.CollisionType}/{authoredFloor.Bts}.");
        }

        int comparedCheckpoints = 1;
        AssertCheckpoint(game, runtime, seed, SeedFrame);
        for (int frame = SeedFrame; frame < FinalFrame; frame++)
        {
            game.StepCaptured(inputs[frame], frame, 1);
            if (TryReadCheckpoint(directory, frame + 1) is byte[] checkpoint)
            {
                AssertCheckpoint(game, runtime, checkpoint, frame + 1);
                comparedCheckpoints++;
            }
        }

        byte[] final = ReadCheckpoint(directory, FinalFrame);
        if (game.GameState != SuperMetroidGameState.MainGameplay ||
            runtime.Samus!.Kinematics.YFixed != ReadFixed(final, M.Y, M.YFraction))
        {
            throw new InvalidDataException(
                "The floor-clip sequence did not finish in live gameplay at the native " +
                $"crossed-floor position ${ReadFixed(final, M.Y, M.YFraction):X8}.");
        }

        // One pixel above the movie's exact setup is a faithful adjacent control: it runs
        // the same room, BTS, pause/hurt transition, and controller samples without any
        // collision bypass. Native alignment is setup-dependent rather than a global floor
        // disable, so this neighboring start must be stopped by the authored floor.
        var (adjacentGame, adjacentRuntime) = CreateGame(romPath, seed, inputs);
        adjacentRuntime.Samus!.YPosition--;
        for (int frame = SeedFrame; frame < FinalFrame; frame++)
            adjacentGame.StepCaptured(inputs[frame], frame, 2);
        if (adjacentRuntime.Samus.YPosition >= M.FloorTopY)
        {
            throw new InvalidDataException(
                "The one-pixel-higher adjacent setup incorrectly crossed the authored " +
                $"floor: Y=${adjacentRuntime.Samus.Kinematics.YFixed:X8}.");
        }

        Console.WriteLine(
            $"PASS: {comparedCheckpoints} native Green Brinstar reserve/pause floor-clip " +
            "checkpoints through frame 5200 match production room, pose, exact 16.16 " +
            "position, and vertical trajectory; the exact setup crosses its authored " +
            "shootable/BTS floor while the one-pixel-higher control remains above it.");
        return 0;
    }

    private static (SuperMetroidGame Game, SuperMetroidRuntime Runtime) CreateGame(
        string romPath,
        byte[] seed,
        ushort[] inputs)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        SuperMetroidRuntime runtime = CreateRuntime(bus, seed);
        var game = new SuperMetroidGame(bus);
        typeof(SuperMetroidGame).GetField("runtime", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(game, runtime);
        typeof(SuperMetroidGame).GetProperty(nameof(SuperMetroidGame.GameState))!
            .SetValue(game, (SuperMetroidGameState)ReadWord(seed, M.GameState));
        typeof(SuperMetroidGame).GetField("pauseBrightness", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(game, (byte)0);
        typeof(SuperMetroidGame).GetField("pauseFadeCounter", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(game, 1);
        typeof(SuperMetroidRuntime).GetProperty(nameof(SuperMetroidRuntime.NmiFrameCounter8))!
            .SetValue(runtime, seed[M.NmiFrameCounter8]);
        typeof(SuperMetroidRuntime).GetProperty(nameof(SuperMetroidRuntime.NmiFrameCounter))!
            .SetValue(runtime, ReadWord(seed, M.NmiFrameCounter));
        runtime.Controller1.Latch(inputs[SeedFrame - 1]);
        return (game, runtime);
    }

    private static SuperMetroidRuntime CreateRuntime(SuperMetroidAddressSpace bus, byte[] memory)
    {
        ushort W(int address) => ReadWord(memory, address);
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.System.LoadCollectedItemBytes(
            memory.AsSpan(M.CollectedItemBits, Bank80SystemState.ItemBitByteCount));
        runtime.LoadCartridgeRoomForDebug(W(M.Room), W(M.CameraX), W(M.CameraY));

        RoomLevelData level = runtime.LevelData ?? throw new InvalidDataException(
            "The native floor-clip checkpoint did not load room collision data.");
        for (int index = 0; index < level.WidthInBlocks * level.HeightInBlocks; index++)
        {
            level.SetForegroundEntry(index, W(M.Level + index * sizeof(ushort)));
            level.SetBehavior(index, memory[M.Bts + index]);
        }

        SamusState samus = runtime.Samus ?? throw new InvalidDataException(
            "The native floor-clip checkpoint did not load Samus.");
        samus.InputLocked = false;
        samus.EquippedItems = W(M.Items);
        samus.CollectedItems = W(M.CollectedItems);
        samus.EquippedBeams = W(M.Beams);
        samus.CollectedBeams = W(M.CollectedBeams);
        samus.Health = W(M.Health);
        samus.MaxHealth = W(M.MaxHealth);
        samus.ReserveTankMode = W(M.ReserveMode);
        samus.MaxReserveEnergy = W(M.MaxReserve);
        samus.ReserveEnergy = W(M.Reserve);
        samus.Pose = (byte)W(M.Pose);
        samus.XPosition = W(M.X);
        samus.YPosition = W(M.Y);
        samus.Kinematics.XSubposition = W(M.XFraction);
        samus.Kinematics.YSubposition = W(M.YFraction);
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.SetAnimationFrameFromSpecialHandler(W(M.Animation), W(M.AnimationTimer));
        samus.PoseHistory.PreviousPose = W(M.PreviousPose);
        samus.PoseHistory.PreviousDirectionAndMovement = W(M.PreviousDirection);
        samus.PoseHistory.LastDifferentPose = W(M.LastDifferentPose);
        samus.PoseHistory.LastDifferentDirectionAndMovement = W(M.LastDifferentDirection);
        samus.HorizontalSpeed.BaseSpeed = W(M.BaseSpeed);
        samus.HorizontalSpeed.BaseSubspeed = W(M.BaseFraction);
        samus.HorizontalSpeed.ExtraRunSpeed = W(M.ExtraSpeed);
        samus.HorizontalSpeed.ExtraRunSubspeed = W(M.ExtraFraction);
        samus.HorizontalSpeed.AccelerationMode = W(M.AccelerationMode);
        samus.HorizontalSpeed.HasRunningMomentum = W(M.Momentum) != 0;
        samus.HorizontalSpeed.SpeedBoostCounter = W(M.BoostCounter);
        // The clip's decisive turn-completion frame overlaps the last tick of a hurt
        // request. Native changes animation command three to command eight, installing
        // the turn's target pose and then applying the shared knockback-finished bottom
        // alignment. Omitting these words turns the probe into a different, healthy jump.
        samus.HurtFlashCounter = W(M.HurtFlashCounter);
        samus.KnockbackDirection = W(M.KnockbackDirection);
        samus.KnockbackXDirection = W(M.KnockbackXDirection);
        samus.InvincibilityTimer = W(M.InvincibilityTimer);
        samus.KnockbackTimer = W(M.KnockbackTimer);
        samus.KnockbackActive = false;
        samus.Kinematics.YSpeed = W(M.VerticalSpeed);
        samus.Kinematics.YSubspeed = W(M.VerticalFraction);
        samus.Kinematics.YDirection = W(M.VerticalDirection);
        return runtime;
    }

    private static void AssertCheckpoint(
        SuperMetroidGame game,
        SuperMetroidRuntime runtime,
        byte[] expected,
        int frame)
    {
        SamusState samus = runtime.Samus ?? throw new InvalidDataException(
            $"Native floor-clip frame {frame} has no live Samus state.");
        string actual =
            $"room={runtime.ActiveRoom?.Pointer ?? 0:X4}," +
            $"pose={samus.Pose:X2}," +
            $"x={samus.Kinematics.XFixed:X8},y={samus.Kinematics.YFixed:X8}," +
            $"yspeed={samus.Kinematics.YSpeed:X4}{samus.Kinematics.YSubspeed:X4}," +
            $"ydir={samus.Kinematics.YDirection:X4}";
        string wanted =
            $"room={ReadWord(expected, M.Room):X4}," +
            $"pose={(byte)ReadWord(expected, M.Pose):X2}," +
            $"x={ReadFixed(expected, M.X, M.XFraction):X8}," +
            $"y={ReadFixed(expected, M.Y, M.YFraction):X8}," +
            $"yspeed={ReadWord(expected, M.VerticalSpeed):X4}{ReadWord(expected, M.VerticalFraction):X4}," +
            $"ydir={ReadWord(expected, M.VerticalDirection):X4}";
        if (actual != wanted)
            throw new InvalidDataException(
                $"Floor-clip frame {frame} differs.\nNative: {wanted}\nPort:   {actual}");
    }

    private static byte[] ReadCheckpoint(string directory, int frame) =>
        TryReadCheckpoint(directory, frame) ?? throw new FileNotFoundException(
            $"No native floor-clip checkpoint exists for frame {frame} under {directory}.");

    private static byte[]? TryReadCheckpoint(string directory, int frame)
    {
        foreach (string set in new[] { "green-crossing-every-frame", "green-sparse-finish" })
        {
            string path = Path.Combine(directory, set, $"frame-{frame}.wram");
            if (!File.Exists(path))
                continue;
            byte[] memory = File.ReadAllBytes(path);
            if (memory.Length != SuperMetroidAddressSpace.WorkRamByteCount)
                throw new InvalidDataException($"Expected complete Snes9x WRAM capture {path}.");
            return memory;
        }
        return null;
    }

    private static ushort ReadWord(ReadOnlySpan<byte> memory, int address) =>
        BinaryPrimitives.ReadUInt16LittleEndian(memory[address..]);

    private static uint ReadFixed(ReadOnlySpan<byte> memory, int whole, int fraction) =>
        (uint)ReadWord(memory, whole) << 16 | ReadWord(memory, fraction);
}
