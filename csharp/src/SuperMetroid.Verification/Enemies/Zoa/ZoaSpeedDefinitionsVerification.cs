using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Zoa;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.Zoa;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks compiled Zoa velocity windows and shooting movement against cartridge words.</summary>
internal static class ZoaSpeedDefinitionsVerification
{

    /// <summary>Compares all byte-indexed speed windows and, unless limited to definitions, the signed subpixel movement path.</summary>
    internal static void VerifyCompiledZoaSpeeds(SuperMetroidAddressSpace rom, bool definitionsOnly = false)
    {
        ushort Word(int address) => CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
        // #1412: the catalog states each stage's speed in eighths of a pixel; the cartridge's whole/fraction record for
        // that stage decodes to the same 16.16 speed, and the trailing record is the stationary zero no stage reaches.
        int Record(int stage)
        {
            int address = ZoaSpeedSources.SpeedTable + stage * ZoaSpeedSources.RecordBytes;
            return unchecked(unchecked((int)SixteenSixteen.Compose(Word(address), Word(address + 2))));
        }
        AssertEqual(4, ZoaSpeedDefinitions.StageSpeedEighths.Length, "Zoa has four speed stages");
        for (int stage = 0; stage < ZoaSpeedDefinitions.StageSpeedEighths.Length; stage++)
            AssertEqual(Record(stage), ZoaSpeedDefinitions.StageSpeedEighths[stage] * 8192, $"Zoa speed stage {stage} decodes to the catalog speed");
        AssertEqual(0, Record(ZoaSpeedDefinitions.StageSpeedEighths.Length), "Zoa's trailing speed record is stationary");
        if (definitionsOnly)
        {
            Console.WriteLine("Zoa speed cases: all four stage speeds match the cartridge records.");
            return;
        }
        var enemies = new RoomEnemySystem(new EnemySession());
        Action<RoomEnemySlot, ZoaEnemyState, Position16<RoomPixels>> shoot = ZoaBehavior.RunZoaShooting;
        RoomEnemySlot actor = enemies.SlotTable.Slots[0];
        var state = new ZoaEnemyState();
        for (int stage = 0; stage < ZoaSpeedDefinitions.StageSpeedEighths.Length; stage++)
        {
            int magnitude = Record(stage);
            state.ShotSpeedStage = stage;
            for (ushort direction = 0; direction <= 2; direction += 2)
            {
                // Keep the same animation installed: this exercises movement without an art-data bus.
                state.InstructionListTableIndex = state.PreviousInstructionListTableIndex =
                    (ZoaAnimationSelector)direction;
                int displacement = direction == 0 ? -magnitude : magnitude;
                for (int fraction = 0; fraction <= ushort.MaxValue; fraction++)
                {
                    actor.Position = new(new(128, (ushort)fraction), new(128, 123));
                    shoot(actor, state, Position16<RoomPixels>.Zero);
                    uint expected = unchecked(0x00800000u + (uint)fraction + (uint)displacement);
                    AssertEqual(FixedCoordinate.WholeOf(unchecked((int)expected)), actor.Position.X.Whole, "Zoa real signed whole displacement");
                    AssertEqual((ushort)expected, actor.Position.X.Fraction, "Zoa real fractional carry/borrow");
                    AssertEqual((ushort)128, actor.Position.Y.Whole, "Zoa shooting preserves Y");
                    AssertEqual((ushort)123, actor.Position.Y.Fraction, "Zoa shooting preserves sub-Y");
                }
            }
        }

        // #1269: leaving the camera square snaps Zoa to its spawn coordinate. $A3:B51E-$B527
        // writes only the whole words, so both subpixel fractions carry into the next flight.
        state.ShotSpeedStage = 0;
        state.InstructionListTableIndex = state.PreviousInstructionListTableIndex =
            ZoaAnimationSelector.None;
        state.SpawnPosition = new(0x560, 0xd8);
        actor.Position = new(new(128, 0x8000), new(128, 0x4000));
        int resetDisplacement = -Record(0);
        uint movedX = unchecked(0x00808000u + (uint)resetDisplacement);
        shoot(actor, state, new(0x1000, 0x1000));
        AssertEqual((ushort)0x560, actor.Position.X.Whole, "off-screen Zoa returns to its spawn X");
        AssertEqual((ushort)0xd8, actor.Position.Y.Whole, "off-screen Zoa returns to its spawn Y");
        AssertEqual((ushort)movedX, actor.Position.X.Fraction, "the spawn reset keeps the moved sub-X");
        AssertEqual((ushort)0x4000, actor.Position.Y.Fraction, "the spawn reset keeps sub-Y");
        Console.WriteLine("Zoa compiled speeds: all four stage speeds and 524288 real signed/subpixel integrations match without a speed-table bus.");
    }

    /// <summary>Runs the retail Zoa speed and movement comparison.</summary>
    [VerificationCommand("--zoa-speeds")]
    internal static int ZoaSpeedsCommand()
    {
        VerifyCompiledZoaSpeeds(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc")));
        return 0;
    }
}
