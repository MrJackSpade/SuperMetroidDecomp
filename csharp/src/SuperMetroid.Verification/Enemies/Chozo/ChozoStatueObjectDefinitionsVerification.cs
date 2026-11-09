using SuperMetroid.Verification.Rooms.RoomObjects;
using static SuperMetroid.Verification.Enemies.Chozo.ChozoStatueVisualsVerification;
using static SuperMetroid.Verification.Enemies.Chozo.LookupChozoLayoutsVerification;
using static SuperMetroid.Verification.Enemies.Chozo.LookupChozoProgramVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Progression;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Progression;
using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.TestSupport.MovieReplay.NativeObjectListDefinitions;

using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
namespace SuperMetroid.Verification.Enemies.Chozo;

/// <summary>Checks Chozo statue PLM headers, programs, layouts, and installed visuals against cartridge data.</summary>
internal static class ChozoStatueObjectDefinitionsVerification
{
    /// <summary>Runs the Chozo terrain, instruction, draw, layout, and visual checks as separate suites.</summary>
    internal static void VerifyChozoStatueObjectDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyChozoTerrainHeaders), () => VerifyChozoTerrainHeaders(rom));
        Suite(nameof(VerifyChozoProgramControls), () => VerifyChozoProgramControls(rom));
        Suite(nameof(VerifyChozoProgramDraws), () => VerifyChozoProgramDraws(rom));
        Suite(nameof(VerifyChozoProgramTargets), () => VerifyChozoProgramTargets(rom));
        Suite(nameof(VerifyChozoProgramCallback), () => VerifyChozoProgramCallback(rom));
        Suite(nameof(VerifyChozoProgramEvent), () => VerifyChozoProgramEvent(rom));
        Suite(nameof(RoomObjectProgramsVerification.VerifySharedPrograms), () => RoomObjectProgramsVerification.VerifySharedPrograms(rom));
        Suite(nameof(VerifyChozoLayoutGeometry), () => VerifyChozoLayoutGeometry(rom));
        Suite(nameof(VerifyChozoLayoutCollision), () => VerifyChozoLayoutCollision(rom));
        Suite(nameof(VerifyChozoLayoutVisuals), () => VerifyChozoLayoutVisuals(rom));
        Suite(nameof(VerifyChozoStatueVisualInstallation), () => VerifyChozoStatueVisualInstallation(rom));
        Console.WriteLine(
            "Chozo statue objects: five headers, their native lists against their typed objects, native draws and visual-only terrain overrides pass.");
    }

    /// <summary>
    /// Checks the five statue objects across the whole room-object domain, plays each one's native list against its
    /// typed object (#1554), and checks what each spawn leaves at its place.
    /// </summary>
    private static void VerifyChozoTerrainHeaders(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        IReadOnlyList<RoomObjectHeaderId> definitions = ChozoStatueObjectDefinitions.Headers;
        AssertEqual(5, definitions.Count, "Chozo statue object count");

        ushort[] headers = [0xd6d6,0xd6ee,0xd6f8,0xd6fc,0xd113];
        // RoomObjectHeaderId's values are arbitrary (#1414): the domain is every member, each translated to its cartridge word.
        AssertTrue(headers.SequenceEqual(definitions.Select(RoomObjectNativeWords.Word)), "Chozo original header enumeration");
        foreach (RoomObjectHeaderId candidate in Enum.GetValues<RoomObjectHeaderId>())
        {
            if (!headers.Contains(RoomObjectNativeWords.Word(candidate)))
                AssertThrows<InvalidDataException>(() => ChozoStatueObjectHeaders.Require(candidate), "Chozo full unsupported header domain");
            else
                AssertEqual(candidate, ChozoStatueObjectHeaders.Require(candidate), "Chozo dispatch header identity");
        }

        // The four terrain changes play as their native lists do; the slope accesses change the slope as they end.
        var changes = new Dictionary<RoomObjectHeaderId, TerrainChange>
        {
            [RoomObjectHeaderId.WreckedShipChozoHand] = ChozoStatueObjectDefinitions.WreckedShipHand,
            [RoomObjectHeaderId.ClearSlopeAccessForWreckedShipChozo] = ChozoStatueObjectDefinitions.ClearSlopeAccess,
            [RoomObjectHeaderId.BlockSlopeAccessForWreckedShipChozo] = ChozoStatueObjectDefinitions.BlockSlopeAccess,
            [RoomObjectHeaderId.CrumbleLowerNorfairChozoRoomPlug] = ChozoStatueObjectDefinitions.CrumblePlug,
        };
        foreach ((RoomObjectHeaderId header, TerrainChange change) in changes)
        {
            NativeTerrainChanges.Compare(cartridge, change, NativeList(cartridge, header), ChozoTerrainStepAt, header.ToString(),
                out bool began, out IReadOnlyList<NativeTerrainChanges.NativeStep> atEnd);
            AssertTrue(!began, $"{header} opens nothing as it starts");
            string expected = header == RoomObjectHeaderId.ClearSlopeAccessForWreckedShipChozo ? SpikesBecomeSlopes
                : header == RoomObjectHeaderId.BlockSlopeAccessForWreckedShipChozo ? SlopesBecomeSpikes
                : "";
            AssertEqual(expected, string.Join(", ", atEnd.Select(step => step.Effect)), $"{header} acts as it ends exactly as its typed object does");
        }
        VerifyLowerNorfairHandList(cartridge);

        foreach (RoomObjectHeaderId header in definitions)
        {
            const int width = 32;
            const int height = 16;
            var level = CreateEmptyRoom(new(width, height));
            var roomObjects = RoomObjectFixtures.Unbound();
            var request = new ChozoStatueObjectRequest(
                header,
                Block: new Position16<RoomBlocks>(3, 4),
                IsHardcoded: false);

            AssertTrue(
                roomObjects.ChozoStatue.TrySpawnChozoStatueObject(level, request),
                $"Chozo PLM {header} allocates");
            RoomObjectSlotSnapshot slot = roomObjects.PopulationSlots.Single();
            AssertEqual(0, slot.ProcessingPosition, "Chozo PLM takes the first free slot in processing order");
            AssertEqual(header, slot.HeaderPointer, "Chozo PLM header identity");
            AssertEqual(4 * width + 3, slot.BlockIndex, "Chozo PLM requested block coordinate");
            AssertTrue(header == RoomObjectHeaderId.LowerNorfairChozoHand
                    ? slot.Behaviour is LowerNorfairChozoHandObject { Phase: LowerNorfairChozoHandPhase.Starting }
                    : slot.Behaviour is TerrainChangeObject { Change: var started } && started == changes[header]
                        && (slot.Behaviour is ChozoSlopeAccessObject) == (header is RoomObjectHeaderId.ClearSlopeAccessForWreckedShipChozo or RoomObjectHeaderId.BlockSlopeAccessForWreckedShipChozo),
                $"Chozo PLM {header} starts as its typed object");

            if (header == RoomObjectHeaderId.WreckedShipChozoHand)
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
            () => ChozoStatueObjectHeaders.Require(RoomObjectHeaderId.CollisionWreckedShipChozoHandCheck),
            "Chozo collision-trigger header cannot enter terrain-spawn domain");
    }

    /// <summary>The comparison's name for the clear-slope access's ending.</summary>
    private const string SpikesBecomeSlopes = "spikes become slopes";

    /// <summary>The comparison's name for the block-slope access's ending.</summary>
    private const string SlopesBecomeSpikes = "slopes become spikes";

    /// <summary>The list a statue object's header starts.</summary>
    private static ushort NativeList(IImportCartridgeSource cartridge, RoomObjectHeaderId header) =>
        cartridge.ReadCartridgeWord(0x840000 | unchecked((ushort)(RoomObjectNativeWords.Word(header) + 2)));

    /// <summary>The slope accesses' two slope changes.</summary>
    private static bool ChozoTerrainStepAt(IImportCartridgeSource cartridge, ushort cursor, out NativeTerrainChanges.NativeStep step)
    {
        RoomObjectInstruction instruction = InstructionAt<RoomObjectInstruction>(cartridge, cursor);
        string? effect = instruction == RoomObjectInstruction.TransformSpikesToSlopes ? SpikesBecomeSlopes
            : instruction == RoomObjectInstruction.RevertSlopesToSpikes ? SlopesBecomeSpikes
            : null;
        step = new NativeTerrainChanges.NativeStep(Offset16<RoomBlocks>.Zero, Sound: null, Begins: false, Length: 2, Effect: effect);
        return effect is not null;
    }

    /// <summary>
    /// The Lower Norfair hand's native list does what <see cref="LowerNorfairChozoHandObject"/> does: it continues at
    /// its lowered-acid list once the acid is lowered, and otherwise installs the trigger-block wait and sleeps; the
    /// lowered-acid list lowers the acid, draws the cleared hand for one update and ends.
    /// </summary>
    private static void VerifyLowerNorfairHandList(IImportCartridgeSource cartridge)
    {
        ushort Word(int address) => cartridge.ReadCartridgeWord(0x840000 | address);
        ushort entry = NativeList(cartridge, RoomObjectHeaderId.LowerNorfairChozoHand);
        AssertEqual(RoomObjectInstruction.GotoIfEventSet, InstructionAt<RoomObjectInstruction>(cartridge, entry), "Lower Norfair hand checks an event first");
        AssertEqual(StoryEvent.LowerNorfairChozoLoweredAcid.Number(), Word(entry + 2), "Lower Norfair hand checks the lowered-acid event");
        ushort lowered = Word(entry + 4);
        AssertEqual(RoomObjectInstruction.InstallFrameBehavior, InstructionAt<RoomObjectInstruction>(cartridge, entry + 6), "Lower Norfair hand otherwise waits");
        AssertEqual(ChozoStatueObjectNativeRoutines.WaitForLowerNorfairHand, Word(entry + 8), "Lower Norfair hand waits for its trigger block");
        AssertEqual(RoomObjectInstruction.Sleep, InstructionAt<RoomObjectInstruction>(cartridge, entry + 10), "Lower Norfair hand sleeps while it waits");
        AssertEqual(RoomObjectInstruction.SetLoweredAcidHeight, InstructionAt<RoomObjectInstruction>(cartridge, lowered), "lowered hand lowers the acid first");
        AssertEqual(1, (int)Word(lowered + 2), "lowered hand shows for one update");
        AssertEqual((int)RoomObjectDrawAddressCodec.Of(ChozoStatueObjectDefinitions.LoweredHand), (int)Word(lowered + 4), "lowered hand's draw");
        AssertEqual(RoomObjectInstruction.Delete, InstructionAt<RoomObjectInstruction>(cartridge, lowered + 6), "lowered hand ends after its draw");
    }
}
