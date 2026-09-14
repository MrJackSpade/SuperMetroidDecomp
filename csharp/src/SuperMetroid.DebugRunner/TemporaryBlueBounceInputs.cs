using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;

/// <summary>Ordinary/Spring Ball bounce controls and native boost-loss boundaries.</summary>
internal static class TemporaryBlueBounceInputs
{
    public static ushort At(int frame, bool left, int mode)
    {
        if (frame <= 422) return TemporaryBlueCarryInputs.At(frame, left, 4);
        SnesButton input = SnesButton.A | (left ? SnesButton.Left : SnesButton.Right);
        int control = mode % 4;
        if (control == 1 || control == 3 && frame < 500) input &= ~SnesButton.A;
        if (control == 2 && frame >= 480) input = SnesButton.A;
        return (ushort)input;
    }

    public static void Verify(SamusState samus, int frame, int mode)
    {
        if (frame < 399) return;
        int lossFrame = mode switch { 0 or 1 or 5 => 518, 2 or 6 => 480, 3 => 508, _ => int.MaxValue };
        if (samus.HorizontalSpeed.SpeedBoostCounter != (frame < lossFrame ? 0x0401 : 0))
            throw new InvalidDataException("Ordinary/Spring Ball boost changed at the wrong bounce/input boundary.");
        if (frame == 799 && mode is 4 or 7 &&
            (samus.Pose is not (SamusPoseIds.SpringBallFallingRightPose or SamusPoseIds.SpringBallFallingLeftPose) ||
             samus.Kinematics.YFixed >= 0x01f90000))
            throw new InvalidDataException("Spring Ball must actually be airborne after repeated bouncing.");
    }
}
