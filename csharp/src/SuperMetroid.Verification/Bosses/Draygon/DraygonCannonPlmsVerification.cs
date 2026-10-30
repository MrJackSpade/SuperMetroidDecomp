using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.Draygon.DraygonCannonPlmProgramVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.SyntheticRoomCartridgeFixturesVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Bosses.Draygon;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Draygon cannon PLM setup, missile damage, destruction terrain, and enemy firing flags in a constructed room.</summary>
internal static class DraygonCannonPlmsVerification
{
    /// <summary>
    /// Reproduces the exact four retail cannon records in a constructed 2x2-screen room.
    /// The typed programs run setup, hit filtering, destruction, slot order, and the shared
    /// bank-$84-to-bank-$A5 control-word writes from unrelated Draygon combat timing.
    /// </summary>
    internal static void VerifyDraygonCannonPlms()
    {
        Suite(nameof(VerifyDraygonCannonPlmProgram), () => VerifyDraygonCannonPlmProgram());
        const int roomWidth = 32;
        const int roomHeight = 32;
        var bus = new TestAddressSpace();
        SeedDraygonCannonFixture(bus);
        RoomLevelData level = CreateEmptyRoom(new(roomWidth, roomHeight));
        var disabledWords = new List<DraygonWallCannon>();
        var plms = PlmFixtures.Unbound();

        AssertEqual(4, PlmFixtures.LoadPopulation(ref plms, new PlmRoom(level, BgArtworkFixtures.For(new SnesVram()), new SharedGameState(), AreaId.Maridia), RoomPlmPopulationImporter.Read(bus, pointer: 0x9000), new RoomPlmServices { Samus = new FixtureRoomSamus(() => null), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false }, Enemies = new FixturePlmEnemyEffects { DraygonCannonDisabled = disabledWords.Add } }),
            "Draygon cannon population record count");

        RoomPlmSlotSnapshot[] slots = plms.PopulationSlots.ToArray();
        AssertEqual(4, slots.Length, "Draygon cannon native slot count");
        AssertEqual(39, slots[0].NativeSlotIndex, "pre-destroyed cannon receives first slot");
        AssertEqual(38, slots[1].NativeSlotIndex, "lower-left cannon receives second slot");
        AssertEqual(37, slots[2].NativeSlotIndex, "upper-right cannon receives third slot");
        AssertEqual(36, slots[3].NativeSlotIndex, "lower-right cannon receives fourth slot");

        int preDestroyedBlock = 11 * roomWidth + 2;
        int rightBlock = 18 * roomWidth + 2;
        int upperLeftBlock = 15 * roomWidth + 29;
        int lowerLeftBlock = 21 * roomWidth + 29;
        AssertEqual(RoomCollisionType.ShootableBlock,
            level.GetCollisionBlockByIndex(rightBlock).CollisionType,
            "shielded right cannon setup installs a shootable block");
        AssertEqual(0x44, level.GetCollisionBlockByIndex(rightBlock).Behavior.Value,
            "shielded right cannon setup installs behaviour 44");
        AssertEqual(RoomCollisionType.VerticalExtension,
            level.GetCollisionBlockByIndex(rightBlock + roomWidth).CollisionType,
            "shielded right cannon setup installs its vertical extension");
        AssertEqual(0xff, level.GetCollisionBlockByIndex(rightBlock + roomWidth).Behavior.Value,
            "shielded right cannon extension points to its origin");

        StepDraygonCannonPlms(plms, level);
        AssertSequenceEqual(
            new[] { DraygonWallCannon.UpperLeft },
            disabledWords,
            "pre-destroyed cannon immediately writes its bank-$A5 control word");
        AssertDestroyedCannon(level, preDestroyedBlock, roomWidth, "pre-destroyed cannon");

        AssertTrue(plms.ProjectileTriggers.TryNotifyResidentProjectileHit(rightBlock, 0x0000),
            "beam collision reaches the right cannon PLM");
        StepDraygonCannonPlms(plms, level);
        AssertEqual(1, disabledWords.Count, "beam cannot damage a Draygon cannon");

        for (int hit = 0; hit < 3; hit++)
        {
            AssertTrue(plms.ProjectileTriggers.TryNotifyResidentProjectileHit(rightBlock, 0x0100),
                $"missile hit {hit + 1} reaches right cannon");
            SettleDraygonCannonHit(plms, level);
        }
        AssertTrue(disabledWords.Contains(DraygonWallCannon.LowerLeft),
            "third missile disables the lower-left firing word");
        AssertDestroyedCannon(level, rightBlock, roomWidth, "right cannon");

        AssertTrue(plms.ProjectileTriggers.TryNotifyResidentProjectileHit(upperLeftBlock, 0x0200),
            "Super Missile collision reaches left cannon");
        SettleDraygonCannonHit(plms, level);
        AssertTrue(disabledWords.Contains(DraygonWallCannon.UpperRight),
            "one Super Missile disables the upper-right firing word");
        AssertDestroyedCannon(level, upperLeftBlock, roomWidth, "upper-left cannon");
        AssertTrue(!disabledWords.Contains(DraygonWallCannon.LowerRight),
            "untouched lower-right cannon remains enabled");
        AssertEqual(RoomCollisionType.ShootableBlock,
            level.GetCollisionBlockByIndex(lowerLeftBlock).CollisionType,
            "untouched lower-right cannon remains shootable");

        Console.WriteLine(
            "  Draygon cannons: retail slot order, setup, missile thresholds, terrain, and firing flags agree.");
    }

    /// <summary>Installs the four retail cannon records in the fixture bus.</summary>
    private static void SeedDraygonCannonFixture(TestAddressSpace bus)
    {
        SeedRoomPlmPopulation(bus, 0x9000, [
            0x65, 0xdf, 2, 11, 0x02, 0x88,
            0x59, 0xdf, 2, 18, 0x04, 0x88,
            0x71, 0xdf, 29, 15, 0x06, 0x88,
            0x71, 0xdf, 29, 21, 0x08, 0x88,
            0, 0,
        ]);
    }

    /// <summary>Advances the PLM long enough to resolve a notified projectile hit and its follow-up draw.</summary>
    private static void SettleDraygonCannonHit(
        RoomPlmSystem plms,
        RoomLevelData level)
    {
        for (int frame = 0; frame < 3; frame++)
            StepDraygonCannonPlms(plms, level);
    }

    /// <summary>Runs one PLM frame with fixed view and no scroll, enemy-death, or controller events.</summary>
    private static void StepDraygonCannonPlms(
        RoomPlmSystem plms,
        RoomLevelData level) =>
        plms.Step(level, new PlmFrameInputs { Scrolls = null, EnemyDeaths = 0, EnemyDeathQuota = 0, ControllerNewInput = 0 });

    /// <summary>Requires both cannon cells to become spike blocks with the destroyed-cannon behaviour.</summary>
    private static void AssertDestroyedCannon(
        RoomLevelData level,
        int blockIndex,
        int roomWidth,
        string context)
    {
        foreach (int cell in new[] { blockIndex, blockIndex + roomWidth })
        {
            AssertEqual(RoomCollisionType.SpikeBlock,
                level.GetCollisionBlockByIndex(cell).CollisionType,
                $"{context} replaces cell {cell} with a spike block");
            AssertEqual(0x03, level.GetCollisionBlockByIndex(cell).Behavior.Value,
                $"{context} replaces cell {cell} with behaviour 03");
        }
    }

    /// <summary>Runs the constructed-room Draygon cannon PLM verification command.</summary>
    [VerificationCommand("--draygon-cannon-plms")]
    internal static int DraygonCannonPlmsCommand()
    {
        Suite(nameof(VerifyDraygonCannonPlms), () => VerifyDraygonCannonPlms());
        return 0;
    }
}
