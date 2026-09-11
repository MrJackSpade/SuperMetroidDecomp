using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyGrappleBlueDoors()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
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
            var level = new RoomLevelData(width, width, words, bts, new ushort[words.Length], new byte[8]);
            int hitBlock = origin + hitOffset * stride;
            var samus = new SamusState
            {
                XPosition = (ushort)(hitBlock % width * 16 + 8),
                YPosition = (ushort)(hitBlock / width * 16 + 8),
            };
            // Seed an extending beam exactly at each cap tile. Collision still traverses
            // the real StepFiring substeps and extension-chain resolver, not a PLM shortcut.
            SeedStationaryGrappleCollisionProbe(bus, samus);
            var plms = new RoomPlmSystem();
            var result = SamusGrappleMovement.StepFiring(bus, level, samus, (ushort)SnesButton.X, plms);
            AssertTrue(result.CancelQueued, $"grapple cancels on blue {orientation} cap {hitOffset}");
            AssertEqual(1, plms.ActiveCount, "grapple allocates one blue-door opening actor");
            AssertEqual(RoomCollisionType.SolidBlock, level.GetCollisionBlockByIndex(origin).CollisionType,
                "blue-door setup changes origin before animation");
            var streamer = level.CreateBackgroundStreamer();
            bool heardOpening = false;
            for (int frame = 0; frame < 19; frame++)
            {
                plms.Step(bus, level, streamer, 0x1000, 0x1000, 0);
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
    private static void SeedStationaryGrappleCollisionProbe(ISnesAddressSpace bus, SamusState samus)
    {
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.Grapple.Phase = GrapplePhase.Firing;
        samus.Grapple.FireDirection = samus.ReadShotDirection(bus);
    }
}
