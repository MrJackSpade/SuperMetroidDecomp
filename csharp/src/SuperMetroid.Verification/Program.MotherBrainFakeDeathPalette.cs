using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Verifies both Mother Brain fake-death grayscale fades against every native CGRAM palette frame,
    /// including the zero-pointer terminator's preserved colors and phase transition.
    /// </summary>
    /// <param name="rom">The address space supplying the native grayscale palette pointer tables and colors.</param>
    /// <param name="installed">The parsed palette presentation installed into the enemy system.</param>
    private static void VerifyMotherBrainFakeDeathPalette(
        ISnesAddressSpace rom, MotherBrainRainbowPalettePresentation installed)
    {
        Suite(nameof(VerifyFade), () => VerifyFade(toGrey: true));
        Suite(nameof(VerifyFade), () => VerifyFade(toGrey: false));

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
                SuperMetroid.AssetExtraction.CartridgePaletteImporter.LoadToCgram(expected, rom, MotherBrainRainbowPaletteRomData.SourceBank | pointer,
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

    /// <summary>
    /// Creates an enemy system with the supplied fake-death palette and isolated CGRAM, while rejecting bus palette reads.
    /// </summary>
    /// <param name="colors">The palette presentation used by the fake-death fade routines.</param>
    /// <param name="cgram">The CGRAM buffer that receives palette writes.</param>
    /// <returns>An enemy system fixture whose palette path is independent of cartridge bus reads.</returns>
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

    /// <summary>Runs one fake-death fade step and captures its resulting CGRAM words, timer, and phase.</summary>
    /// <param name="colors">The palette presentation consumed by the selected fade routine.</param>
    /// <param name="toGrey">Selects the descent fade toward gray when true, or the ascent transition from gray when false.</param>
    /// <returns>The copied CGRAM contents, resulting function timer, and resulting Mother Brain body phase.</returns>
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
