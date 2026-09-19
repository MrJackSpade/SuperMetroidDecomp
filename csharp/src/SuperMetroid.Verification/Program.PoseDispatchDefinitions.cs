using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyPoseDispatchDefinitions(SuperMetroidAddressSpace rom)
    {
        var forbidden = new SlopeHeightNoReadBus();
        var samus = new SamusState();
        var commit = typeof(SamusState).GetMethod("CommitPoseHistory",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .CreateDelegate<Action<SamusState, ISnesAddressSpace>>();
        for (int pose = 0; pose <= byte.MaxValue; pose++)
        {
            int address = SamusMovementRomData.Poses.Definitions + pose * SamusMovementRomData.Poses.DefinitionByteCount;
            byte facing = rom.ReadByte(address), movement = rom.ReadByte(address + 1), fallback = rom.ReadByte(address + 2);
            ISnesAddressSpace source = forbidden;
            samus.Pose = (byte)pose;
            byte aim = rom.ReadByte(address + 3);
            AssertEqual(aim, samus.ReadShotDirection(source), "Live aim and restrictions retain the full native byte without authored ROM reads");
            AssertEqual(aim, SamusState.ReadShotDirection(source, (byte)pose), "Prospective aim retains native byte and adjacent indexes");
            AssertEqual(facing, samus.ReadPoseXDirection(source), "Live pose facing retains native value");
            AssertEqual(facing, SamusState.ReadPoseXDirection(source, (byte)pose), "Prospective pose facing retains native value");
            AssertEqual(fallback, samus.ReadNoInputFallbackPose(source), "No-input fallback retains native pose or sentinel");
            if (movement <= (byte)SamusMovementType.Special)
            {
                AssertEqual(movement, (byte)samus.ReadMovementType(source), "Live movement discriminator matches native");
                AssertEqual(movement, (byte)SamusState.ReadMovementType(source, (byte)pose), "Prospective movement discriminator matches native");
                commit(samus, source);
                AssertEqual((ushort)(facing | movement << 8), samus.PoseHistory.PreviousDirectionAndMovement,
                    "Actual history publisher retains native facing/movement word");
            }
            else
                AssertThrows<InvalidDataException>(() => samus.ReadMovementType(source), "Adjacent invalid movement remains a loud error");
        }

        // These three compiled tables are queried by ordinary movement, rendering, and
        // no-input transition paths. A collection-expression property can look immutable
        // while still materializing a backing object on every access under a particular
        // compiler/runtime combination, so guard the actual production API rather than
        // assuming its source declaration is allocation-free.
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            byte pose = unchecked((byte)(index % 253));
            checksum += SamusState.ReadPoseXDirection(forbidden, pose);
            checksum += (byte)SamusState.ReadMovementType(forbidden, pose);
            checksum += SamusPoseDispatchDefinitions.ReadNoInputPose(pose);
        }
        long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        AssertTrue(checksum != 0, "Pose-dispatch allocation probe consumes live table values");
        AssertEqual(0L, allocated,
            "Warmed pose-dispatch lookups allocate no per-frame table storage");

        Console.WriteLine("Pose dispatch: all 256 facing/movement/fallback/aim byte indexes match native with all ROM reads forbidden, including three adjacent-code records; warmed production lookups allocate nothing.");
    }
}
