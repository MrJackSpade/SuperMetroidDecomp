using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using static GrapplePoseRefireVerification;
using static SuiteTimingVerification;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Spatial;

internal static class GrappleDoorsVerification
{
    internal static void VerifyGrappleBlueDoors()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach (ColoredDoorOrientation orientation in Enum.GetValues<ColoredDoorOrientation>())
        for (int hitOffset = 0; hitOffset < 4; hitOffset++)
        {
            const int width = 16;
            const int origin = 4 * width + 4;
            bool verticalCap = orientation is ColoredDoorOrientation.Left or ColoredDoorOrientation.Right;
            int stride = verticalCap ? width : 1;
            var words = new ushort[width * width];
            var bts = new byte[words.Length];
            words[origin] = (ushort)((int)RoomCollisionType.ShootableBlock << 12);
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
            SeedStationaryGrappleCollisionProbe(bus, samus);
            var plms = new RoomPlmSystem();
            var result = SamusGrappleMovement.StepFiring(bus, level, samus, (ushort)SnesButton.X, plms);
            AssertTrue(result.Phase == GrapplePhase.CancelPending, $"grapple cancels on blue {orientation} cap {hitOffset}");
            AssertEqual(1, plms.ActiveCount, "grapple allocates one blue-door opening actor");
            AssertEqual(RoomCollisionType.SolidBlock, level.GetCollisionBlockByIndex(origin).CollisionType,
                "blue-door setup changes origin before animation");
            var streamer = level.CreateBackgroundStreamer();
            bool heardOpening = false;
            for (int frame = 0; frame < 19; frame++)
            {
                plms.Step(bus, level, streamer, new PlmView { Layer1Position = new(0x1000, 0x1000), Bg1XOffset = 0 });
                heardOpening |= plms.SoundRequests.Any(r => r == new PlmSoundRequest(
                    SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 7), 6));
            }
            AssertTrue(heardOpening, "grapple-opened blue door emits its native opening sound");
            for (int offset = 0; offset < 4; offset++)
                AssertEqual(RoomCollisionType.Air, level.GetCollisionBlockByIndex(origin + offset * stride).CollisionType,
                    "grapple-opened door clears every cap block after the retail animation");
        }
        Console.WriteLine("  Grapple blue doors: all four orientations and sixteen cap/extension hits open with native timing and sound.");
    }

    /// <summary>Seeds an endpoint collision probe without an impossible forward-facing firing pose.</summary>
    internal static void SeedStationaryGrappleCollisionProbe(ISnesAddressSpace bus, SamusState samus)
    {
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        samus.Grapple.Phase = GrapplePhase.Firing;
        samus.Grapple.FireDirection = samus.ReadShotDirection(bus);
    }

    [VerificationCommand("--grapple-doors")]
    internal static int GrappleDoorsCommand()
    {
        Suite(nameof(VerifyGrappleBlueDoors), () => VerifyGrappleBlueDoors());
        Suite(nameof(VerifyGrapplePoseRefire), () => VerifyGrapplePoseRefire());
        return 0;
    }
}
