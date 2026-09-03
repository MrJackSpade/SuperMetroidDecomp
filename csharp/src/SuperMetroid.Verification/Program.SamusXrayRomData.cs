using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Validates the shared X-ray palette and tangent ranges against the private retail
    /// cartridge. The behavioral verifier separately exercises standing, crouching,
    /// turning, visor cycling, restoration, and the visible window produced by the table.
    /// </summary>
    static void VerifySamusXrayRomData()
    {
        byte[] angleBands =
        [
            SamusXrayRomData.AnimationAngles.RightFrameOne,
            SamusXrayRomData.AnimationAngles.RightFrameTwo,
            SamusXrayRomData.AnimationAngles.RightFrameThree,
            SamusXrayRomData.AnimationAngles.RightFrameFour,
            SamusXrayRomData.AnimationAngles.LeftFrameFour,
            SamusXrayRomData.AnimationAngles.LeftFrameThree,
            SamusXrayRomData.AnimationAngles.LeftFrameTwo,
            SamusXrayRomData.AnimationAngles.LeftFrameOne,
        ];
        for (int index = 1; index < angleBands.Length; index++)
        {
            AssertTrue(angleBands[index - 1] < angleBands[index],
                $"X-ray animation boundary {index} follows its predecessor");
        }

        AssertEqual(SnesAngle.HalfTurn.TableIndex + 1,
            SamusXrayRomData.Window.AbsoluteTangentWordCount,
            "X-ray absolute tangent table includes both half-turn endpoints");
        AssertEqual(SamusXrayRomData.Palette.SamusCgramIndex + 4,
            SamusXrayRomData.Palette.VisorCgramIndex,
            "X-ray visor occupies Samus palette color four");

        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine("  Samus X-ray ROM data: retail range audit skipped (private ROM absent).");
            return;
        }

        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        TouchRange(bus, SamusXrayRomData.Palette.VisorWords,
            SamusXrayRomData.Palette.FullCycleEndWordOffset,
            "X-ray visor palette words");
        TouchRange(bus, SamusXrayRomData.Palette.NormalSuitPointers,
            3 * sizeof(ushort), "X-ray normal-suit palette pointers");
        TouchRange(bus, SamusXrayRomData.Window.AbsoluteTangentTable,
            SamusXrayRomData.Window.AbsoluteTangentWordCount * sizeof(ushort),
            "X-ray absolute tangent table");

        Console.WriteLine(
            "  Samus X-ray ROM data: animation bands, visor palettes, and all 129 tangent words are in range.");
    }
}
