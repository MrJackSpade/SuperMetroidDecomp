using SuperMetroid.Core.Enemies;
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
                TubeCollapse = new MotherBrainTubeCollapseProgress(Tube: null, MotherBrainTubeDropStage.Standing),
            };
            state.FakeDeathExplosionTimer.LastFor(101);
            if (toGrey)
                state.DescentPhase = MotherBrainDescentPhase.FadeToGray;
            else
                state.AscentPhase = MotherBrainAscentPhase.TransitionFromGray;
            Action<MotherBrainEnemyState> step = toGrey
                ? enemies.MotherBrainBehavior.FakeDeath.RunMotherBrainFadeToGray
                : MotherBrainFakeDeath.TransitionMotherBrainFromGray;
            int table = toGrey ? MotherBrainSourceDefinitions.Palettes.FakeDeathToGreyPointerTable :
                MotherBrainSourceDefinitions.Palettes.RevivalFromGreyPointerTable;
            MotherBrainFadeProgress fade = toGrey ? state.FakeDeathGreying : state.FromFakeDeathGrey;
            // Each image holds as the native function timer reloaded at each step lasts (#2404): eight below the hold to
            // grey, four below the hold back. The fresh fade shows its first image on its first update.
            int hold = (toGrey ? 8 : 4) + 1;
            AssertEqual(hold, fade.Fade.ImageUpdates, "each image of the fade holds as long as the native function timer");
            for (int frame = 0; frame < MotherBrainRainbowPaletteGeometry.GreyFrameCount; frame++)
            {
                int pointerAddress = table + frame * sizeof(ushort);
                ushort pointer = LittleEndianBytes.Word(CpuBusRegionRead.Read(rom, pointerAddress), CpuBusRegionRead.Read(rom, pointerAddress + 1));
                AssertTrue(pointer != 0, "native Mother Brain fake-death palette frame exists");
                SuperMetroid.AssetExtraction.Cartridge.CartridgePaletteImporter.LoadToCgram(expected, rom, MotherBrainSourceDefinitions.Palettes.PaletteBank | pointer,
                    MotherBrainRainbowPaletteGeometry.FakeDeathColors,
                    MotherBrainColorDestinations.BrainColor);
                for (int update = 1; update < (frame == 0 ? 1 : hold); update++)
                {
                    step(state);
                    AssertEqual(frame - 1, fade.Shown?.Step, $"the fade holds image {frame - 1} on its update {update}");
                }
                step(state);
                AssertTrue(new CgramPalettes(expected).ImageEquals(new CgramPalettes(Shown(state, installed))),
                    $"installed Mother Brain fake-death {(toGrey ? "descent" : "ascent")} frame {frame} matches full native CGRAM");
                AssertEqual(frame, fade.Shown?.Step, "the fade moves on to its next image once the image before has held");
            }

            // The update after the last image's hold is the native zero-terminator, not an editable color.
            for (int update = 1; update < hold; update++)
            {
                step(state);
                AssertTrue(!fade.IsOver, $"the fade holds its last image on its update {update}");
            }
            step(state);
            AssertTrue(fade.IsOver, "the fade is over once its last image has held");
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
