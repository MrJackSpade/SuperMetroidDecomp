using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static MapPresentationVerification;
using static SuiteTimingVerification;

internal static class MotherBrainFakeDeathPaletteVerification
{
    [CpuBusBoundary("exact bus test: retail ROM oracle for the installed Mother Brain fake-death palette")]
    internal static void VerifyMotherBrainFakeDeathPalette(
        ISnesAddressSpace rom, MotherBrainRainbowPalettePresentation installed)
    {
        Suite(nameof(VerifyFade), () => VerifyFade(toGrey: true));
        Suite(nameof(VerifyFade), () => VerifyFade(toGrey: false));

        void VerifyFade(bool toGrey)
        {
            var expected = new SnesCgram();
            var actual = new SnesCgram();
            var enemies = CreateFakeDeathPaletteEnemy(installed, actual);
            var state = new MotherBrainEnemyState(enemies.SlotTable.Slots[0])
            {
                Function = toGrey
                    ? MotherBrainBodyFunction.FakeDeathDescentFadeToGray
                    : MotherBrainBodyFunction.FakeDeathAscentTransitionFromGray,
                TubeCollapseFunction = MotherBrainTubeCollapseFunction.Finished,
                FakeDeathExplosionTimer = 100,
            };
            Action<MotherBrainEnemyState> step = toGrey
                ? enemies.MotherBrainBehavior.RunMotherBrainFadeToGray
                : enemies.MotherBrainBehavior.TransitionMotherBrainFromGray;
            int table = toGrey ? MotherBrainFakeDeathPaletteRomData.ToGreyPointerTable :
                MotherBrainFakeDeathPaletteRomData.FromGreyPointerTable;
            for (int frame = 0; frame < MotherBrainRainbowPaletteFormat.GreyFrameCount; frame++)
            {
                int pointerAddress = table + frame * sizeof(ushort);
                ushort pointer = (ushort)(SnesLittleEndian.Word(CpuBusRegionRead.Read(rom, pointerAddress), CpuBusRegionRead.Read(rom, pointerAddress + 1)));
                AssertTrue(pointer != 0, "native Mother Brain fake-death palette frame exists");
                SuperMetroid.AssetExtraction.CartridgePaletteImporter.LoadToCgram(expected, rom, MotherBrainRainbowPaletteRomData.SourceBank | pointer,
                    MotherBrainFakeDeathPaletteRomData.ColorCount,
                    MotherBrainFakeDeathPaletteRomData.BrainColor);
                state.FunctionTimer = 0;
                step(state);
                AssertTrue(new CgramPalettes(expected).ImageEquals(new CgramPalettes(actual)),
                    $"installed Mother Brain fake-death {(toGrey ? "descent" : "ascent")} frame {frame} matches full native CGRAM");
                AssertEqual((ushort)(frame + 1), toGrey ? state.GrayFadeIndex : state.GrayTransitionCounter,
                    "native grey fade cursor increments once per selected frame");
                AssertEqual(toGrey ? (ushort)8 : (ushort)4, state.FunctionTimer,
                    "installed color frame preserves native function timer");
            }

            // The ninth selection is the native zero-terminator, not an editable color.
            state.FunctionTimer = 0;
            step(state);
            AssertTrue(new CgramPalettes(expected).ImageEquals(new CgramPalettes(actual)),
                "fake-death grey terminator leaves every CGRAM word unchanged");
            AssertEqual(toGrey ? MotherBrainBodyFunction.FakeDeathDescentCollapseTubes :
                MotherBrainBodyFunction.SecondPhaseStretchingShakeHead, state.Function,
                "fake-death grey terminator preserves the native phase transition");
        }
    }

    private static RoomEnemySystem CreateFakeDeathPaletteEnemy(
        MotherBrainRainbowPalettePresentation colors, SnesCgram cgram)
    {
        var enemies = new RoomEnemySystem { Presentation = { MotherBrainRainbowColors = colors } };
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        enemies.BindFixtureBus(NewPaletteReadForbiddenBus());
        enemies.Presentation._cgram = cgram;
        return enemies;
    }

    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    internal static (Bgr555[] Colors, ushort FunctionTimer, MotherBrainBodyFunction Function)
        CaptureFakeDeathPaletteFrame(MotherBrainRainbowPalettePresentation colors, bool toGrey)
    {
        var cgram = new SnesCgram();
        var enemies = CreateFakeDeathPaletteEnemy(colors, cgram);
        var state = new MotherBrainEnemyState(enemies.SlotTable.Slots[0])
        {
            Function = toGrey
                ? MotherBrainBodyFunction.FakeDeathDescentFadeToGray
                : MotherBrainBodyFunction.FakeDeathAscentTransitionFromGray,
            TubeCollapseFunction = MotherBrainTubeCollapseFunction.Finished,
            FakeDeathExplosionTimer = 100,
        };
        Action<MotherBrainEnemyState> step = toGrey
            ? enemies.MotherBrainBehavior.RunMotherBrainFadeToGray
            : enemies.MotherBrainBehavior.TransitionMotherBrainFromGray;
        step(state);
        return (cgram.Colors.ToArray(), state.FunctionTimer, state.Function);
    }
}
