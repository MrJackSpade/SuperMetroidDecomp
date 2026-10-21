using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Progression;
namespace SuperMetroid.Verification.Fx;
/// <summary>Checks conveyor inside-block reactions, standing carry,
/// Wrecked Ship power gating, and solid-wall clipping.</summary>
internal static class TreadmillPhysicsVerification
{
    /// <summary>Uses a four-by-four terrain fixture to verify every authored
    /// conveyor direction and the resulting production horizontal motion.</summary>
    internal static void VerifyTreadmillPhysics()
    {
        ushort[] words = new ushort[16];
        byte[] bts = new byte[16];
        for (int i = 8; i < 12; i++) { words[i] = 0x30ff; bts[i] = 8; }
        for (int i = 12; i < 16; i++) words[i] = 0x8000;
        var level = new RoomLevelData(new Extent16<RoomBlocks>(4, 4), new RoomLevelPlanes(words, bts, new ushort[16]), new byte[8]);
        var samus = new SamusState { Position = new(new(24, 0), new(40, 0))};
        samus.Kinematics.Radius = samus.Kinematics.Radius.WithY(8);
        SamusInsideBlockReactions.PrepareFrame(level, samus, AreaId.Crateria);
        AssertEqual(2 << 16, samus.Kinematics.ExtraDisplacement.X.RawValue, "treadmill inside reaction publishes carry");
        SamusGroundedMovement.StepStanding(level, samus, 0, controllerInput: 0);
        AssertEqual((ushort)26, samus.Position.X.Whole, "standing movement consumes treadmill carry");
        int cases = 0;
        foreach (byte behavior in new byte[] { 8, 9, 10, 11 })
        foreach (AreaId area in new[] { AreaId.Crateria, AreaId.WreckedShip })
        foreach (bool powered in new[] { false, true })
        foreach (ushort verticalSpeed in new ushort[] { 0, 1 })
        {
            for (int i = 8; i < 12; i++) level.SetBehavior(i, behavior);
            samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(24, 40)) };
            var body = samus.Kinematics;
            body.Radius = body.Radius.WithY(8);
            body.YSpeed = new SnesUnsignedSixteenSixteen(verticalSpeed, 0xffff); // Native admission does not inspect the fractional word.
            body.ExtraDisplacement = body.ExtraDisplacement.WithX(SuperMetroid.Core.Spatial.SnesSignedSixteenSixteen.FromParts(7, 0x1234));
            samus.HorizontalSpeed.SelectEnvironmentSpeedTable(SamusLiquidMedium.Water);
            bool admitted = behavior >= 10 || (verticalSpeed == 0 && (area != AreaId.WreckedShip || powered));
            int expected = admitted ? SnesSixteenSixteen.FromWhole((behavior & 1) == 0 ? 2 : -2) : 0x71234;
            SamusInsideBlockReactions.PrepareFrame(level, samus, area, powered);
            AssertEqual(expected, body.ExtraDisplacement.X.RawValue, $"conveyor BTS {behavior}, area {area}, power {powered}, Y {verticalSpeed}");
            AssertEqual(SamusMovementRomData.HorizontalMotion.NormalAirSpeedTable,
                samus.HorizontalSpeed.ActiveSpeedTableBaseAddress, "conveyor inside handler restores normal speed pointer");
            if (admitted)
            {
                SamusGroundedMovement.StepStanding(level, samus, 0, controllerInput: 0);
                AssertEqual((ushort)(24 + SnesSixteenSixteen.WholeOf(expected)), samus.Position.X.Whole, "conveyor actual horizontal motion");
            }
            cases++;
        }
        // A solid obstruction must clip the carried displacement through the ordinary mover.
        level.SetForegroundEntry(10, 0x8000);
        level.SetBehavior(9, 8);
        samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(27, 40)) };
        samus.Kinematics.Radius = samus.Kinematics.Radius.WithY(8);
        SamusInsideBlockReactions.PrepareFrame(level, samus, AreaId.Crateria);
        SamusGroundedMovement.StepStanding(level, samus, 0, controllerInput: 0);
        AssertEqual((ushort)27, samus.Position.X.Whole, "conveyor carry clips at solid wall");
        Console.WriteLine($"  Conveyors: {cases} direction/power/vertical-speed cases, actual carry and wall clipping agree.");
    }
}
