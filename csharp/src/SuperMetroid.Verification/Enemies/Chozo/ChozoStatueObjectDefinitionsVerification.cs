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
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Verification.Harness;
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
        Suite(nameof(VerifyLowerNorfairHandTrigger), () => VerifyLowerNorfairHandTrigger());
        Suite(nameof(VerifyChozoProgramControls), () => VerifyChozoProgramControls(rom));
        Suite(nameof(VerifyChozoProgramDraws), () => VerifyChozoProgramDraws(rom));
        Suite(nameof(VerifyChozoProgramTargets), () => VerifyChozoProgramTargets(rom));
        Suite(nameof(VerifyChozoProgramCallback), () => VerifyChozoProgramCallback(rom));
        Suite(nameof(VerifyChozoProgramEvent), () => VerifyChozoProgramEvent(rom));
        Suite(nameof(VerifyChozoLayoutGeometry), () => VerifyChozoLayoutGeometry(rom));
        Suite(nameof(VerifyChozoLayoutCollision), () => VerifyChozoLayoutCollision(rom));
        Suite(nameof(VerifyChozoLayoutVisuals), () => VerifyChozoLayoutVisuals(rom));
        Suite(nameof(VerifyChozoStatueVisualInstallation), () => VerifyChozoStatueVisualInstallation(rom));
        Console.WriteLine(
            "Chozo statue objects: six room objects over five native lists against their typed objects, the Lower Norfair hand's trigger, native draws and visual-only terrain overrides pass.");
    }

    /// <summary>
    /// Checks that the six statue room objects are the original game's five statue objects, the plug's crumble spawned two
    /// ways, plays each one's native list against its typed object (#1554), and checks what each spawn leaves at its
    /// place: the plug's two spawns leave their own blocks (#1622).
    /// </summary>
    private static void VerifyChozoTerrainHeaders(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        ChozoStatueRoomObject[] statueObjects = Enum.GetValues<ChozoStatueRoomObject>();
        AssertEqual(6, statueObjects.Length, "Chozo statue room object count");

        ushort[] headers = [0xd6d6,0xd6ee,0xd6f8,0xd6fc,0xd113];
        AssertTrue(headers.SequenceEqual(statueObjects.Select(statueObject => RoomObjectNativeWords.Word(NativeIdentities[statueObject])).Distinct()),
            "the statue room objects are the original game's five statue objects, in order");

        // The four terrain changes play as their native lists do; the slope accesses change the slope as they end.
        var changes = new Dictionary<ChozoStatueRoomObject, TerrainChange>
        {
            [ChozoStatueRoomObject.WreckedShipHand] = ChozoStatueObjectDefinitions.WreckedShipHand,
            [ChozoStatueRoomObject.SlopeAccessOpening] = ChozoStatueObjectDefinitions.ClearSlopeAccess,
            [ChozoStatueRoomObject.SlopeAccessClosing] = ChozoStatueObjectDefinitions.BlockSlopeAccess,
            [ChozoStatueRoomObject.PlugCrumbleAtHand] = ChozoStatueObjectDefinitions.CrumblePlug,
            [ChozoStatueRoomObject.PlugCrumbleUnderFootstep] = ChozoStatueObjectDefinitions.CrumblePlug,
        };
        foreach ((ChozoStatueRoomObject statueObject, TerrainChange change) in changes)
        {
            NativeTerrainChanges.Compare(cartridge, change, NativeList(cartridge, statueObject), ChozoTerrainStepAt, statueObject.ToString(),
                out bool began, out IReadOnlyList<NativeTerrainChanges.NativeStep> atEnd);
            AssertTrue(!began, $"{statueObject} opens nothing as it starts");
            string expected = statueObject == ChozoStatueRoomObject.SlopeAccessOpening ? SpikesBecomeSlopes
                : statueObject == ChozoStatueRoomObject.SlopeAccessClosing ? SlopesBecomeSpikes
                : "";
            AssertEqual(expected, string.Join(", ", atEnd.Select(step => step.Effect)), $"{statueObject} acts as it ends exactly as its typed object does");
        }
        VerifyLowerNorfairHandList(cartridge);

        foreach (ChozoStatueRoomObject statueObject in statueObjects)
        {
            const int width = 32;
            const int height = 16;
            var level = CreateEmptyRoom(new(width, height));
            var roomObjects = RoomObjectFixtures.Unbound();
            var block = new Position16<RoomBlocks>(3, 4);
            RoomTerrainBlock before = level.GetCollisionBlock(block).Terrain;

            AssertTrue(
                roomObjects.ChozoStatue.TrySpawnChozoStatueObject(level, new RoomObjectSpawnRequest<ChozoStatueRoomObject>(block, statueObject)),
                $"Chozo statue's {statueObject} spawns");
            RoomObjectSlotSnapshot slot = roomObjects.PopulationSlots.Single();
            AssertEqual(0, slot.ProcessingPosition, "the statue's room object takes the first free place in the update order");
            AssertEqual(block, slot.Block, "the statue's room object sits at its requested block");
            AssertTrue(statueObject == ChozoStatueRoomObject.LowerNorfairHand
                    ? slot.Behaviour is LowerNorfairChozoHandObject { Phase: LowerNorfairChozoHandPhase.Starting }
                    : slot.Behaviour is TerrainChangeObject { Change: var started } && started == changes[statueObject]
                        && (slot.Behaviour is ChozoSlopeAccessObject) == (statueObject is ChozoStatueRoomObject.SlopeAccessOpening or ChozoStatueRoomObject.SlopeAccessClosing),
                $"Chozo statue's {statueObject} starts as its typed object");

            RoomCollisionBlock left = level.GetCollisionBlock(block);
            switch (statueObject)
            {
                case ChozoStatueRoomObject.WreckedShipHand:
                    AssertEqual(RoomCollisionType.SpecialBlock, left.CollisionType, "Wrecked Ship hand installs special-block collision");
                    AssertEqual(ChozoStatueRoomDefinitions.WreckedShipHandBehavior, left.Behavior, "Wrecked Ship hand installs its special-solid behaviour");
                    break;
                case ChozoStatueRoomObject.PlugCrumbleAtHand:
                    AssertEqual(ChozoStatueRoomDefinitions.HandCrumbleSetupBlock, left.Terrain, "the hand's plug crumble leaves the block the room's load last drew");
                    break;
                case ChozoStatueRoomObject.PlugCrumbleUnderFootstep:
                    AssertEqual(ChozoStatueRoomDefinitions.FootstepCrumbleSetupBlock, left.Terrain, "the footstep's plug crumble leaves block zero");
                    break;
                case ChozoStatueRoomObject.LowerNorfairHand:
                case ChozoStatueRoomObject.SlopeAccessOpening:
                case ChozoStatueRoomObject.SlopeAccessClosing:
                    AssertEqual(before, left.Terrain, $"{statueObject}'s spawn leaves its block as it was");
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(ChozoStatueRoomObject)} {statueObject}.");
            }
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => RoomObjectFixtures.Unbound().ChozoStatue.TrySpawnChozoStatueObject(CreateEmptyRoom(new(32, 16)),
                new RoomObjectSpawnRequest<ChozoStatueRoomObject>(new(3, 4), (ChozoStatueRoomObject)(-1))),
            "a spawn naming no statue room object fails");
    }

    /// <summary>
    /// The original game's identity of each statue room object, through which verification reads its native list; both
    /// plug crumbles are the one original object, spawned two ways.
    /// </summary>
    private static readonly Dictionary<ChozoStatueRoomObject, RoomObjectHeaderId> NativeIdentities = new()
    {
        [ChozoStatueRoomObject.LowerNorfairHand] = RoomObjectHeaderId.LowerNorfairChozoHand,
        [ChozoStatueRoomObject.WreckedShipHand] = RoomObjectHeaderId.WreckedShipChozoHand,
        [ChozoStatueRoomObject.SlopeAccessOpening] = RoomObjectHeaderId.ClearSlopeAccessForWreckedShipChozo,
        [ChozoStatueRoomObject.SlopeAccessClosing] = RoomObjectHeaderId.BlockSlopeAccessForWreckedShipChozo,
        [ChozoStatueRoomObject.PlugCrumbleAtHand] = RoomObjectHeaderId.CrumbleLowerNorfairChozoRoomPlug,
        [ChozoStatueRoomObject.PlugCrumbleUnderFootstep] = RoomObjectHeaderId.CrumbleLowerNorfairChozoRoomPlug,
    };

    /// <summary>
    /// The Lower Norfair hand waits for the Power Bomb block at its trigger block to break open (#1580). A Power Bomb
    /// breaks the trigger block while the hand waits; the hand keeps waiting through every update before the block's
    /// breakup shows its last stage, and on the first of its turns that follows that stage it makes the trigger block the
    /// hand's special block and leaves the room. That is the update the cartridge's comparison fires on: the first turn of
    /// the hand at which the trigger block shows blank air, which the stock breakup's last stage is the first to show.
    /// </summary>
    private static void VerifyLowerNorfairHandTrigger()
    {
        RoomLevelData level = CreateEmptyRoom(new(48, 32));
        var trigger = new Position16<RoomBlocks>(ChozoStatueRoomDefinitions.LowerNorfairTriggerX, ChozoStatueRoomDefinitions.LowerNorfairTriggerY);
        level.SetTerrain(trigger, RoomTerrainBlock.Of(0x1f4, BlockMirroring.None, RoomCollisionType.ShootableBlock));
        RoomObjectSystem roomObjects = RoomObjectFixtures.Bound(
            new RoomObjectRoom(level, BgArtworkFixtures.For(new SnesVram()), new SharedGameState(new SharedRandom()), AreaId.Norfair),
            new RoomObjectServices { Samus = new FixtureRoomSamus(() => null), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => true } },
            RoomObjectFixtures.Unbound());
        AssertTrue(roomObjects.ChozoStatue.TrySpawnChozoStatueObject(level,
                new RoomObjectSpawnRequest<ChozoStatueRoomObject>(ChozoStatueRoomDefinitions.LowerNorfairHandBlock, ChozoStatueRoomObject.LowerNorfairHand)),
            "Lower Norfair hand allocates");

        // The hand starts on its first update and waits from its second while the trigger block stands intact.
        const int updatesBeforeBreak = 3;
        for (int update = 0; update < updatesBeforeBreak; update++)
            roomObjects.Step(level);
        AssertTrue(LowerNorfairHand(roomObjects) is { Phase: LowerNorfairChozoHandPhase.Waiting },
            "Lower Norfair hand waits while its trigger block stands intact");

        // The trigger block is a permanent Power Bomb block: behaviour nine of the shootable table.
        const byte permanentPowerBombBlock = 9;
        AssertTrue(roomObjects.TrySpawnProjectileShotBlock(level, trigger, permanentPowerBombBlock,
                SamusProjectileType.Of(SamusProjectileFamily.PowerBomb), solidBlock: true),
            "a Power Bomb breaks the trigger block");
        AssertTrue(!roomObjects.BlockReactions.IsBrokenOpen(trigger), "the trigger block is not broken open as its breakup starts");
        bool breakupRunsFirst = roomObjects.PopulationSlots.Single(slot => slot.Block == trigger).ProcessingPosition
            < roomObjects.PopulationSlots.Single(slot => slot.Behaviour is LowerNorfairChozoHandObject).ProcessingPosition;
        BlockReactionChange breakup = BlockReactionChangeDefinitions.PermanentPowerBombBlock;
        int lastStageUpdate = breakup.FirstFrameDelay + breakup.Frames.SkipLast(1).Sum(frame => frame.Updates);
        int firingUpdate = breakupRunsFirst ? lastStageUpdate : lastStageUpdate + 1;

        for (int update = 1; update < firingUpdate; update++)
        {
            roomObjects.Step(level);
            AssertTrue(LowerNorfairHand(roomObjects) is { Phase: LowerNorfairChozoHandPhase.Waiting },
                $"Lower Norfair hand still waits on update {update} of the trigger block's breakup");
            AssertEqual(update >= lastStageUpdate, roomObjects.BlockReactions.IsBrokenOpen(trigger),
                $"the trigger block stands broken open from its breakup's last stage, update {update}");
            // The stock breakup's last stage is the first to show blank air, so the cartridge's comparison could not fire
            // before it.
            AssertEqual(update >= lastStageUpdate, level.GetCollisionBlock(trigger).Terrain == BlankAirBlock.Value,
                $"the trigger block first shows blank air on its breakup's last stage, update {update}");
        }

        roomObjects.Step(level);
        AssertTrue(LowerNorfairHand(roomObjects) is null,
            $"Lower Norfair hand leaves on update {firingUpdate}, its first turn after the breakup's last stage");
        RoomCollisionBlock handBlock = level.GetCollisionBlock(trigger);
        AssertEqual(RoomCollisionType.SpecialBlock, handBlock.CollisionType, "the triggered hand makes its trigger block special");
        AssertEqual(ChozoStatueRoomDefinitions.LowerNorfairHandBehavior, handBlock.Behavior,
            "the triggered hand gives its trigger block the hand's behaviour");
    }

    /// <summary>The live Lower Norfair hand, or null once it has left the room.</summary>
    private static LowerNorfairChozoHandObject? LowerNorfairHand(RoomObjectSystem roomObjects) =>
        roomObjects.PopulationSlots.Select(slot => slot.Behaviour).OfType<LowerNorfairChozoHandObject>().SingleOrDefault();

    /// <summary>The comparison's name for the clear-slope access's ending.</summary>
    private const string SpikesBecomeSlopes = "spikes become slopes";

    /// <summary>The comparison's name for the block-slope access's ending.</summary>
    private const string SlopesBecomeSpikes = "slopes become spikes";

    /// <summary>The native list a statue room object starts.</summary>
    private static ushort NativeList(IImportCartridgeSource cartridge, ChozoStatueRoomObject statueObject) =>
        cartridge.ReadCartridgeWord(0x840000 | unchecked((ushort)(RoomObjectNativeWords.Word(NativeIdentities[statueObject]) + 2)));

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
        ushort entry = NativeList(cartridge, ChozoStatueRoomObject.LowerNorfairHand);
        AssertEqual(RoomObjectInstruction.GotoIfEventSet, InstructionAt<RoomObjectInstruction>(cartridge, entry), "Lower Norfair hand checks an event first");
        AssertEqual(StoryEvent.LowerNorfairChozoLoweredAcid.Number(), Word(entry + 2), "Lower Norfair hand checks the lowered-acid event");
        ushort lowered = Word(entry + 4);
        AssertEqual(RoomObjectInstruction.InstallFrameBehavior, InstructionAt<RoomObjectInstruction>(cartridge, entry + 6), "Lower Norfair hand otherwise waits");
        AssertEqual(ChozoStatueObjectNativeRoutines.WaitForLowerNorfairHand, Word(entry + 8), "Lower Norfair hand waits for its trigger block");
        AssertEqual(RoomObjectInstruction.Sleep, InstructionAt<RoomObjectInstruction>(cartridge, entry + 10), "Lower Norfair hand sleeps while it waits");
        AssertEqual(RoomObjectInstruction.SetLoweredAcidHeight, InstructionAt<RoomObjectInstruction>(cartridge, lowered), "lowered hand lowers the acid first");
        AssertEqual(1, (int)Word(lowered + 2), "lowered hand shows for one update");
        AssertEqual(RoomObjectDrawAddressCodec.Of(ChozoStatueObjectDefinitions.LoweredHand), (int)Word(lowered + 4), "lowered hand's draw");
        AssertEqual(RoomObjectInstruction.Delete, InstructionAt<RoomObjectInstruction>(cartridge, lowered + 6), "lowered hand ends after its draw");
    }
}
