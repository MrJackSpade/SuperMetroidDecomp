using SuperMetroid.Core.Game;

/// <summary>Five complete controller-only carry cycles, including adjacent failure timings.</summary>
internal static class TemporaryBlueChainInputs
{
    public static ushort At(int frame, bool left, int mode)
    {
        int carryMode = mode switch
        {
            0 => 18, 1 => 19, 2 => 20, 3 => 26,
            4 => 27, 5 => 28, 6 => 38, 7 => 39,
            _ => throw new ArgumentOutOfRangeException(nameof(mode)),
        };
        int cycleFrame = frame < 400 ? frame : 400 + (frame - 400) % 120;
        return TemporaryBlueCarryInputs.At(cycleFrame, left, carryMode);
    }

    public static void Verify(SamusState samus, int frame, int mode)
    {
        if (frame < 399) return;
        bool success = mode is not (0 or 4);
        ushort expected = success || frame < 494 ? (ushort)0x0401 : (ushort)0;
        if (samus.HorizontalSpeed.SpeedBoostCounter != expected || samus.Shinespark.ShineTimer != 0)
            throw new InvalidDataException("Repeated carry changed its native boost-loss or charge-expiry boundary.");
        if (!success || frame < 400) return;
        int phase = (frame - 400) % 120;
        if (phase == 40 && samus.Kinematics.YFixed >= 0x01f00000)
            throw new InvalidDataException("Every successful carry cycle must become airborne.");
        if (phase == 119 &&
            (samus.Kinematics.YFixed != 0x01f0ffff ||
             samus.Pose is not (SamusPoseIds.CrouchingAimDiagonalUpRightPose or SamusPoseIds.CrouchingAimDiagonalUpLeftPose)))
            throw new InvalidDataException("Every successful carry cycle must finish with the native crouched landing.");
    }
}
