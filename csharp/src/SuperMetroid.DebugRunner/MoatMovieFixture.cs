using System.Buffers.Binary;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using M = MoatMovieMemory;

/// <summary>Shared native-checkpoint loader for the player-supplied Moat CWJ movie.</summary>
internal static class MoatMovieFixture
{
    private const string MovieSha256 =
        "90CDD95DC88845972FEA9CFDBF636D6443C6A825FA9FE0A52A9F2E8648681D39";

    public static ushort[] LoadInputs(string directory)
    {
        VerifyMovie(directory);
        return File.ReadLines(Path.Combine(directory, "inputs.csv"))
            .Skip(1)
            .Select(line => ushort.Parse(
                line.Split(',')[1],
                System.Globalization.NumberStyles.HexNumber))
            .ToArray();
    }

    public static byte[] LoadCheckpoint(
        string directory,
        string fileName,
        string expectedSha256)
    {
        byte[] memory = File.ReadAllBytes(Path.Combine(directory, fileName));
        if (memory.Length != SuperMetroidAddressSpace.WorkRamByteCount ||
            Convert.ToHexString(SHA256.HashData(memory)) != expectedSha256)
        {
            throw new InvalidDataException(
                $"Expected complete Snes9x WRAM capture {fileName} for the supplied movie.");
        }

        return memory;
    }

    public static SuperMetroidRuntime CreateRuntime(
        SuperMetroidAddressSpace bus,
        byte[] memory)
    {
        ushort W(int address) => BinaryPrimitives.ReadUInt16LittleEndian(memory.AsSpan(address));
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.System.LoadCollectedItemBytes(
            memory.AsSpan(M.CollectedItemBits, Bank80SystemState.ItemBitByteCount));
        runtime.LoadCartridgeRoomForDebug(W(M.Room), W(M.CameraX), W(M.CameraY));

        // Preserve the native room's already-mutated doors and item blocks. Rebuilding
        // these from the pristine room header would no longer represent this movie frame.
        RoomLevelData level = runtime.LevelData ?? throw new InvalidDataException(
            "The native CWJ checkpoint did not load room collision data.");
        for (int index = 0; index < level.WidthInBlocks * level.HeightInBlocks; index++)
        {
            level.SetForegroundEntry(index, W(M.Level + index * sizeof(ushort)));
            level.SetBehavior(index, memory[M.Bts + index]);
        }

        SamusState samus = runtime.Samus ?? throw new InvalidDataException(
            "The native CWJ checkpoint did not load Samus.");
        samus.InputLocked = false;
        samus.EquippedItems = W(M.Items);
        samus.EquippedBeams = W(M.Beams);
        samus.Health = W(M.Health);
        samus.MaxHealth = W(M.MaxHealth);
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
        samus.Kinematics.YSpeed = W(M.VerticalSpeed);
        samus.Kinematics.YSubspeed = W(M.VerticalFraction);
        samus.Kinematics.YDirection = W(M.VerticalDirection);
        return runtime;
    }

    public static ushort ReadWord(ReadOnlySpan<byte> memory, int address) =>
        BinaryPrimitives.ReadUInt16LittleEndian(memory[address..]);

    private static void VerifyMovie(string directory)
    {
        string moviePath = Path.Combine(directory, "cwj.smv");
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(moviePath))) != MovieSha256)
        {
            throw new InvalidDataException(
                "Use the player-supplied CWJ movie, not a recaptured or retimed input sequence.");
        }
    }
}
