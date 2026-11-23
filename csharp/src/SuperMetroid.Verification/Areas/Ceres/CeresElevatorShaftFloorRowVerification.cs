using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Areas.Ceres;

/// <summary>
/// The Ceres elevator shaft's turning floor shows the room from the row below the camera's, as every gameplay view does
/// (#2617): a level shaft's top line shows the same room row an ordinary room's would. One row lower, the floor showed the
/// landing pad's top row below the arrival's concealer, a flat line in the pad's slot before the real pad landed.
/// </summary>
internal static class CeresElevatorShaftFloorRowVerification
{
    /// <summary>Camera rows the check places the shaft's camera at: the room's top, a row above it and a row inside it.</summary>
    private static readonly int[] CameraRows = [0, -1, 37];

    /// <summary>Checks the level shaft's top line against the ordinary view's shown row for each camera row.</summary>
    internal static void VerifyElevatorShaftShowsTheRowBelowTheCamera()
    {
        TurningFloorTransform level = TurningFloorTransform.Level(CeresShaftRotationDefinitions.RotationCenter);
        foreach (int row in CameraRows)
        {
            var camera = new Offset16<RoomPixels>(0, row);
            FloorView floor = CeresPerspectiveFloors.ElevatorShaft(level, camera).Transform;
            AssertEqual(0d, floor.Angle, "the level shaft does not turn");
            AssertEqual(RoomView.ShownRow(row), (int)floor.Scroll.Y,
                $"with the camera on row {row}, the shaft's top line shows the room row an ordinary view shows");
        }
    }

    /// <summary>Runs the elevator shaft floor row check.</summary>
    [VerificationCommand("--ceres-elevator-shaft-floor-row")]
    internal static int CeresElevatorShaftFloorRowCommand()
    {
        Suite(nameof(VerifyElevatorShaftShowsTheRowBelowTheCamera), VerifyElevatorShaftShowsTheRowBelowTheCamera);
        return 0;
    }
}
