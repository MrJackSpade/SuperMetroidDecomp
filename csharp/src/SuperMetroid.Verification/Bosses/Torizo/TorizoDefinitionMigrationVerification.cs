using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Verification.Harness;

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
        AssertEqual(0xc6ab, TorizoNativeWords.Functions.Word(TorizoFunction.Idle), "idle function routine");
        AssertEqual(0xd5e6, TorizoNativeWords.Functions.Word(TorizoFunction.GoldenPreInstruction), "golden normal movement routine");
        AssertEqual(0xc95e, TorizoNativeWords.PreInstructions.Word(TorizoPreInstruction.Idle), "idle movement routine");

        // The latches sit where the cartridge's parameter words keep them.
        AssertEqual(0xb234, TorizoParameterFlagCodec.ToParameterWord(TorizoPostureFlags.FacingRight | TorizoPostureFlags.OppositeLegForward, 0x1234),
            "posture word facing right, opposite leg forward");
        AssertEqual(TorizoPostureFlags.OppositeLegForward | TorizoPostureFlags.TurningAround | TorizoPostureFlags.FacingRight,
            TorizoParameterFlagCodec.PostureOf(0xe123), "posture of an authored word");
        AssertEqual(0x0123, TorizoParameterFlagCodec.PostureLowBitsOf(0xe123), "low bits of an authored word");
        AssertEqual(TorizoBehaviorFlags.CaughtSuperMissile, TorizoParameterFlagCodec.BehaviorOf(0x1000), "caught Super Missile bit");
        AssertEqual(TorizoBehaviorFlags.GutBlownUp | TorizoBehaviorFlags.FaceBlownUp, TorizoParameterFlagCodec.BehaviorOf(0xc00f), "blown-up bits");

        // The random decisions test the masks the cartridge does.
        AssertEqual(0x8142, TorizoRandomDefinitions.LowHealthDrool.Mask, "low-health drool mask");
        AssertEqual(0x0102, TorizoRandomDefinitions.LowHealthCall.Mask, "low-health call mask");
        AssertEqual(0x0110, TorizoRandomDefinitions.MediumRangeAttack.Mask, "medium-range attack mask");
        AssertEqual(0x0101, TorizoRandomDefinitions.ForwardLeap.Mask, "forward leap mask");

        // The code's inventory is the cartridge's two immediates.
        AssertEqual(0xf337, GoldenTorizoCodeDefinitions.Items.ToEquipmentWord(), "code items word");
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
