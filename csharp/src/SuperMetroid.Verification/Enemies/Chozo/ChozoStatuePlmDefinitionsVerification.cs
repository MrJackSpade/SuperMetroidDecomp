using SuperMetroid.Verification.Rooms.Plms;
using static SuperMetroid.Verification.Enemies.Chozo.ChozoStatueVisualsVerification;
using static SuperMetroid.Verification.Enemies.Chozo.LookupChozoLayoutsVerification;
using static SuperMetroid.Verification.Enemies.Chozo.LookupChozoProgramVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;

using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
namespace SuperMetroid.Verification.Enemies.Chozo;

/// <summary>Checks Chozo statue PLM headers, programs, layouts, and installed visuals against cartridge data.</summary>
internal static class ChozoStatuePlmDefinitionsVerification
{
    /// <summary>Runs the Chozo terrain, instruction, draw, layout, and visual checks as separate suites.</summary>
    internal static void VerifyChozoStatuePlmDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyChozoTerrainHeaders), () => VerifyChozoTerrainHeaders(rom));
        Suite(nameof(VerifyChozoProgramControls), () => VerifyChozoProgramControls(rom));
        Suite(nameof(VerifyChozoProgramDraws), () => VerifyChozoProgramDraws(rom));
        Suite(nameof(VerifyChozoProgramTargets), () => VerifyChozoProgramTargets(rom));
        Suite(nameof(VerifyChozoProgramCallback), () => VerifyChozoProgramCallback(rom));
        Suite(nameof(VerifyChozoProgramEvent), () => VerifyChozoProgramEvent(rom));
        Suite(nameof(RoomPlmProgramsVerification.VerifySharedPrograms), () => RoomPlmProgramsVerification.VerifySharedPrograms(rom));
        Suite(nameof(VerifyChozoLayoutGeometry), () => VerifyChozoLayoutGeometry(rom));
        Suite(nameof(VerifyChozoLayoutCollision), () => VerifyChozoLayoutCollision(rom));
        Suite(nameof(VerifyChozoLayoutVisuals), () => VerifyChozoLayoutVisuals(rom));
        Suite(nameof(VerifyChozoStatueVisualInstallation), () => VerifyChozoStatueVisualInstallation(rom));
        Console.WriteLine(
            "Chozo statue PLMs: five headers, four bounded instruction streams, shared delete, native draws and visual-only terrain overrides pass.");
    }

    /// <summary>Checks five terrain headers across the full ushort domain and validates their spawned slot state.</summary>
    private static void VerifyChozoTerrainHeaders(SuperMetroidAddressSpace rom)
    {
        ChozoStatuePlmDefinition[] definitions = ChozoStatuePlmDefinitions.All.ToArray();
        AssertEqual(5, definitions.Length, "Chozo terrain PLM definition count");

        ushort[] headers = [0xd6d6,0xd6ee,0xd6f8,0xd6fc,0xd113];
        // PlmHeaderId's values are arbitrary (#1414): the domain is every member, each translated to its cartridge word.
        AssertTrue(headers.SequenceEqual(definitions.Select(d => PlmNativeWords.Word(d.Header))), "Chozo original header enumeration");
        foreach (PlmHeaderId candidate in Enum.GetValues<PlmHeaderId>())
        {
            ushort header = PlmNativeWords.Word(candidate);
            if (!headers.Contains(header))
                AssertThrows<InvalidDataException>(() => ChozoStatuePlmHeaders.Resolve(candidate), "Chozo full unsupported header domain");
            else
            {
                var selected = ChozoStatuePlmHeaders.Resolve(candidate);
                AssertEqual(candidate, selected.Header, "Chozo dispatch header identity");
                AssertEqual(PlmProgramStartOracle.ForNativeList(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x840000 | (header + 2)))), selected.Program,
                    "Chozo dispatch original initial list");
                AssertEqual(definitions.Single(d => d.Header == candidate), selected, "Chozo named/enumerated dispatch agreement");
            }
        }
        foreach (ChozoStatuePlmDefinition definition in definitions)
        {
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x840000 | unchecked((ushort)(PlmNativeWords.Word(definition.Header) + 2))));
            AssertEqual(
                PlmProgramStartOracle.ForNativeList(expected),
                definition.Program,
                $"Chozo PLM {definition.Header} initial program matches cartridge");

            const int width = 32;
            const int height = 16;
            var level = CreateEmptyRoom(new(width, height));
            var plms = PlmFixtures.Unbound();
            var request = new ChozoStatuePlmRequest(
                definition.Header,
                Block: new Position16<RoomBlocks>(3, 4),
                IsHardcoded: false);

            AssertTrue(
                plms.ChozoStatue.TrySpawnChozoStatuePlm(level, request),
                $"Chozo PLM {definition.Header} allocates");
            RoomPlmSlotSnapshot slot = plms.PopulationSlots.Single();
            AssertEqual(39, slot.NativeSlotIndex, "Chozo PLM uses highest free native slot");
            AssertEqual(definition.Header, slot.HeaderPointer, "Chozo PLM header identity");
            AssertEqual(4 * width + 3, slot.BlockIndex, "Chozo PLM requested block coordinate");
            AssertTrue(PlmProgramStartOracle.IsRunning(definition.Program, slot), "Chozo PLM starts its first program");
            AssertEqual(1, slot.InstructionTimer, "Chozo PLM starts on timer one");

            if (definition.Header == PlmHeaderId.WreckedShipChozoHand)
            {
                RoomCollisionBlock block = level.GetCollisionBlock(new(3, 4));
                AssertEqual(
                    RoomCollisionType.SpecialBlock,
                    block.CollisionType,
                    "Wrecked Ship hand installs special-block collision");
                AssertEqual(
                    ChozoStatueRoomDefinitions.WreckedShipHandBehavior,
                    block.Behavior,
                    "Wrecked Ship hand installs its special-solid behaviour");
            }
        }

        AssertThrows<InvalidDataException>(
            () => ChozoStatuePlmHeaders.Resolve(PlmHeaderId.CollisionWreckedShipChozoHandCheck),
            "Chozo collision-trigger header cannot enter terrain-spawn domain");
    }
}
