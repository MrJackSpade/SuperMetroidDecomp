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
            ISnesAddressSpace source = pose <= 0xfc ? forbidden : rom;
            samus.Pose = (byte)pose;
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
        Console.WriteLine("Pose dispatch: all 253 authored facing/movement/fallback records match native with all ROM reads forbidden; trailing indexes retain adjacent-data behavior.");
    }
}
