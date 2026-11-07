using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>
/// Checks the rows the import places Mother Brain's encounter's enemies in (#1512) against their initializations on the
/// cartridge: her brain selects row one, her body clears its palette bits, the Shitroid and the Zebetites select row two,
/// the Baby's cutscene row seven, and a Rinka row two; and that the import refuses a sheet the graphics-set data places in
/// another row than the one her encounter's shades recolour.
/// </summary>
internal static class MotherBrainSheetRowsVerification
{
    /// <summary>Checks each row and the refusal of a disagreeing one.</summary>
    internal static int VerifyMotherBrainSheetRows()
    {
        var rows = new GraphicsSetRows(RoomEnemyGraphicsSetLists.MembersOf(RoomEnemyGraphicsSetDefinitions.MotherBrain01), EnemyGraphicsSetPaletteRowDefinitions.Rows);
        AssertEqual(1, rows.RowOf(EnemyDefinitionId.MotherBrainHead), "Mother Brain's brain draws in row one");
        AssertEqual(0, rows.RowOf(EnemyDefinitionId.MotherBrainBody), "Mother Brain's body draws in the rows its parts name");
        AssertEqual(2, rows.RowOf(EnemyDefinitionId.BabyMetroid), "The Shitroid draws in row two");
        AssertEqual(2, rows.RowOf(EnemyDefinitionId.Zebetite), "The Zebetites draw in row two");
        AssertEqual(7, rows.RowOf(EnemyDefinitionId.BabyMetroidCutscene), "The Baby's cutscene draws in row seven");
        AssertEqual(2, rows.RowOf(EnemyDefinitionId.Rinka), "A Rinka in Mother Brain's room draws in row two");
        foreach (EnemyDefinitionId enemy in new[] { EnemyDefinitionId.MotherBrainHead, EnemyDefinitionId.MotherBrainBody,
                     EnemyDefinitionId.BabyMetroid, EnemyDefinitionId.Zebetite, EnemyDefinitionId.BabyMetroidCutscene })
        {
            int row = rows.RowOf(enemy);
            MotherBrainSheetShades.RequireDrawRow(enemy, row);
            AssertThrows<InvalidDataException>(() => MotherBrainSheetShades.RequireDrawRow(enemy, (row + 1) % 8),
                $"A sheet placing {enemy} in another row than her encounter writes is refused");
        }
        Console.WriteLine("Mother Brain sheet rows: her encounter's enemies draw in the rows their initializations select, and a disagreeing row is refused.");
        return 0;
    }

    /// <summary>Runs the focused Mother Brain sheet-row check.</summary>
    [VerificationCommand("--mother-brain-sheet-rows")]
    internal static int MotherBrainSheetRowsCommand() => VerifyMotherBrainSheetRows();
}
