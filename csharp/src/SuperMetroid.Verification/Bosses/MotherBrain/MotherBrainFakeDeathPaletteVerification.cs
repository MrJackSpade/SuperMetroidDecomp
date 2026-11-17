using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.CartridgeTables;

using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>Checks Mother Brain's fake-death gray fades against native full-CGRAM frames.</summary>
internal static class MotherBrainFakeDeathPaletteVerification
{
    /// <summary>Compares both descent and ascent color tables, timers, and zero-terminator transitions.</summary>
    internal static void VerifyMotherBrainFakeDeathPalette(
        ISnesAddressSpace rom, InstalledMotherBrainRainbowPalette installed)
    {
        Suite(nameof(VerifyFade), () => VerifyFade(toGrey: true));
        Suite(nameof(VerifyFade), () => VerifyFade(toGrey: false));

        void VerifyFade(bool toGrey)
        {
            var expected = new SnesCgram();
            var enemies = CreateFakeDeathPaletteEnemy();
            var state = new MotherBrainEnemyState(enemies.Places.InUpdateOrder[0], enemies.Places.InUpdateOrder[1], new MotherBrainWalkCounter(), new MotherBrainBurstCycle(), new MotherBrainBrainAnimation())
            {
                TubeCollapseStep = MotherBrainTubeCollapseStep.Finished,
                FakeDeathExplosionTimer = { Count = 100 },
            };
            if (toGrey)
                state.DescentPhase = MotherBrainDescentPhase.FadeToGray;
            else
                state.AscentPhase = MotherBrainAscentPhase.TransitionFromGray;
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
                    MotherBrainRainbowPaletteGeometry.FakeDeathColors,
                    MotherBrainColorDestinations.BrainColor);
                state.PhaseTimer.Count = 0;
                step(state);
                AssertTrue(new CgramPalettes(expected).ImageEquals(new CgramPalettes(Shown(state, installed))),
                    $"installed Mother Brain fake-death {(toGrey ? "descent" : "ascent")} frame {frame} matches full native CGRAM");
                AssertEqual(frame + 1, toGrey ? state.FakeDeathGreying.Steps : state.FromFakeDeathGrey.Steps,
                    "native grey fade cursor increments once per selected frame");
                AssertEqual(toGrey ? 8 : 4, state.PhaseTimer.Count,
                    "installed color frame preserves native function timer");
            }

            // The ninth selection is the native zero-terminator, not an editable color.
            state.PhaseTimer.Count = 0;
            step(state);
            AssertTrue(new CgramPalettes(expected).ImageEquals(new CgramPalettes(Shown(state, installed))),
                "fake-death grey terminator leaves every CGRAM word unchanged");
            AssertTrue(toGrey
                    ? state.Stage == MotherBrainBodyStage.FakeDeathDescent && state.DescentPhase == MotherBrainDescentPhase.CollapseTubes
                    : state.Stage == MotherBrainBodyStage.Phase2Fight && state.Phase2FightPhase == MotherBrainPhase2FightPhase.StretchingShakeHead,
                "fake-death grey terminator preserves the native phase transition");
        }
    }

    /// <summary>Creates a Mother Brain actor with her installed beam colours.</summary>
    private static RoomEnemySystem CreateFakeDeathPaletteEnemy()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        return enemies;
    }

    /// <summary>
    /// The colours Mother Brain's sprites show over black: the import's shades of <paramref name="colors"/>, laid as her
    /// fades laid them on <paramref name="state"/> (#1512).
    /// </summary>
    private static SnesCgram Shown(MotherBrainEnemyState state, InstalledMotherBrainRainbowPalette colors)
    {
        ushort[] words = BossShadeColours.Laid(state.Colours.OwnShading.Shades, SuperMetroid.AssetExtraction.Bosses.MotherBrainSheetShades.RainbowSprites(colors),
            BossShadeColours.Sprites, 0);
        var cgram = new SnesCgram();
        for (int colour = 0; colour < SnesCgram.ColorCount; colour++)
            cgram.SetColor(colour, SuperMetroid.Core.Assets.RgbColor.FromWord(words[colour]));
        return cgram;
    }
}
