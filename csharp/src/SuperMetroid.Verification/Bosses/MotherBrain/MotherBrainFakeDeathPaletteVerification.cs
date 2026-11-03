using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.CartridgeTables;

using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;
namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>Checks Mother Brain's fake-death gray fades against native full-CGRAM frames.</summary>
internal static class MotherBrainFakeDeathPaletteVerification
{
    /// <summary>Compares both descent and ascent color tables, timers, and zero-terminator transitions.</summary>
    internal static void VerifyMotherBrainFakeDeathPalette(
        ISnesAddressSpace rom, MotherBrainRainbowPalettePresentation installed)
    {
        Suite(nameof(VerifyFade), () => VerifyFade(toGrey: true));
        Suite(nameof(VerifyFade), () => VerifyFade(toGrey: false));

        void VerifyFade(bool toGrey)
        {
            var expected = new SnesCgram();
            var enemies = CreateFakeDeathPaletteEnemy(installed);
            var state = new MotherBrainEnemyState(enemies.SlotTable.Slots[0])
            {
                Function = toGrey
                    ? MotherBrainBodyFunction.FakeDeathDescentFadeToGray
                    : MotherBrainBodyFunction.FakeDeathAscentTransitionFromGray,
                TubeCollapseFunction = MotherBrainTubeCollapseFunction.Finished,
                FakeDeathExplosionTimer = WrappingCountdown.Start(100),
            };
            Action<MotherBrainEnemyState> step = toGrey
                ? enemies.MotherBrainBehavior.FakeDeath.RunMotherBrainFadeToGray
                : MotherBrainFakeDeath.TransitionMotherBrainFromGray;
            int table = toGrey ? MotherBrainSourceDefinitions.Palettes.FakeDeathToGreyPointerTable :
                MotherBrainSourceDefinitions.Palettes.RevivalFromGreyPointerTable;
            for (int frame = 0; frame < MotherBrainRainbowPaletteGeometry.GreyFrameCount; frame++)
            {
                int pointerAddress = table + frame * sizeof(ushort);
                ushort pointer = LittleEndianBytes.Word(CpuBusRegionRead.Read(rom, pointerAddress), CpuBusRegionRead.Read(rom, pointerAddress + 1));
                AssertTrue(pointer != 0, "native Mother Brain fake-death palette frame exists");
                SuperMetroid.AssetExtraction.Cartridge.CartridgePaletteImporter.LoadToCgram(expected, rom, MotherBrainSourceDefinitions.Palettes.PaletteBank | pointer,
                    MotherBrainFakeDeathPaletteDefinitions.ColorCount,
                    MotherBrainColorDestinations.BrainColor);
                state.FunctionTimer = WrappingCountdown.Start(0);
                step(state);
                AssertTrue(new CgramPalettes(expected).ImageEquals(new CgramPalettes(Shown(state, installed))),
                    $"installed Mother Brain fake-death {(toGrey ? "descent" : "ascent")} frame {frame} matches full native CGRAM");
                AssertEqual((ushort)(frame + 1), toGrey ? state.GrayFadeIndex : state.GrayTransitionCounter,
                    "native grey fade cursor increments once per selected frame");
                AssertEqual(toGrey ? (ushort)8 : (ushort)4, state.FunctionTimer.RawValue,
                    "installed color frame preserves native function timer");
            }

            // The ninth selection is the native zero-terminator, not an editable color.
            state.FunctionTimer = WrappingCountdown.Start(0);
            step(state);
            AssertTrue(new CgramPalettes(expected).ImageEquals(new CgramPalettes(Shown(state, installed))),
                "fake-death grey terminator leaves every CGRAM word unchanged");
            AssertEqual(toGrey ? MotherBrainBodyFunction.FakeDeathDescentCollapseTubes :
                MotherBrainBodyFunction.SecondPhaseStretchingShakeHead, state.Function,
                "fake-death grey terminator preserves the native phase transition");
        }
    }

    /// <summary>Creates a Mother Brain actor with installed colors.</summary>
    private static RoomEnemySystem CreateFakeDeathPaletteEnemy(MotherBrainRainbowPalettePresentation colors) =>
        new(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance) { BossArtworkBinding = { MotherBrainRainbowColors = colors } };

    /// <summary>
    /// The colours Mother Brain's sprites show over black: the import's shades of <paramref name="colors"/>, laid as her
    /// fades laid them on <paramref name="state"/> (#1512).
    /// </summary>
    private static SnesCgram Shown(MotherBrainEnemyState state, MotherBrainRainbowPalettePresentation colors)
    {
        ushort[] words = BossShadeColours.Laid(state.Colours.Sprites.Laid, SuperMetroid.AssetExtraction.Bosses.MotherBrainSheetShades.RainbowSprites(colors),
            BossShadeColours.Sprites, 0);
        var cgram = new SnesCgram();
        for (int colour = 0; colour < SnesCgram.ColorCount; colour++)
            cgram.SetColor(colour, SuperMetroid.Core.Assets.RgbColor.FromWord(words[colour]));
        return cgram;
    }

    /// <summary>Captures one production fake-death fade update for palette and state comparisons.</summary>
    internal static CaptureFakeDeathPaletteFrameResult
        CaptureFakeDeathPaletteFrame(MotherBrainRainbowPalettePresentation colors, bool toGrey)
    {
        var enemies = CreateFakeDeathPaletteEnemy(colors);
        var state = new MotherBrainEnemyState(enemies.SlotTable.Slots[0])
        {
            Function = toGrey
                ? MotherBrainBodyFunction.FakeDeathDescentFadeToGray
                : MotherBrainBodyFunction.FakeDeathAscentTransitionFromGray,
            TubeCollapseFunction = MotherBrainTubeCollapseFunction.Finished,
            FakeDeathExplosionTimer = WrappingCountdown.Start(100),
        };
        Action<MotherBrainEnemyState> step = toGrey
            ? enemies.MotherBrainBehavior.FakeDeath.RunMotherBrainFadeToGray
            : MotherBrainFakeDeath.TransitionMotherBrainFromGray;
        step(state);
        return new CaptureFakeDeathPaletteFrameResult(Shown(state, colors).Colors.ToArray(), state.FunctionTimer.RawValue, state.Function);
    }
}
