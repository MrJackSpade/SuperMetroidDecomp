using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyTreadmillPhysics()
    {
        var bus = new TestAddressSpace();
        ushort[] words = new ushort[16];
        byte[] bts = new byte[16];
        for (int i = 8; i < 12; i++) { words[i] = 0x30ff; bts[i] = 8; }
        for (int i = 12; i < 16; i++) words[i] = 0x8000;
        var level = new RoomLevelData(4, 4, words, bts, new ushort[16], new byte[8]);
        var samus = new SamusState { XPosition = 24, YPosition = 40 };
        samus.Kinematics.YRadius = 8;
        SamusInsideBlockReactions.PrepareFrame(bus, level, samus, AreaId.Crateria);
        AssertEqual(2 << 16, samus.Kinematics.ExtraXFixed, "treadmill inside reaction publishes carry");
        SamusGroundedMovement.StepStandingRight(bus, level, samus, 0);
        AssertEqual((ushort)26, samus.XPosition, "standing movement consumes treadmill carry");
        int cases = 0;
        foreach (byte behavior in new byte[] { 8, 9, 10, 11 })
        foreach (AreaId area in new[] { AreaId.Crateria, AreaId.WreckedShip })
        foreach (bool powered in new[] { false, true })
        foreach (ushort verticalSpeed in new ushort[] { 0, 1 })
        {
            for (int i = 8; i < 12; i++) level.SetBehavior(i, behavior);
            samus = new SamusState { XPosition = 24, YPosition = 40 };
            var body = samus.Kinematics;
            body.YRadius = 8;
            body.YSpeed = verticalSpeed;
            body.YSubspeed = 0xffff; // Native admission does not inspect the fractional word.
            body.ExtraXDisplacement = 7;
            body.ExtraXSubdisplacement = 0x1234;
            samus.HorizontalSpeed.SelectEnvironmentSpeedTable(SamusLiquidPhysicsState.Water);
            bool admitted = behavior >= 10 || (verticalSpeed == 0 && (area != AreaId.WreckedShip || powered));
            int expected = admitted ? ((behavior & 1) == 0 ? 2 : -2) << 16 : 0x71234;
            SamusInsideBlockReactions.PrepareFrame(bus, level, samus, area, powered);
            AssertEqual(expected, body.ExtraXFixed, $"conveyor BTS {behavior}, area {area}, power {powered}, Y {verticalSpeed}");
            AssertEqual(SamusMovementRomData.HorizontalMotion.NormalAirSpeedTable,
                samus.HorizontalSpeed.ActiveSpeedTableBaseAddress, "conveyor inside handler restores normal speed pointer");
            if (admitted)
            {
                SamusGroundedMovement.StepStandingRight(bus, level, samus, 0);
                AssertEqual((ushort)(24 + (expected >> 16)), samus.XPosition, "conveyor actual horizontal motion");
            }
            cases++;
        }
        // A solid obstruction must clip the carried displacement through the ordinary mover.
        level.SetForegroundEntry(10, 0x8000);
        level.SetBehavior(9, 8);
        samus = new SamusState { XPosition = 27, YPosition = 40 };
        samus.Kinematics.YRadius = 8;
        SamusInsideBlockReactions.PrepareFrame(bus, level, samus, AreaId.Crateria);
        SamusGroundedMovement.StepStandingRight(bus, level, samus, 0);
        AssertEqual((ushort)27, samus.XPosition, "conveyor carry clips at solid wall");
        Console.WriteLine($"  Conveyors: {cases} direction/power/vertical-speed cases, actual carry and wall clipping agree.");
    }
}
