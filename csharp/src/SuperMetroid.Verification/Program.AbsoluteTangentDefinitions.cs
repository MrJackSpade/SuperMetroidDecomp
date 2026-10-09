using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    /// <summary>Checks the compiled tangent table against its native words and verifies production consumers avoid runtime table reads.</summary>
    /// <param name="rom">Cartridge address space used to read the reference tangent words.</param>
    /// <param name="definitionsOnly">When <see langword="true"/>, limits checks to the compiled definition and its bounds.</param>
    private static void VerifyCompiledAbsoluteTangent(SuperMetroidAddressSpace rom, bool definitionsOnly = false)
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
        if (definitionsOnly)
        {
            foreach (int invalid in new[] { int.MinValue, -1, 129, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => AbsoluteTangentDefinitions.Sample(invalid), "Tangent invalid index rejected");
            return;
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
        var beam = new MotherBrainRainbowBeamHdmaState
        {
            PresentationColors = RetailPresentationFixture().MotherBrainRainbowPalette,
        };
        foreach (byte angle in new byte[] { 0, 32, 64, 96, 128, 160, 192, 224 })
            beam.Step(guard, true, 100, 95, SnesAngle.FromTableIndex(angle), 512);
        AssertThrows<ArgumentOutOfRangeException>(() => AbsoluteTangentDefinitions.Sample(129), "Tangent beyond native endpoint rejected");
        Console.WriteLine("Absolute tangent: 129 native words, all direct caller endpoints, 768 X-ray directions and 520 window builds reject runtime tangent reads.");
    }

    /// <summary>Wraps an address space and rejects runtime reads from the absolute-tangent table.</summary>
    /// <param name="source">Underlying address space for allowed reads and all writes.</param>
    private sealed class TangentReadGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes cartridge-source reads through the guarded runtime-read path.</summary>
        /// <param name="address">Address requested by the importer.</param>
        /// <returns>The byte returned by the underlying address space, unless the tangent table is forbidden.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads from the tangent-table range and forwards other reads to the wrapped address space.</summary>
        /// <param name="address">Runtime address to read.</param>
        /// <returns>The byte at an allowed address.</returns>
        /// <exception cref="InvalidOperationException">The runtime attempts to read the absolute-tangent table.</exception>
        public byte ReadByte(int address) => address is >= 0x91c9d4 and < 0x91cad6
            ? throw new InvalidOperationException("Runtime absolute-tangent ROM read.") : source.ReadByte(address);

        /// <summary>Forwards a write unchanged to the wrapped address space.</summary>
        /// <param name="address">Address to write.</param>
        /// <param name="value">Byte to store.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
