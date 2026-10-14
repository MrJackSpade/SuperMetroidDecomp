using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

/// <summary>Checks a crouched Samus walking off a floor receives the native
/// center and subpixel correction before the falling radius is published.</summary>
internal static class CompactWalkOffCollisionVerification
{
    /// <summary>Exercises both facings against a synthetic floor and verifies
    /// pose, radius timing, vertical speed, and corrected Y position.</summary>
    internal static void VerifyCompactWalkOffCollision()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var words = new ushort[16 * 32];
        for (int x = 0; x < 16; x++) words[16 * 16 + x] = 0x8000;
        var level = new RoomLevelData(new Extent16<RoomBlocks>(16, 32), new RoomLevelPlanes(words, new byte[words.Length], new ushort[words.Length]), new byte[8]);
        foreach (bool left in new[] { false, true })
        {
            // Native soft-unmorph comparison: the neighboring ordinary landing's
            // grounding probe leaves a compact body near the floor. Falling expands
            // that body, so F404 must shift its center and clamp its fractional word.
            var samus = new SamusState
            {
                Pose = left ? SamusPoseId.CrouchingLeftPose : SamusPoseId.CrouchingRightPose,
                Position = FixedPosition<RoomPixels>.FromWhole(new(80, 240)),
            };
            samus.PoseMetadata.RefreshCollisionRadii(bus);
            samus.Presentation.InitializeAnimation(bus);
            samus.Kinematics.Position = samus.Kinematics.Position.WithFractionY(0x6400);
            samus.Kinematics.YSpeed = 2;
            samus.Kinematics.YSubspeed = 0xf400;
            SamusPoseId target = left ? SamusPoseId.FallingLeftPose : SamusPoseId.FallingRightPose;
            samus.PoseTransitions.ApplyWalkedOffFloorTransition(bus, level, target);
            AssertEqual(0x00edffffu, samus.Kinematics.Position.Y.RawValue, "compact walk-off uses native floor center/subpixel correction");
            AssertEqual(target, samus.Pose, "compact walk-off selects ordinary falling art");
            AssertEqual(16, samus.Kinematics.Radius.Y, "compact walk-off retains live crouch radius until alpha");
            samus.PoseMetadata.RefreshCollisionRadii(bus);
            AssertEqual(19, samus.Kinematics.Radius.Y, "compact walk-off next alpha publishes falling radius");
            AssertEqual(0x00edffffu, samus.Kinematics.Position.Y.RawValue, "radius publication does not repeat compact correction");
            AssertEqual(0, samus.Kinematics.YSpeed, "accepted falling command clears whole vertical speed");
            AssertEqual(0, samus.Kinematics.YSubspeed, "accepted falling command clears fractional vertical speed");
            AssertEqual(SamusVerticalDirection.Down, samus.Kinematics.YDirection, "accepted falling command starts downward motion");
        }
    }
}
