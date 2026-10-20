using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>Checks Mother Brain's fake-death gray fades against native full-CGRAM frames.</summary>
internal static class MotherBrainFakeDeathPaletteVerification
{
    /// <summary>Compares both descent and ascent color tables, timers, and zero-terminator transitions.</summary>
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
                FakeDeathExplosionTimer = NativeCountdown.Start(100),
            };
            Action<MotherBrainEnemyState> step = toGrey
                ? enemies.MotherBrainBehavior.FakeDeath.RunMotherBrainFadeToGray
                : enemies.MotherBrainBehavior.FakeDeath.TransitionMotherBrainFromGray;
            int table = toGrey ? MotherBrainFakeDeathPaletteRomData.ToGreyPointerTable :
                MotherBrainFakeDeathPaletteRomData.FromGreyPointerTable;
            for (int frame = 0; frame < MotherBrainRainbowPaletteFormat.GreyFrameCount; frame++)
            {
                int pointerAddress = table + frame * sizeof(ushort);
                ushort pointer = SnesLittleEndian.Word(CpuBusRegionRead.Read(rom, pointerAddress), CpuBusRegionRead.Read(rom, pointerAddress + 1));
                AssertTrue(pointer != 0, "native Mother Brain fake-death palette frame exists");
                SuperMetroid.AssetExtraction.Cartridge.CartridgePaletteImporter.LoadToCgram(expected, rom, MotherBrainRainbowPaletteRomData.SourceBank | pointer,
                    MotherBrainFakeDeathPaletteRomData.ColorCount,
                    MotherBrainFakeDeathPaletteRomData.BrainColor);
                state.FunctionTimer = NativeCountdown.Start(0);
                step(state);
                AssertTrue(new CgramPalettes(expected).ImageEquals(new CgramPalettes(actual)),
                    $"installed Mother Brain fake-death {(toGrey ? "descent" : "ascent")} frame {frame} matches full native CGRAM");
                AssertEqual((ushort)(frame + 1), toGrey ? state.GrayFadeIndex : state.GrayTransitionCounter,
                    "native grey fade cursor increments once per selected frame");
                AssertEqual(toGrey ? (ushort)8 : (ushort)4, state.FunctionTimer.RawValue,
                    "installed color frame preserves native function timer");
            }

            // The ninth selection is the native zero-terminator, not an editable color.
            state.FunctionTimer = NativeCountdown.Start(0);
            step(state);
            AssertTrue(new CgramPalettes(expected).ImageEquals(new CgramPalettes(actual)),
                "fake-death grey terminator leaves every CGRAM word unchanged");
            AssertEqual(toGrey ? MotherBrainBodyFunction.FakeDeathDescentCollapseTubes :
                MotherBrainBodyFunction.SecondPhaseStretchingShakeHead, state.Function,
                "fake-death grey terminator preserves the native phase transition");
        }
    }

    /// <summary>Creates a Mother Brain actor with installed colors and a bus that forbids palette ROM reads.</summary>
    private static RoomEnemySystem CreateFakeDeathPaletteEnemy(
        MotherBrainRainbowPalettePresentation colors, SnesCgram cgram)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { Presentation = { MotherBrainRainbowColors = colors } };
        enemies.Presentation.BindCgram(cgram);
        return enemies;
    }

    /// <summary>Captures one production fake-death fade update for palette and state comparisons.</summary>
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
            FakeDeathExplosionTimer = NativeCountdown.Start(100),
        };
        Action<MotherBrainEnemyState> step = toGrey
            ? enemies.MotherBrainBehavior.FakeDeath.RunMotherBrainFadeToGray
            : enemies.MotherBrainBehavior.FakeDeath.TransitionMotherBrainFromGray;
        step(state);
        return (cgram.Colors.ToArray(), state.FunctionTimer.RawValue, state.Function);
    }
}
