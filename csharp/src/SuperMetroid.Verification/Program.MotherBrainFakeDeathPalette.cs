using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyMotherBrainFakeDeathPalette(
        ISnesAddressSpace rom, MotherBrainRainbowPalettePresentation installed)
    {
        VerifyFade(toGrey: true);
        VerifyFade(toGrey: false);

        void VerifyFade(bool toGrey)
        {
            var expected = new SnesCgram();
            var actual = new SnesCgram();
            var enemies = CreateFakeDeathPaletteEnemy(installed, actual);
            var state = new MotherBrainEnemyState(enemies.Slots[0])
            {
                Function = toGrey
                    ? MotherBrainBodyFunction.FakeDeathDescentFadeToGray
                    : MotherBrainBodyFunction.FakeDeathAscentTransitionFromGray,
                TubeCollapseFunction = MotherBrainTubeCollapseFunction.Finished,
                FakeDeathExplosionTimer = 100,
            };
            MethodInfo step = typeof(RoomEnemySystem).GetMethod(
                toGrey ? "RunMotherBrainFadeToGray" : "TransitionMotherBrainFromGray",
                BindingFlags.Instance | BindingFlags.NonPublic)!;
            int table = toGrey ? MotherBrainFakeDeathPaletteRomData.ToGreyPointerTable :
                MotherBrainFakeDeathPaletteRomData.FromGreyPointerTable;
            for (int frame = 0; frame < MotherBrainRainbowPaletteFormat.GreyFrameCount; frame++)
            {
                int pointerAddress = table + frame * sizeof(ushort);
                ushort pointer = (ushort)(rom.ReadByte(pointerAddress) |
                    rom.ReadByte(pointerAddress + 1) << 8);
                AssertTrue(pointer != 0, "native Mother Brain fake-death palette frame exists");
                expected.LoadFromBus(rom, MotherBrainRainbowPaletteRomData.SourceBank | pointer,
                    MotherBrainFakeDeathPaletteRomData.ColorCount,
                    MotherBrainFakeDeathPaletteRomData.BrainColor);
                state.FunctionTimer = 0;
                step.Invoke(enemies, [state]);
                AssertTrue(expected.Colors.SequenceEqual(actual.Colors),
                    $"installed Mother Brain fake-death {(toGrey ? "descent" : "ascent")} frame {frame} matches full native CGRAM");
                AssertEqual((ushort)(frame + 1), toGrey ? state.GrayFadeIndex : state.GrayTransitionCounter,
                    "native grey fade cursor increments once per selected frame");
                AssertEqual(toGrey ? (ushort)8 : (ushort)4, state.FunctionTimer,
                    "installed color frame preserves native function timer");
            }

            // The ninth selection is the native zero-terminator, not an editable color.
            state.FunctionTimer = 0;
            step.Invoke(enemies, [state]);
            AssertTrue(expected.Colors.SequenceEqual(actual.Colors),
                "fake-death grey terminator leaves every CGRAM word unchanged");
            AssertEqual(toGrey ? MotherBrainBodyFunction.FakeDeathDescentCollapseTubes :
                MotherBrainBodyFunction.SecondPhaseStretchingShakeHead, state.Function,
                "fake-death grey terminator preserves the native phase transition");
        }
    }

    private static RoomEnemySystem CreateFakeDeathPaletteEnemy(
        MotherBrainRainbowPalettePresentation colors, SnesCgram cgram)
    {
        var enemies = new RoomEnemySystem { MotherBrainRainbowColors = colors };
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies,
            new PaletteReadForbiddenBus());
        typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(enemies, cgram);
        return enemies;
    }

    private static (ushort[] Colors, ushort FunctionTimer, MotherBrainBodyFunction Function)
        CaptureFakeDeathPaletteFrame(MotherBrainRainbowPalettePresentation colors, bool toGrey)
    {
        var cgram = new SnesCgram();
        var enemies = CreateFakeDeathPaletteEnemy(colors, cgram);
        var state = new MotherBrainEnemyState(enemies.Slots[0])
        {
            Function = toGrey
                ? MotherBrainBodyFunction.FakeDeathDescentFadeToGray
                : MotherBrainBodyFunction.FakeDeathAscentTransitionFromGray,
            TubeCollapseFunction = MotherBrainTubeCollapseFunction.Finished,
            FakeDeathExplosionTimer = 100,
        };
        typeof(RoomEnemySystem).GetMethod(
            toGrey ? "RunMotherBrainFadeToGray" : "TransitionMotherBrainFromGray",
            BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(enemies, [state]);
        return (cgram.Colors.ToArray(), state.FunctionTimer, state.Function);
    }
}
