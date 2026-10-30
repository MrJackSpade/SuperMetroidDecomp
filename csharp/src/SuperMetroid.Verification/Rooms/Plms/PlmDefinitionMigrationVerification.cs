using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>
/// Confirms #1414's PLM migration: each deleted PLM catalog's definitions still produce, for one member of each kind, the
/// values the catalog held, the member-matched identifications agree with the former header arithmetic, and the
/// native-word table translates the arbitrary identities to the cartridge's header words. None reads the ROM.
/// </summary>
internal static class PlmDefinitionMigrationVerification
{
    /// <summary>Compares the new definitions and identifications with the values the deleted catalogs and arithmetic produced.</summary>
    internal static void VerifyPlmDefinitionMigration()
    {
        // The header words are verification's; the identities' own values are arbitrary.
        AssertEqual(0xb62f, PlmNativeWords.Word(PlmHeaderId.Nothing), "first PLM header word");
        AssertEqual(0xc85a, PlmNativeWords.Word(PlmHeaderId.YellowDoorFacingLeft), "yellow door header word");
        AssertEqual(0xefcf, PlmNativeWords.Word(PlmHeaderId.ShotBlockReserveTank), "last PLM header word");
        AssertEqual(PlmHeaderId.EyeDoorFacingRight, PlmNativeWords.Header(0xdb4c), "eye door header decodes");

        // Door and item identification by member equals the former range and stride arithmetic.
        AssertTrue(DoorPlms.TryIdentifyColoredDoor(PlmHeaderId.GreenDoorFacingUp, out ColoredDoorColor color, out ColoredDoorOrientation orientation)
            && color == ColoredDoorColor.Green && orientation == ColoredDoorOrientation.Up, "green door facing up");
        AssertTrue(!DoorPlms.TryIdentifyColoredDoor(PlmHeaderId.BlueDoorFacingLeft, out _, out _), "blue door is not a colored door");
        AssertTrue(GreyDoorPlms.TryIdentifyGreyDoor(PlmHeaderId.BombTorizoGreyDoor, out ColoredDoorOrientation torizo)
            && torizo == ColoredDoorOrientation.Right, "Bomb Torizo grey door faces right");
        AssertTrue(GreyDoorPlms.TryIdentifyGreyDoor(PlmHeaderId.GreyDoorFacingDown, out ColoredDoorOrientation down)
            && down == ColoredDoorOrientation.Down, "grey door facing down");
        AssertTrue(CollectiblePlms.TryIdentifyPermanentCollectible(PlmHeaderId.ChozoMorphBall, out InWorldCollectibleKind kind, out CollectiblePresentation presentation)
            && kind == InWorldCollectibleKind.MorphBall && presentation == CollectiblePresentation.ChozoOrb, "Chozo Morph Ball");
        AssertTrue(CollectiblePlms.TryIdentifyPermanentCollectible(PlmHeaderId.ShotBlockReserveTank, out kind, out presentation)
            && kind == InWorldCollectibleKind.ReserveTank && presentation == CollectiblePresentation.ShotBlock, "shot-block Reserve Tank");
        AssertTrue(!CollectiblePlms.TryIdentifyPermanentCollectible(PlmHeaderId.RedDoorFacingUp, out _, out _), "a door is no item");
        AssertEqual(63, PermanentCollectiblePlmDefinitions.Items.Count, "three presentations of twenty-one items");

        // The station animations are named by layout instead of the header's list word plus an offset.
        AssertEqual(StationAnimationList.MapIdle, StationAccessLayouts.Map.Animation, "map station animation");
        AssertEqual((StationAnimationList?)StationAnimationList.MapAcquired, StationAccessLayouts.Map.AcquiredAnimation, "map station acquired animation");
        AssertTrue(StationAccessLayouts.Energy.AcquiredAnimation is null, "energy station has one animation");
        AssertEqual(0xad66, StationNativeWords.AnimationLists.Word(StationAnimationList.MapIdle), "map idle list word");

        // The deleted RomData catalogs' values.
        AssertEqual(5, DownwardGateDefinitions.GateHeightInBlocks, "gate height");
        AssertEqual(new RoomBlockBehavior(0x10), DownwardGateDefinitions.ClosedGateBehavior, "closed gate behaviour");
        AssertEqual(0x1000, DownwardGateDefinitions.OneBlockDistance, "gate projectile block distance");
        AssertEqual(RoomTerrainBlock.Of(0x044, BlockMirroring.None, RoomCollisionType.SolidBlock), NoobTubePlmDefinitions.SolidProjectileTrigger, "n00b tube origin block");
        AssertEqual(0x000b, NoobTubePlmDefinitions.EarthquakeType, "n00b tube earthquake type");
        AssertEqual(0x0040, NoobTubePlmDefinitions.EarthquakeTimer, "n00b tube earthquake timer");
        AssertEqual(0xfb00, NoobTubeDebrisDefinitions.BubbleInitialYVelocity.RawValue, "bubble velocity");
        AssertEqual(0x00c0, NoobTubeDebrisDefinitions.CrackFallVelocity.RawValue, "crack velocity");
        AssertEqual(0x00c0, NoobTubeDebrisDefinitions.ShardFallYVelocity.RawValue, "shard fall velocity");
        AssertEqual(0xee00, NoobTubeDebrisDefinitions.HiddenXPosition, "hidden actor X");
        AssertEqual(0x0ae0, SpeedBoosterEscapeDefinitions.StartFxMotionSamusX, "lavaquake start X");
        AssertEqual(0xff80, SpeedBoosterEscapeDefinitions.InitialLavaquakeVelocity.RawValue, "lavaquake velocity");
        AssertEqual(0xffe0, SpeedBoosterPickupDefinitions.LavaRiseVelocity.RawValue, "pickup lava velocity");
        AssertEqual(0x0010, SamusEaterPlmDefinitions.HeldInvincibilityBit, "plant hold invincibility bit");
        AssertEqual(48, SamusEaterPlmDefinitions.ReleaseInvincibilityFrames, "plant release invincibility");
        AssertEqual(2, SamusEaterPlmDefinitions.ChewDamage, "plant chew damage");
        AssertEqual(RoomTerrainBlock.Of(0x123, BlockMirroring.None, RoomCollisionType.SolidBlock), RoomTerrainBlock.Of(0x123, BlockMirroring.None, RoomCollisionType.SpecialBlock).Deactivated(), "plant trigger deactivation leaves a solid block solid with its appearance");
        AssertEqual(null, MetroidsClearedEvents.ResolveEvent(8), "ninth metroids-cleared entry is a no-op");
        AssertEqual((EventNumber?)EventNumber.FirstMetroidHallCleared, MetroidsClearedEvents.ResolveEvent(9), "first quota observer");
        AssertEqual((EventNumber?)EventNumber.SecondMetroidShaftCleared, MetroidsClearedEvents.ResolveEvent(12), "last quota observer");

        // Terrain blocks keep the collision kind and appearance of the former literal words.
        AssertEqual(RoomTerrainBlock.Of(0x180, BlockMirroring.None, RoomCollisionType.SolidBlock), MaridiaElevatubePlmDefinitions.Terrain, "elevatube block");
        AssertEqual(RoomTerrainBlock.Of(0x046, BlockMirroring.None, RoomCollisionType.ShootableBlock), GatePlms.DownwardGateShotBlock(0).LeftBlock, "blue-left gate trigger block");
        AssertEqual(RoomTerrainBlock.Of(0x04d, BlockMirroring.None, RoomCollisionType.ShootableBlock), GatePlms.DownwardGateShotBlock(7).RightBlock, "yellow-right gate trigger block");
        var bombHorizontal = new LinkedRestorePlmFamily.Draw(PlmDraw.Of(BombBlockRestoreDraw.Horizontal), PlmRestoreShape.Horizontal, LinkedRestorePlmFamily.BombBlock);
        AssertEqual(RoomTerrainBlock.Of(0x058, BlockMirroring.None, RoomCollisionType.BombableBlock), bombHorizontal.BlockAt(0, 0), "bomb block parent");
        AssertEqual(RoomTerrainBlock.Of(0x058, BlockMirroring.None, RoomCollisionType.HorizontalExtension), bombHorizontal.BlockAt(0, 1), "bomb block horizontal child");
        var crumbleVertical = new LinkedRestorePlmFamily.Draw(PlmDraw.Of(ContactCrumbleRestoreDraw.Vertical), PlmRestoreShape.Vertical, LinkedRestorePlmFamily.ContactCrumble);
        AssertEqual(RoomTerrainBlock.Of(0x0bc, BlockMirroring.None, RoomCollisionType.SpecialBlock), crumbleVertical.BlockAt(0, 0), "contact crumble parent");
        AssertEqual(RoomTerrainBlock.Of(0x0bc, BlockMirroring.None, RoomCollisionType.VerticalExtension), crumbleVertical.BlockAt(0, 1), "contact crumble vertical child");
        Console.WriteLine("PLM definition migration: header words, member identifications, station animations, catalog values and terrain blocks keep their values.");
    }

    /// <summary>Runs the focused PLM definition-migration check.</summary>
    [VerificationCommand("--plm-definition-migration")]
    internal static int PlmDefinitionMigrationCommand()
    {
        VerifyPlmDefinitionMigration();
        return 0;
    }
}
