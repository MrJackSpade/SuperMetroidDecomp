using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Progression;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
namespace SuperMetroid.Verification.Fx;
/// <summary>Checks conveyor inside-block reactions, standing carry,
/// Wrecked Ship power gating, and solid-wall clipping.</summary>
internal static class TreadmillPhysicsVerification
{
    /// <summary>Uses a four-by-four terrain fixture to verify every authored
    /// conveyor direction and the resulting production horizontal motion.</summary>
    internal static void VerifyTreadmillPhysics()
    {
        var terrain = new RoomTerrainBlock[16];
        var behavior = new RoomBlockBehavior[16];
        terrain.AsSpan().Fill(Air);
        behavior.AsSpan().Fill(RoomBlockBehaviorValues.None);
        for (int i = 8; i < 12; i++)
        {
            terrain[i] = WreckedShipTreadmillBlockDefinitions.ActiveTreadmillBlock;
            behavior[i] = WreckedShipTreadmillBlockDefinitions.RightwardsBehavior;
        }
        for (int i = 12; i < 16; i++) terrain[i] = Solid;
        var level = CreateRoom(new(4, 4), terrain, behavior);
        var samus = new SamusState { Position = new(new(24, 0), new(40, 0))};
        samus.Kinematics.Radius = samus.Kinematics.Radius.WithY(8);
        SamusInsideBlockReactions.PrepareFrame(level, samus, AreaId.Crateria);
        AssertEqual(2 << 16, samus.Kinematics.ExtraDisplacement.X.RawValue, "treadmill inside reaction publishes carry");
        SamusGroundedMovement.StepStanding(level, samus, 0, controllerInput: 0);
        AssertEqual((ushort)26, samus.Position.X.Whole, "standing movement consumes treadmill carry");
        int cases = 0;
        foreach (ConveyorCase conveyor in Conveyors)
        foreach (AreaId area in new[] { AreaId.Crateria, AreaId.WreckedShip })
        foreach (bool powered in new[] { false, true })
        foreach (ushort verticalSpeed in new ushort[] { 0, 1 })
        {
            for (int i = 8; i < 12; i++) level.SetBehavior(i, conveyor.Behavior);
            samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(24, 40)) };
            var body = samus.Kinematics;
            body.Radius = body.Radius.WithY(8);
            body.YSpeed = new UnsignedSixteenSixteen(verticalSpeed, 0xffff); // Native admission does not inspect the fractional word.
            body.ExtraDisplacement = body.ExtraDisplacement.WithX(SuperMetroid.Core.Spatial.SignedSixteenSixteen.FromParts(7, 0x1234));
            samus.HorizontalSpeed.SelectEnvironmentSpeedTable(SamusLiquidMedium.Water);
            bool gated = conveyor.Behavior == WreckedShipTreadmillBlockDefinitions.RightwardsBehavior ||
                conveyor.Behavior == WreckedShipTreadmillBlockDefinitions.LeftwardsBehavior;
            bool admitted = !gated || (verticalSpeed == 0 && (area != AreaId.WreckedShip || powered));
            int expected = admitted ? SixteenSixteen.FromWhole(conveyor.Direction == WreckedShipTreadmillDirection.Rightwards ? 2 : -2) : 0x71234;
            SamusInsideBlockReactions.PrepareFrame(level, samus, area, powered);
            AssertEqual(expected, body.ExtraDisplacement.X.RawValue, $"conveyor behaviour {conveyor.Behavior}, area {area}, power {powered}, Y {verticalSpeed}");
            AssertEqual(SamusMovementDefinitions.HorizontalMotion.NormalAirSpeedTable,
                samus.HorizontalSpeed.ActiveSpeedTableBaseAddress, "conveyor inside handler restores normal speed pointer");
            if (admitted)
            {
                SamusGroundedMovement.StepStanding(level, samus, 0, controllerInput: 0);
                AssertEqual((ushort)(24 + SixteenSixteen.WholeOf(expected)), samus.Position.X.Whole, "conveyor actual horizontal motion");
            }
            cases++;
        }
        // A solid obstruction must clip the carried displacement through the ordinary mover.
        level.SetTerrain(10, Solid);
        level.SetBehavior(9, WreckedShipTreadmillBlockDefinitions.RightwardsBehavior);
        samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(27, 40)) };
        samus.Kinematics.Radius = samus.Kinematics.Radius.WithY(8);
        SamusInsideBlockReactions.PrepareFrame(level, samus, AreaId.Crateria);
        SamusGroundedMovement.StepStanding(level, samus, 0, controllerInput: 0);
        AssertEqual((ushort)27, samus.Position.X.Whole, "conveyor carry clips at solid wall");
        Console.WriteLine($"  Conveyors: {cases} direction/power/vertical-speed cases, actual carry and wall clipping agree.");
    }

    /// <summary>The fixture's open cells: air showing block zero.</summary>
    private static readonly RoomTerrainBlock Air = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.Air);

    /// <summary>The fixture's floor and wall: a solid block showing block zero.</summary>
    private static readonly RoomTerrainBlock Solid = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.SolidBlock);

    /// <summary>
    /// The four conveyor behaviours: the Wrecked Ship treadmill's rightwards and leftwards directions, which only carry a
    /// grounded Samus and, in the Wrecked Ship, only when it is powered; then the two ungated conveyors.
    /// </summary>
    private static readonly ConveyorCase[] Conveyors =
    [
        new(WreckedShipTreadmillBlockDefinitions.RightwardsBehavior, WreckedShipTreadmillDirection.Rightwards),
        new(WreckedShipTreadmillBlockDefinitions.LeftwardsBehavior, WreckedShipTreadmillDirection.Leftwards),
        new(new RoomBlockBehavior(10), WreckedShipTreadmillDirection.Rightwards),
        new(new RoomBlockBehavior(11), WreckedShipTreadmillDirection.Leftwards),
    ];

    /// <summary>One conveyor behaviour and the direction it carries Samus.</summary>
    private readonly record struct ConveyorCase(RoomBlockBehavior Behavior, WreckedShipTreadmillDirection Direction);
}
