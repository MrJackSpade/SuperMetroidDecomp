using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain;
using SuperMetroid.AssetExtraction.CartridgeTables;

using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Verification.Bosses;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Checks Zebetite's eight-frame two-color pulse against native
/// CGRAM updates while preserving its runtime gates and counter.</summary>
internal static class EnemyTileArtworkZebetiteColorsVerification
{
    /// <summary>The colour the Zebetites' pulse starts at: the second enemy row's colour twelve.</summary>
    private const int PulseFirstColour = 0xac;

    /// <summary>The place the room's population loads the primary half in.</summary>
    private const int PrimaryPlace = 0;

    /// <summary>The place the room's population loads the linked half in.</summary>
    private const int LinkedPlace = 1;

    /// <summary>Compares live pulse frames with cartridge colors, then checks the engine gates and malformed asset rejection.</summary>
    internal static void VerifyInstalledZebetiteColors(ISnesAddressSpace bus,
        string stockDirectory, ImportEnemyArtwork stock)
    {
        if (stock.Bosses.ZebetiteColors is null)
            throw new InvalidDataException("Installed enemy artwork lacks Zebetite pulse colors.");
        SnesCgram nativeCgram = SeedColors();
        ushort nativeCounter = 0;
        CartridgeReadGuard guard = new CartridgeReadGuard(bus).Forbid(
            address => address is >= MotherBrainSourceDefinitions.Zebetite.PulseColors and
                < (MotherBrainSourceDefinitions.Zebetite.PulseColors +
                    ZebetiteColorFormat.FrameCount * ZebetiteColorFormat.ColorsPerFrame *
                    sizeof(ushort)),
            address => $"Installed Zebetite pulse read migrated ROM color ${address:X6}.");
        RoomEnemySystem installed = Create(guard, stock, new SharedFadeStepCount());
        for (int call = 0; call < ZebetiteColorFormat.FrameCount * 2; call++)
        {
            ReferenceCycle(nativeCgram, ref nativeCounter);
            Cycle(installed);
            AssertEqual((call + 1) % ZebetitePulse.All.Count,
                Counter(installed),
                "Installed Zebetite pulse retains the shared counter and wrap");
            AssertEqual(nativeCounter, Counter(installed),
                "Installed Zebetite pulse keeps native frame selection");
            AssertTrue(new CgramPalettes(nativeCgram).ImageEquals(new CgramPalettes(Shown(installed, stock))),
                "Installed Zebetite pulse preserves full native CGRAM and neighbors");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "Installed Zebetite pulse never reads migrated ROM colors");

        byte[] stockBytes = File.ReadAllBytes(Path.Combine(stockDirectory, ZebetiteColorFile.FileName));
        // A door's fade into the room runs the room's enemies before each of its steps on the shared fade count (#1724):
        // the pulse steps on the fade's first update, holds while the fade is under way and steps again once it completes.
        var fadeStepCount = new SharedFadeStepCount();
        RoomEnemySystem selected = Create(guard, stock, fadeStepCount);
        var doorFade = new GradualFadeSteps(DoorTransitionColourDefinitions.FadeLastStep, fadeStepCount);
        int counterBeforeGate = Counter(selected);
        Cycle(selected);
        AssertEqual((counterBeforeGate + 1) % ZebetitePulse.All.Count, Counter(selected),
            "the Zebetite pulse steps on a door fade's first update, before the fade takes its first step");
        AssertTrue(!doorFade.Step(), "the door fade takes its first step");
        counterBeforeGate = Counter(selected);
        do
            Cycle(selected);
        while (!doorFade.Step());
        AssertEqual(counterBeforeGate, Counter(selected), "the Zebetite pulse holds through the rest of the door fade");
        Cycle(selected);
        AssertEqual((counterBeforeGate + 1) % ZebetitePulse.All.Count, Counter(selected),
            "the Zebetite pulse steps again once the door fade completes");
        counterBeforeGate = Counter(selected);
        // The linked half's update skips the pulse.
        RunActive(selected, LinkedPlace);
        AssertEqual(counterBeforeGate, Counter(selected),
            "Zebetite secondary-half gate remains an engine-owned condition");

        var invalid = RequiredJson.Parse(stockBytes);
        invalid.Required("frames").Required(0).Required(0)["green"] = 32;
        AssertThrows<InvalidDataException>(() => ZebetiteColorFile.Read(
            new MemoryStream(System.Text.Encoding.UTF8.GetBytes(invalid.ToJsonString()))),
            "Out-of-range Zebetite RGB5 component rejected");
        Console.WriteLine("  Zebetite colors: 16 native RGB5 words, sixteen live pulse calls, full CGRAM, ROM guard, gates and channel rejection pass.");

        static RoomEnemySystem Create(
            ISnesAddressSpace source, ImportEnemyArtwork artwork, SharedFadeStepCount fadeStepCount)
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(artwork.Enemies),
                RoomEnemyCompatibilityFixtures.ForNewRoom(fadeStepCount, new UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance, installedBosses: artwork.Bosses);
            // The room's population load sets up a primary half, which steps the pulse, and a linked half, which does not,
            // in their first generation: no Zebetite has been destroyed.
            enemies.VerificationServices().ReadZebetitesDestroyed = () => 0;
            _ = enemies.LoadPopulation(
                PopulationPlacementAccess.Entry(EnemyDefinitionId.Zebetite, new ZebetiteSpawnSettings(ZebetiteHalf.Primary)),
                PopulationPlacementAccess.Entry(EnemyDefinitionId.Zebetite, new ZebetiteSpawnSettings(ZebetiteHalf.Linked)));
            return enemies;
        }

        // The import's shade of pulse frame <frame> among <artwork>'s Zebetite shades (#1512).
        static SheetShade Pulse(ImportEnemyArtwork artwork, int frame) =>
            MotherBrainSheetShades.Zebetites(artwork.Bosses).Single(shade => shade.Shade == ZebetiteShades.Pulse(ZebetitePulse.All[frame]));

        // The colours the Zebetites show over the seeded colours: the pulse frame last shown, as the import pre-coloured it.
        static SnesCgram Shown(RoomEnemySystem enemies, ImportEnemyArtwork artwork)
        {
            SnesCgram cgram = SeedColors();
            if (enemies.ZebetiteBehavior.PulseShown is { } shown)
            {
                SheetShade pulse = Pulse(artwork, shown.Step);
                foreach ((int ink, RgbColor colour) in pulse.InkColours)
                    cgram.SetColor(BossShadeColours.Sprites + ink, colour);
            }
            return cgram;
        }

        static int Counter(RoomEnemySystem enemies) => enemies.ZebetiteBehavior.SharedPulse.Entry.Step;

        // Independent $A6:FD5E pulse: advance modulo eight, copy two raw colors.
        void ReferenceCycle(SnesCgram cgram, ref ushort counter)
        {
            counter = (ushort)((counter + 1) & 7);
            for (int color = 0; color < 2; color++)
            {
                int address = 0xa6fd87 + counter * 4 + color * 2;
                cgram.SetColor(PulseFirstColour + color,
                    RgbColor.FromWord(bus.ReadCartridgeWord(address)));
            }
        }

        static SnesCgram SeedColors()
        {
            var cgram = new SnesCgram();
            for (int color = 0; color < SnesCgram.ColorCount; color++)
                cgram.SetColor(color, RgbColor.FromWord((ushort)(color * 31)));
            return cgram;
        }

        // One update of the primary half's active main AI, which steps the pulse.
        static void Cycle(RoomEnemySystem enemies) => RunActive(enemies, PrimaryPlace);

        // One update of the active main AI of the Zebetite half in <place>, at full health, which it keeps.
        static void RunActive(RoomEnemySystem enemies, int place)
        {
            ZebetiteEntity zebetite = enemies.Places.InUpdateOrder[place].EntityOf<ZebetiteEntity>();
            zebetite.State.Action = ZebetiteAction.Active;
            enemies.ZebetiteBehavior.RunZebetiteMainAi(zebetite.Occupant, zebetite.State);
        }
    }
}
