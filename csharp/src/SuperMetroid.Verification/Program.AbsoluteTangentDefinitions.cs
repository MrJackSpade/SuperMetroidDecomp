using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    private static void VerifyCompiledAbsoluteTangent(SuperMetroidAddressSpace rom)
    {
        int Native(int index) => rom.ReadByte(0x91c9d4 + index * 2) | rom.ReadByte(0x91c9d5 + index * 2) << 8;
        var guard = new TangentReadGuard(rom);
        var eye = typeof(EyeBeamWindowBuilder).GetMethod("Tangent", BindingFlags.NonPublic | BindingFlags.Static)!
            .CreateDelegate<Func<ISnesAddressSpace, int, int>>();
        var mother = typeof(MotherBrainRainbowBeamHdmaState).GetMethod("Tangent", BindingFlags.NonPublic | BindingFlags.Static)!
            .CreateDelegate<Func<ISnesAddressSpace, int, int>>();
        for (int index = 0; index <= 128; index++)
        {
            AssertEqual((ushort)Native(index), AbsoluteTangentDefinitions.Sample(index), "All native tangent words including endpoint");
            AssertEqual(Native(index), eye(guard, index), "Eye actual tangent reader without ROM");
            AssertEqual(Native(index), mother(guard, index), "Mother Brain actual tangent reader without ROM");
            AssertEqual(Native(index), mother(guard, index - 256), "Mother Brain caller byte wrap");
        }
        var direction = typeof(SnesGameplayFrameRenderer).GetMethod("ReadXrayDirection", BindingFlags.NonPublic | BindingFlags.Static)!;
        for (int angle = -256; angle < 512; angle++)
        {
            int wrapped = angle & 255;
            int x, y;
            if (wrapped is 64 or 192) { x = wrapped == 64 ? 256 : -256; y = 0; }
            else
            {
                int tangent = Native(wrapped & 127);
                x = wrapped < 128 ? tangent : -tangent;
                y = wrapped < 64 || wrapped >= 192 ? -256 : 256;
            }
            object actual = direction.Invoke(null, [guard, angle])!;
            AssertEqual(x, (int)actual.GetType().GetProperty("X")!.GetValue(actual)!, "X-ray wrapped/cardinal X direction");
            AssertEqual(y, (int)actual.GetType().GetProperty("Y")!.GetValue(actual)!, "X-ray wrapped/cardinal Y direction");
        }
        // Production window builders must run without tangent-table reads, including inclusive angle 256.
        for (int angle = 0; angle < 256; angle++)
        {
            _ = EyeBeamWindowBuilder.Build(guard, 100, 100, angle, 0);
            _ = EyeBeamWindowBuilder.Build(guard, -16, 100, angle, 1);
        }
        var beam = new MotherBrainRainbowBeamHdmaState();
        foreach (byte angle in new byte[] { 0, 32, 64, 96, 128, 160, 192, 224 })
            beam.Step(guard, true, 100, 95, SnesAngle.FromTableIndex(angle), 512);
        AssertThrows<ArgumentOutOfRangeException>(() => AbsoluteTangentDefinitions.Sample(129), "Tangent beyond native endpoint rejected");
        Console.WriteLine("Absolute tangent: 129 native words, all direct caller endpoints, 768 X-ray directions and 520 window builds reject runtime tangent reads.");
    }

    private sealed class TangentReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address) => address is >= 0x91c9d4 and < 0x91cad6
            ? throw new InvalidOperationException("Runtime absolute-tangent ROM read.") : source.ReadByte(address);
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
