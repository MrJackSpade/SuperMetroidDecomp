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
        AssertEqual(0x10, DownwardGateDefinitions.ClosedGateBts, "closed gate behaviour");
        AssertEqual(0x1000, DownwardGateDefinitions.OneBlockDistance, "gate projectile block distance");
        AssertEqual(2, DownwardGateDefinitions.BlockAssociationScale, "gate projectile block association");
        AssertEqual(0x8044, NoobTubePlmDefinitions.SolidProjectileTriggerLevelWord.Raw, "n00b tube origin block");
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
        AssertEqual(0xb123 & 0x8fff, new RoomLevelWord(0xb123).WithPlmDeactivation().Raw, "plant trigger deactivation keeps the former mask's bits");
        AssertEqual(null, MetroidsClearedEvents.ResolveEvent(16), "ninth metroids-cleared entry is a no-op");
        AssertEqual((EventNumber?)EventNumber.FirstMetroidHallCleared, MetroidsClearedEvents.ResolveEvent(18), "first quota observer");
        AssertEqual((EventNumber?)EventNumber.SecondMetroidShaftCleared, MetroidsClearedEvents.ResolveEvent(24), "last quota observer");

        // Level words built from collision type and visual block equal the former literal words.
        AssertEqual(0x8180, MaridiaElevatubePlmDefinitions.PhysicalWord.Raw, "elevatube level word");
        AssertEqual(0xc046, GatePlms.DownwardGateShotBlock(0).LeftBlockWord, "blue-left gate trigger block");
        AssertEqual(0xc04d, GatePlms.DownwardGateShotBlock(14).RightBlockWord, "yellow-right gate trigger block");
        var bombHorizontal = new RoomPlmLinkedRestoreDefinition.Draw(PlmDraw.Of(BombBlockRestoreDraw.Horizontal), PlmRestoreShape.Horizontal, RoomPlmLinkedRestoreDefinition.BombBlock);
        AssertEqual(0xf058, bombHorizontal.WordAt(0, 0), "bomb block parent");
        AssertEqual(0x5058, bombHorizontal.WordAt(0, 1), "bomb block horizontal child");
        var crumbleVertical = new RoomPlmLinkedRestoreDefinition.Draw(PlmDraw.Of(ContactCrumbleRestoreDraw.Vertical), PlmRestoreShape.Vertical, RoomPlmLinkedRestoreDefinition.ContactCrumble);
        AssertEqual(0xb0bc, crumbleVertical.WordAt(0, 0), "contact crumble parent");
        AssertEqual(0xd0bc, crumbleVertical.WordAt(0, 1), "contact crumble vertical child");
        Console.WriteLine("PLM definition migration: header words, member identifications, station animations, catalog values and level words keep their values.");
    }

    /// <summary>Runs the focused PLM definition-migration check.</summary>
    [VerificationCommand("--plm-definition-migration")]
    internal static int PlmDefinitionMigrationCommand()
    {
        VerifyPlmDefinitionMigration();
        return 0;
    }
}
