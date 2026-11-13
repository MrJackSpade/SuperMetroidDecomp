using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>
/// Confirms #1414's PLM migration: each deleted PLM catalog's definitions still produce, for one member of each kind, the
/// values the catalog held, the member-matched identifications agree with the former header arithmetic, and the
/// native-word table translates the arbitrary identities to the cartridge's header words. None reads the ROM.
/// </summary>
internal static class RoomObjectDefinitionMigrationVerification
{
    /// <summary>Compares the new definitions and identifications with the values the deleted catalogs and arithmetic produced.</summary>
    internal static void VerifyObjectDefinitionMigration()
    {
        // The header words are verification's; the identities' own values are arbitrary.
        AssertEqual(0xb6cb, RoomObjectNativeWords.Word(RoomObjectHeaderId.InsideReactionBrinstarFloorPlant), "floor plant header word");
        AssertEqual(0xc85a, RoomObjectNativeWords.Word(RoomObjectHeaderId.YellowDoorFacingLeft), "yellow door header word");
        AssertEqual(0xefcf, RoomObjectNativeWords.Word(RoomObjectHeaderId.ShotBlockReserveTank), "last PLM header word");
        AssertEqual(RoomObjectHeaderId.EyeDoorFacingRight, RoomObjectNativeWords.Header(0xdb4c), "eye door header decodes");

        // Door and item identification by member equals the former range and stride arithmetic.
        AssertTrue(DoorObjects.TryIdentifyColoredDoor(RoomObjectHeaderId.GreenDoorFacingUp, out ColoredDoorColor color, out ColoredDoorOrientation orientation)
            && color == ColoredDoorColor.Green && orientation == ColoredDoorOrientation.Up, "green door facing up");
        AssertTrue(!DoorObjects.TryIdentifyColoredDoor(RoomObjectHeaderId.BlueDoorFacingLeft, out _, out _), "blue door is not a colored door");
        AssertTrue(GreyDoorObjects.TryIdentifyGreyDoor(RoomObjectHeaderId.BombTorizoGreyDoor, out ColoredDoorOrientation torizo)
            && torizo == ColoredDoorOrientation.Right, "Bomb Torizo grey door faces right");
        AssertTrue(GreyDoorObjects.TryIdentifyGreyDoor(RoomObjectHeaderId.GreyDoorFacingDown, out ColoredDoorOrientation down)
            && down == ColoredDoorOrientation.Down, "grey door facing down");
        AssertTrue(CollectibleObjects.TryIdentifyPermanentCollectible(RoomObjectHeaderId.ChozoMorphBall, out InWorldCollectibleKind kind, out CollectiblePresentation presentation)
            && kind == InWorldCollectibleKind.MorphBall && presentation == CollectiblePresentation.ChozoOrb, "Chozo Morph Ball");
        AssertTrue(CollectibleObjects.TryIdentifyPermanentCollectible(RoomObjectHeaderId.ShotBlockReserveTank, out kind, out presentation)
            && kind == InWorldCollectibleKind.ReserveTank && presentation == CollectiblePresentation.ShotBlock, "shot-block Reserve Tank");
        AssertTrue(!CollectibleObjects.TryIdentifyPermanentCollectible(RoomObjectHeaderId.RedDoorFacingUp, out _, out _), "a door is no item");
        AssertEqual(63, PermanentCollectibleObjectDefinitions.Items.Count, "three presentations of twenty-one items");

        // The station animations are named by access rule instead of the header's list word plus an offset.
        AssertEqual(StationAnimationList.MapIdle, StationAccessRuleDefinitions.Map.Animation, "map station animation");
        AssertEqual((StationAnimationList?)StationAnimationList.MapAcquired, StationAccessRuleDefinitions.Map.AcquiredAnimation, "map station acquired animation");
        AssertTrue(StationAccessRuleDefinitions.Energy.AcquiredAnimation is null, "energy station has one animation");
        AssertEqual(0xad66, StationNativeWords.AnimationLists.Word(StationAnimationList.MapIdle), "map idle list word");

        // The deleted RomData catalogs' values.
        AssertEqual(5, DownwardGateDefinitions.GateHeightInBlocks, "gate height");
        AssertEqual(RoomBlockBehaviorCodec.Decode(0x10), DownwardGateDefinitions.ClosedGateBehavior, "closed gate behaviour");
        AssertEqual(0x1000, DownwardGateDefinitions.OneBlockDistance, "gate projectile block distance");
        AssertEqual(RoomCollisionType.SolidBlock, NoobTubeObjectDefinitions.OriginCollision, "n00b tube origin collision");
        AssertEqual(11, NativeEarthquakeTypes.TypeOf(NoobTubeObjectDefinitions.Earthquake), "n00b tube earthquake type");
        AssertEqual(0x0040, NoobTubeObjectDefinitions.EarthquakeFrames, "n00b tube earthquake length");
        AssertEqual(0xfb00, NoobTubeDebrisDefinitions.BubbleInitialYVelocity.RawValue, "bubble velocity");
        AssertEqual(0x00c0, NoobTubeDebrisDefinitions.CrackFallVelocity.RawValue, "crack velocity");
        AssertEqual(0x00c0, NoobTubeDebrisDefinitions.ShardFallYVelocity.RawValue, "shard fall velocity");
        AssertEqual(0xee00, NoobTubeDebrisDefinitions.HiddenXPosition, "hidden actor X");
        AssertEqual(0x0ae0, SpeedBoosterEscapeDefinitions.StartFxMotionSamusX, "lavaquake start X");
        AssertEqual(0xff80, SpeedBoosterEscapeDefinitions.InitialLavaquakeVelocity.RawValue, "lavaquake velocity");
        AssertEqual(0xffe0, SpeedBoosterPickupDefinitions.LavaRiseVelocity.RawValue, "pickup lava velocity");
        AssertEqual(16, SamusEaterObjectDefinitions.HeldInvincibilityFrames, "plant hold sustained invincibility frames");
        AssertEqual(48, SamusEaterObjectDefinitions.ReleaseInvincibilityFrames, "plant release invincibility");
        AssertEqual(2, SamusEaterObjectDefinitions.ChewDamage, "plant chew damage");
        AssertEqual(RoomTerrainBlock.Of(0x123, BlockMirroring.None, RoomCollisionType.SolidBlock), RoomTerrainBlock.Of(0x123, BlockMirroring.None, RoomCollisionType.SpecialBlock).Deactivated(), "plant trigger deactivation leaves a solid block solid with its appearance");
        AssertEqual(null, MetroidsClearedEvents.ResolveEvent(8), "ninth metroids-cleared entry is a no-op");
        AssertEqual((StoryEvent?)StoryEvent.FirstMetroidHallCleared, MetroidsClearedEvents.ResolveEvent(9), "first quota observer");
        AssertEqual((StoryEvent?)StoryEvent.SecondMetroidShaftCleared, MetroidsClearedEvents.ResolveEvent(12), "last quota observer");

        // Terrain blocks keep the collision kind and appearance of the former literal words.
        AssertEqual(RoomCollisionType.SolidBlock, MaridiaElevatubeObjectDefinitions.Collision, "elevatube block");
        AssertEqual(RoomBlockAppearance.Of(0x180), InstalledRoomObjectMaridiaElevatubeVisuals.Stock().Image(MaridiaElevatubeObjectDefinitions.BlockDraw).At(new(0, 0)),
            "elevatube block picture");

        AssertEqual((RoomCollisionType?)RoomCollisionType.ShootableBlock, GateObjects.DownwardGateShotBlock(0).LeftCollision, "blue-left gate trigger collision");
        AssertEqual((RoomCollisionType?)RoomCollisionType.ShootableBlock, GateObjects.DownwardGateShotBlock(7).RightCollision, "yellow-right gate trigger collision");
        // A restoration's geometry holds its collision; what it shows is its installed picture (#1557).
        InstalledRoomObjectLinkedRestoreVisuals restorations = InstalledRoomObjectLinkedRestoreVisuals.Stock();
        var bombHorizontal = new LinkedRestoreObjectFamily.Draw(RoomObjectRestoreShape.Horizontal, LinkedRestoreObjectFamily.BombBlock);
        AssertEqual(RoomCollisionType.BombableBlock, bombHorizontal.CollisionAt(0, 0), "bomb block parent");
        AssertEqual(RoomCollisionType.HorizontalExtension, bombHorizontal.CollisionAt(0, 1), "bomb block horizontal child");
        RoomObjectFrameImage bombPicture = restorations.Image(RoomObjectDraw.Of(BombBlockRestoreDraw.Horizontal));
        AssertEqual(RoomBlockAppearance.Of(0x058), bombPicture.At(new(0, 0)), "bomb block parent picture");
        AssertEqual(RoomBlockAppearance.Of(0x058), bombPicture.At(new(1, 0)), "bomb block horizontal child picture");
        var crumbleVertical = new LinkedRestoreObjectFamily.Draw(RoomObjectRestoreShape.Vertical, LinkedRestoreObjectFamily.ContactCrumble);
        AssertEqual(RoomCollisionType.SpecialBlock, crumbleVertical.CollisionAt(0, 0), "contact crumble parent");
        AssertEqual(RoomCollisionType.VerticalExtension, crumbleVertical.CollisionAt(0, 1), "contact crumble vertical child");
        RoomObjectFrameImage crumblePicture = restorations.Image(RoomObjectDraw.Of(ContactCrumbleRestoreDraw.Vertical));
        AssertEqual(RoomBlockAppearance.Of(0x0bc), crumblePicture.At(new(0, 0)), "contact crumble parent picture");
        AssertEqual(RoomBlockAppearance.Of(0x0bc), crumblePicture.At(new(0, 1)), "contact crumble vertical child picture");

        // The blocks setups and reactions write of their own accord are installed pictures that show what the deleted
        // compiled appearances held (#1580).
        InstalledRoomObjectSetupBlocks setupBlocks = InstalledRoomObjectSetupBlocks.Stock();
        AssertEqual(RoomBlockAppearance.Of(0x058), setupBlocks.Shown(RoomObjectSetupBlock.BombBlockParent), "bomb block parent setup picture");
        AssertEqual(RoomBlockAppearance.Of(0x0bc), setupBlocks.Shown(RoomObjectSetupBlock.CrumbleBlockParent), "crumble block parent setup picture");
        AssertEqual(RoomBlockAppearance.Of(0x0b6), setupBlocks.Shown(RoomObjectSetupBlock.SpeedBoosterBlockParent), "Speed Booster block parent setup picture");
        AssertEqual(RoomBlockAppearance.Of(0x052), setupBlocks.Shown(RoomObjectSetupBlock.ShotBlockParent), "shot block parent setup picture");
        AssertEqual(RoomBlockAppearance.Of(0x057), setupBlocks.Shown(RoomObjectSetupBlock.PowerBombBlockParent), "Power Bomb block parent setup picture");
        AssertEqual(RoomBlockAppearance.Of(0x09f), setupBlocks.Shown(RoomObjectSetupBlock.SuperMissileBlockParent), "Super Missile block parent setup picture");
        AssertEqual(RoomBlockAppearance.Of(0x09f), setupBlocks.Shown(RoomObjectSetupBlock.CritterWallStruck), "struck rescue wall setup picture");
        AssertEqual(RoomBlockAppearance.Of(0x0ff), setupBlocks.Shown(RoomObjectSetupBlock.TreadmillIdle), "idle treadmill row setup picture");
        AssertEqual(RoomBlockAppearance.Of(0x0ff), setupBlocks.Shown(RoomObjectSetupBlock.TreadmillRunning), "running treadmill row setup picture");

        Console.WriteLine("PLM definition migration: header words, member identifications, station animations, catalog values and terrain blocks keep their values.");
    }

    /// <summary>Runs the focused PLM definition-migration check.</summary>
    [VerificationCommand("--plm-definition-migration")]
    internal static int RoomObjectDefinitionMigrationCommand()
    {
        VerifyObjectDefinitionMigration();
        return 0;
    }
}
