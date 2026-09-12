using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifySamusBallBounceDefinitions(SuperMetroidAddressSpace rom)
    {
        ushort Word(int a) => (ushort)(rom.ReadByte(a) | rom.ReadByte(a + 1) << 8);
        ushort whole = Word(SamusMovementRomData.VerticalMotion.BallBounceSpeed);
        ushort fraction = Word(SamusMovementRomData.VerticalMotion.BallBounceSubspeed);
        AssertEqual(whole, SamusVerticalMotionDefinitions.BallBounceSpeed, "Native ball rebound whole definition");
        AssertEqual(fraction, SamusVerticalMotionDefinitions.BallBounceSubspeed, "Native ball rebound fraction definition");
        var guard = new ImpulsePoseReadGuard(rom);
        int cases = 0;
        foreach (bool spring in new[] { false, true })
        foreach (bool left in new[] { false, true })
        {
            byte pose = spring
                ? left ? SamusPoseIds.SpringBallFallingLeftPose : SamusPoseIds.SpringBallFallingRightPose
                : left ? SamusPoseIds.MorphBallFallingLeftPose : SamusPoseIds.MorphBallFallingRightPose;
            byte groundedPose = spring
                ? left ? SamusPoseIds.SpringBallGroundLeftPose : SamusPoseIds.SpringBallGroundRightPose
                : left ? SamusPoseIds.MorphBallGroundLeftPose : SamusPoseIds.MorphBallGroundRightPose;
            var samus = new SamusState { Pose = pose, XPosition = 128, YPosition = 128 };
            samus.RefreshCollisionRadii(guard);
            samus.InitializeAnimation(guard);
            for (int phase = 0; phase < 3; phase++)
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                samus.Pose = pose;
                samus.MorphBallBounceState = (ushort)(phase | (spring ? 0x0600 : 0));
                samus.Kinematics.YSpeed = (ushort)raw;
                samus.Kinematics.YSubspeed = ushort.MaxValue;
                samus.Kinematics.YDirection = 2;
                samus.HorizontalSpeed.BaseSpeed = 2;
                samus.HorizontalSpeed.BaseSubspeed = 0x8000;
                samus.HorizontalSpeed.AccelerationMode = 2;
                bool grounded = spring
                    ? samus.ApplySpringBallLanding(guard, controllerInput: 0)
                    : samus.ApplyMorphBallLanding(guard);
                bool first = phase == 0 && unchecked((short)(raw - 3)) >= 0;
                bool rebound = first || phase == 1;
                AssertEqual(!rebound, grounded, "Native rebound versus grounded return");
                AssertEqual(rebound ? (ushort)(first ? whole : whole - 1) : (ushort)0,
                    samus.Kinematics.YSpeed, "Native rebound whole decrement");
                AssertEqual(rebound ? fraction : (ushort)0, samus.Kinematics.YSubspeed, "Native rebound fraction");
                AssertEqual(rebound ? (ushort)1 : (ushort)0, samus.Kinematics.YDirection, "Native rebound direction");
                AssertEqual(rebound ? (ushort)((first ? 1 : 2) | (spring ? 0x0600 : 0)) : (ushort)0,
                    samus.MorphBallBounceState, "Native ball family/phase publication");
                AssertEqual(rebound ? pose : groundedPose, samus.Pose, "Rebound retains airborne pose until grounded");
                AssertEqual(rebound || spring ? 0x00028000u : 0u, samus.HorizontalSpeed.BaseFixed,
                    "Ordinary final landing alone clears horizontal base momentum");
                AssertEqual((ushort)128, samus.YPosition, "Landing handler does not move the collision-resolved center");
                cases++;
            }
        }
        Console.WriteLine($"Samus ball bounce definitions: {cases} actual signed-speed/phase/facing/family landings match native branches with physics reads forbidden.");
    }
}
