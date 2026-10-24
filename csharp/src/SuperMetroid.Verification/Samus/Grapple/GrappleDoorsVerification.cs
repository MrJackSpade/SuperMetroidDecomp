using static SuperMetroid.Verification.Samus.Grapple.GrapplePoseRefireVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
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
            var words = new ushort[width * width];
            var bts = new byte[words.Length];
            words[origin] = (int)RoomCollisionType.ShootableBlock << 12;
            bts[origin] = (byte)(RoomBlockBehaviorValues.BlueDoorFacingLeft.Value + (int)orientation);
            for (int offset = 1; offset < 4; offset++)
            {
                words[origin + offset * stride] = (ushort)((int)(verticalCap
                    ? RoomCollisionType.VerticalExtension : RoomCollisionType.HorizontalExtension) << 12);
                bts[origin + offset * stride] = unchecked((byte)-offset);
            }
            var level = new RoomLevelData(new Extent16<RoomBlocks>(width, width), new RoomLevelPlanes(words, bts, new ushort[words.Length]), new byte[8]);
            int hitBlock = origin + hitOffset * stride;
            var samus = new SamusState
            {
                Position = FixedPosition<RoomPixels>.FromWhole(new((ushort)(hitBlock % width * 16 + 8), (ushort)(hitBlock / width * 16 + 8))),
            };
            // Seed an extending beam exactly at each cap tile. Collision still traverses
            // the real StepFiring substeps and extension-chain resolver, not a PLM shortcut.
            SeedStationaryGrappleCollisionProbe(samus);
            var plms = PlmFixtures.Unbound();
            var result = SamusGrappleMovement.StepFiring(level, samus, SnesButton.X, plms);
            AssertTrue(result.Phase == GrapplePhase.CancelPending, $"grapple cancels on blue {orientation} cap {hitOffset}");
            AssertEqual(1, plms.SlotTable.ActiveCount, "grapple allocates one blue-door opening actor");
            AssertEqual(RoomCollisionType.SolidBlock, level.GetCollisionBlockByIndex(origin).CollisionType,
                "blue-door setup changes origin before animation");
            bool heardOpening = false;
            for (int frame = 0; frame < 19; frame++)
            {
                plms.Step(level);
                heardOpening |= plms.Sounds.SoundRequests.Any(r => r == new SoundRequest(
                    Library3Sound.DoorOpened, SoundQueueLimit.Max6));
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
        samus.Grapple.FireDirection = (SuperMetroid.Core.Samus.Projectiles.SamusProjectileDirection)SamusPoseMetadata.ReadShotDirection(samus);
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
