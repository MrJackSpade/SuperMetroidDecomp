using SuperMetroid.AssetExtraction;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Samus.Grapple.GrapplePoseRefireVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Samus.Grapple;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Audio;

/// <summary>Checks that firing grapple into any blue-door cap opens the door.</summary>
internal static class GrappleDoorsVerification
{
    /// <summary>Exercises four orientations and all cap cells through extension collision and PLM animation.</summary>
    internal static void VerifyGrappleBlueDoors()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach (ColoredDoorOrientation orientation in Enum.GetValues<ColoredDoorOrientation>())
        for (int hitOffset = 0; hitOffset < 4; hitOffset++)
        {
            const int width = 16;
            const int origin = 4 * width + 4;
            bool verticalCap = orientation is ColoredDoorOrientation.Left or ColoredDoorOrientation.Right;
            int stride = verticalCap ? width : 1;
            var words = new RoomTerrainBlock[width * width];
            var bts = new RoomBlockBehavior[words.Length];
            words[origin] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.ShootableBlock);
            bts[origin] = RoomBlockBehaviorCodec.Decode(RoomBlockBehaviorCodec.Encode(RoomBlockBehavior.Reacting(BlockReactionDefinitions.BlueDoorCaps.ReactionFor(ColoredDoorOrientation.Left))) + (int)orientation);
            for (int offset = 1; offset < 4; offset++)
            {
                words[origin + offset * stride] = RoomTerrainBlock.Of(0, BlockMirroring.None, verticalCap
                    ? RoomCollisionType.VerticalExtension : RoomCollisionType.HorizontalExtension);
                bts[origin + offset * stride] = RoomBlockBehavior.Extending(-offset);
            }
            var level = new RoomLevelData(new Extent16<RoomBlocks>(width, width), new RoomLevelPlanes(words, bts, new RoomBlockAppearance[words.Length]), RoomLevelData.NothingBeyondGrid());
            int hitBlock = origin + hitOffset * stride;
            var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
            {
                Position = FixedPosition<RoomPixels>.FromWhole(new((ushort)(hitBlock % width * 16 + 8), (ushort)(hitBlock / width * 16 + 8))),
            };
            // Seed an extending beam exactly at each cap tile. Collision still traverses
            // the real StepFiring substeps and extension-chain resolver, not a PLM shortcut.
            SeedStationaryGrappleCollisionProbe(samus);
            var roomObjects = RoomObjectFixtures.Unbound();
            var result = SamusGrappleMovement.StepFiring(level, samus, ControllerButton.X, roomObjects);
            AssertTrue(result.Phase == GrapplePhase.CancelPending, $"grapple cancels on blue {orientation} cap {hitOffset}");
            AssertEqual(1, roomObjects.SlotTable.ActiveCount, "grapple allocates one blue-door opening actor");
            AssertEqual(RoomCollisionType.SolidBlock, level.GetCollisionBlockByIndex(origin).CollisionType,
                "blue-door setup changes origin before animation");
            bool heardOpening = false;
            for (int frame = 0; frame < 19; frame++)
            {
                roomObjects.Step(level);
                heardOpening |= roomObjects.Sounds.SoundRequests.Any(r => r == new SoundRequest(
                    RoomSound.DoorOpened, SoundRequestPriority.Normal));
            }
            AssertTrue(heardOpening, "grapple-opened blue door emits its native opening sound");
            for (int offset = 0; offset < 4; offset++)
                AssertEqual(RoomCollisionType.Air, level.GetCollisionBlockByIndex(origin + offset * stride).CollisionType,
                    "grapple-opened door clears every cap block after the retail animation");
        }
        Console.WriteLine("  Grapple blue doors: all four orientations and sixteen cap/extension hits open with native timing and sound.");
    }

    /// <summary>Seeds an endpoint collision probe without an impossible forward-facing firing pose.</summary>
    internal static void SeedStationaryGrappleCollisionProbe(SamusState samus)
    {
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        samus.Grapple.Phase = GrapplePhase.Firing;
        samus.Grapple.FireDirection = SamusPoseMetadata.ReadAimDirection(samus)
            ?? throw new InvalidOperationException("The grapple probe's standing pose aims in a direction.");
    }

    /// <summary>Runs door opening and grapple pose-refire checks together.</summary>
    [VerificationCommand("--grapple-doors")]
    internal static int GrappleDoorsCommand()
    {
        Suite(nameof(VerifyGrappleBlueDoors), () => VerifyGrappleBlueDoors());
        Suite(nameof(VerifyGrapplePoseRefire), () => VerifyGrapplePoseRefire());
        return 0;
    }
}
