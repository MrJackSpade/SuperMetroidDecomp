using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Bosses.Phantoon;

/// <summary>
/// Confirms #1413's Phantoon migration: each catalog that left its cartridge encoding still produces, for one member of
/// each kind, what the deleted catalog did, and the import's sources and native words are the cartridge's.
/// </summary>
internal static class PhantoonDefinitionMigrationVerification
{
    /// <summary>Compares the new definitions, sources and native words with the values the deleted catalogs held.</summary>
    internal static void VerifyPhantoonDefinitionMigration()
    {
        // Colours: the health counts stay in Core; the sources and the power-on's cartridge colour count are the import's.
        AssertEqual(8, PhantoonColorDefinitions.HealthBandCount, "health band count");
        AssertEqual(16, PhantoonColorDefinitions.HealthBandColorCount, "health band colours");
        AssertEqual(112, PhantoonSourceDefinitions.PowerOnColourCount, "power-on colours");
        AssertEqual(0xa7cb41, PhantoonSourceDefinitions.HealthBandColors, "health band source");
        AssertEqual(0xa7ca41, PhantoonSourceDefinitions.FadeOutColors, "fade-out source");
        AssertEqual(0xa7ca61, PhantoonSourceDefinitions.PowerOnColors, "power-on source");

        // The wave: the seed two before the cycle's start leaves every first remainder the cartridge's $FFFE seed does.
        for (int step = 0; step <= ushort.MaxValue / 2; step++)
            AssertEqual((0xfffe + 2 * step) & 0x1ff, PhantoonWaveTable.PhaseInCycle(PhantoonWaveDefinitions.InitialPhase + 2 * step),
                $"wave seed's first remainder after a step of {step}");
        AssertEqual(2, PhantoonWaveDefinitions.IntroMode, "intro wave mode");
        AssertEqual(0x8b, PhantoonWaveDefinitions.TrailingByte, "byte after the sine samples");

        // Flame motion and rage keep their values under domain names.
        AssertEqual(2, PhantoonFlameMotionDefinitions.RageAngleStep, "rage angle step");
        AssertEqual(4, PhantoonFlameMotionDefinitions.RageRadiusStep, "rage radius step");
        AssertEqual(128, PhantoonRageDefinitions.WaveInterval, "rage wave interval");
        AssertEqual(8, PhantoonRageDefinitions.WaveCount, "rage wave count");

        // Motion: each signed value is the word the cartridge held.
        AssertEqual(0xfffe, unchecked((ushort)PhantoonMotionDefinitions.ReverseSlowCap), "reverse slow cap word");
        AssertEqual(0xfff9, unchecked((ushort)PhantoonMotionDefinitions.ReverseFastCap), "reverse fast cap word");
        AssertEqual(0xf801, unchecked((ushort)PhantoonMotionDefinitions.SwoopNegativeXThreshold), "horizontal swoop threshold word");
        AssertEqual(0xfa01, unchecked((ushort)PhantoonMotionDefinitions.SwoopNegativeYThreshold), "vertical swoop threshold word");
        AssertEqual(0xffc0, unchecked((ushort)PhantoonMotionDefinitions.SwoopMinimumX), "swoop minimum X word");
        AssertEqual(0xffff, unchecked((ushort)PhantoonCasualFlameDefinitions.NoActiveSchedule), "no-schedule flame count word");

        // The death mosaic's sizes are the MOSAIC values the cartridge writes.
        AssertEqual(0x02, PhantoonNativeWords.DeathMosaicWord(PhantoonDeathDefinitions.InitialMosaicSize), "first death mosaic word");
        AssertEqual(0xf2, PhantoonNativeWords.DeathMosaicWord(PhantoonDeathDefinitions.FinalMosaicSize), "last death mosaic word");

        // The AI functions: every phase has its routine word.
        foreach (PhantoonAiFunction function in Enum.GetValues<PhantoonAiFunction>())
            _ = PhantoonNativeWords.AiFunctions.Word(function);
        AssertEqual(0xd4a9, PhantoonNativeWords.AiFunctions.Word(PhantoonAiFunction.SpawnStartingFlames), "first phase's routine");
        AssertEqual(0xdb3d, PhantoonNativeWords.AiFunctions.Word(PhantoonAiFunction.Dead), "dead phase's routine");
        Console.WriteLine("Phantoon definition migration: colours, wave, flame, motion, mosaic and phase identities keep their values.");
    }

    /// <summary>Runs the focused Phantoon definition-migration check.</summary>
    [VerificationCommand("--phantoon-definition-migration")]
    internal static int PhantoonDefinitionMigrationCommand()
    {
        VerifyPhantoonDefinitionMigration();
        return 0;
    }
}
