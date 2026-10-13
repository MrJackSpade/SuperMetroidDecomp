using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling;
using static SuiteTimingVerification;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using static RetailPresentationFixtureVerification;

/// <summary>Checks the compiled absolute tangent table through X-Ray and beam-window callers.</summary>
internal static class AbsoluteTangentDefinitionsVerification
{
    /// <summary>Compares 129 native samples and wrapped caller directions while forbidding live tangent reads.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for compiled absolute tangent")]
    internal static void VerifyCompiledAbsoluteTangent(SuperMetroidAddressSpace rom, bool definitionsOnly = false)
    {
        var guard = NewTangentReadGuard(rom);
        var eye = typeof(EyeBeamWindowBuilder).GetMethod("Tangent", BindingFlags.NonPublic | BindingFlags.Static)!
            .CreateDelegate<Func<int, int>>();
        var mother = typeof(MotherBrainRainbowBeamHdmaState).GetMethod("Tangent", BindingFlags.NonPublic | BindingFlags.Static)!
            .CreateDelegate<Func<int, int>>();
        for (int index = 0; index <= 128; index++)
        {
            AssertEqual((ushort)CartridgeImportSource.Require(rom).ReadCartridgeWord(0x91c9d4 + index * 2), AbsoluteTangentDefinitions.Sample(index), "All native tangent words including endpoint");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0x91c9d4 + index * 2), eye(index), "Eye actual tangent reader without ROM");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0x91c9d4 + index * 2), mother(index), "Mother Brain actual tangent reader without ROM");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0x91c9d4 + index * 2), mother(index - 256), "Mother Brain caller byte wrap");
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
                int tangent = CartridgeImportSource.Require(rom).ReadCartridgeWord(0x91c9d4 + (wrapped & 127) * 2);
                x = wrapped < 128 ? tangent : -tangent;
                y = wrapped is < 64 or >= 192 ? -256 : 256;
            }
            var actual = (Offset16<ScreenPixels>)direction.Invoke(null, [angle])!;
            AssertEqual(x, (int)actual.X, "X-ray wrapped/cardinal X direction");
            AssertEqual(y, (int)actual.Y, "X-ray wrapped/cardinal Y direction");
        }
        // Production window builders must run without tangent-table reads, including inclusive angle 256.
        for (int angle = 0; angle < 256; angle++)
        {
            _ = EyeBeamWindowBuilder.Build(new Offset16<ScreenPixels>(100, 100), angle, 0);
            _ = EyeBeamWindowBuilder.Build(new Offset16<ScreenPixels>(-16, 100), angle, 1);
        }
        var beam = new MotherBrainRainbowBeamHdmaState
        {
            PresentationColors = RetailPresentationFixture().MotherBrainRainbowPalette,
        };
        foreach (byte angle in new byte[] { 0, 32, 64, 96, 128, 160, 192, 224 })
            beam.Step(guard, true, new(100, 95), SnesAngle.FromTableIndex(angle), 512);
        AssertThrows<ArgumentOutOfRangeException>(() => AbsoluteTangentDefinitions.Sample(129), "Tangent beyond native endpoint rejected");
        Console.WriteLine("Absolute tangent: 129 native words, all direct caller endpoints, 768 X-ray directions and 520 window builds reject runtime tangent reads.");
    }

    /// <summary>Rejects runtime reads of the migrated bank-$91 tangent table.</summary>
    private static CartridgeReadGuard NewTangentReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address => address is >= 0x91c9d4 and < 0x91cad6,
            _ => "Runtime absolute-tangent ROM read.");

    /// <summary>Runs the tangent table and production caller comparison.</summary>
    [VerificationCommand("--absolute-tangent-runtime")]
    internal static int AbsoluteTangentRuntimeCommand()
    {
        Suite(nameof(VerifyCompiledAbsoluteTangent), () => VerifyCompiledAbsoluteTangent(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
