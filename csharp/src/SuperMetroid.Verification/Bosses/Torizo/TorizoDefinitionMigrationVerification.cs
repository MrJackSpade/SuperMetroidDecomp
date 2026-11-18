using SuperMetroid.AssetExtraction.Bosses.Torizo;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Samus;

namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>
/// Confirms #1413's Torizo migration: each catalog that left its cartridge encoding still produces, for one member of
/// each kind, what the deleted catalog did, and the import's sources and native words are the cartridge's.
/// </summary>
internal static class TorizoDefinitionMigrationVerification
{
    /// <summary>Compares the new definitions, sources and native words with the values the deleted catalogs held.</summary>
    internal static void VerifyTorizoDefinitionMigration()
    {
        AssertEqual(0xc6ab, TorizoNativeWords.Phases.Word(TorizoPhase.Idle), "idle function routine");
        AssertEqual(0xd5e6, TorizoNativeWords.Phases.Word(TorizoPhase.GoldenActive), "golden normal movement routine");
        AssertEqual(TorizoCueKind.StopMoving, TorizoNativeWords.MotionCues.Decode(0xc95e, "idle movement routine"), "idle movement routine");

        // The latches sit where the cartridge's parameter words keep them.
        AssertEqual(0xb234, TorizoParameterWords.ToParameterWord(new TorizoPosture(HorizontalDirection.Right, LeadingFoot: HorizontalDirection.Left), 0x1234),
            "posture word facing right, opposite leg forward");
        AssertEqual(new TorizoPosture(HorizontalDirection.Right, LeadingFoot: HorizontalDirection.Left).TurningAround,
            TorizoParameterFlagWords.PostureOf(0xe123), "posture of an authored word");
        AssertEqual(0x0123, TorizoParameterFlagWords.PostureLowBitsOf(0xe123), "low bits of an authored word");

        // The random decisions are taken by exactly the samples the cartridge's masks leave clear (#1632), so they read the
        // masks' bits (#2479).
        static void AssertMaskedDecision(int mask, RandomClearBitsChance decision, string name)
        {
            int mismatches = 0;
            for (int sample = 0; sample <= ushort.MaxValue; sample++)
                mismatches += decision.IsTakenBy(sample) == ((sample & mask) == 0) ? 0 : 1;
            AssertEqual(0, mismatches, $"{name} is taken by every sample its mask leaves clear and no other");
        }
        AssertMaskedDecision(0x8142, TorizoRandomDefinitions.LowHealthDrool, "low-health drool");
        AssertMaskedDecision(0x0102, TorizoRandomDefinitions.LowHealthEggs, "low-health eggs");
        AssertMaskedDecision(0x0110, TorizoRandomDefinitions.MediumRangeAttack, "medium-range attack");
        AssertMaskedDecision(0x0101, TorizoRandomDefinitions.ForwardLeap, "forward leap");

        // The code's inventory is the cartridge's two immediates. The items immediate also sets the bit that records no
        // item, which #1778 deleted and the import's codec drops, so the typed items are compared with its decoding (#1884).
        AssertEqual(SamusEquipmentWordCodec.FromEquipmentWord(0xf337), GoldenTorizoCodeDefinitions.Items, "code items");
        AssertEqual(0x100f, GoldenTorizoCodeDefinitions.Beams.ToEquipmentWord(), "code beams word");

        // The tile pages keep their sizes; their sources are the import's.
        AssertEqual(0x0040, TorizoSwapArtworkDefinitions.SharedDeath.ByteCount, "shared death page size");
        AssertEqual(0x0600, TorizoSwapArtworkDefinitions.GoldenAwakening.ByteCount, "golden awakening page size");
        AssertEqual(0xaab0a5, TorizoSourceDefinitions.PageSources[TorizoTilePage.SharedDeath], "shared death page source");
        AssertEqual(0xadb200, TorizoSourceDefinitions.PageSources[TorizoTilePage.ChozoDebris], "Chozo debris page source");
        foreach (TorizoTileSheet page in TorizoSwapArtworkDefinitions.All)
            AssertTrue(TorizoSourceDefinitions.PageSources.ContainsKey(page.Page), $"{page.Page} has a cartridge source");
        Console.WriteLine("Torizo definition migration: routine identities, latches, random masks, code inventory and tile pages keep their values.");
    }

    /// <summary>Runs the focused Torizo definition-migration check.</summary>
    [VerificationCommand("--torizo-definition-migration")]
    internal static int TorizoDefinitionMigrationCommand()
    {
        VerifyTorizoDefinitionMigration();
        return 0;
    }
}
