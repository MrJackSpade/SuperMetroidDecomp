using SuperMetroid.Core.Game;

/// <summary>Independent conversion, retention and release assertions for the native controller trace.</summary>
internal static class TemporaryBlueSequenceAssertions
{
    public static void VerifySample(SamusState samus, int frame, int mode, int runway, bool left, bool jumpAfter)
    {
        ushort[] boost = [0x0201, 0x0301, 0x0401, 0x0401];
        uint[] extra = [0x3f000, 0x4f000, 0x5f000, 0x6f000];
        uint extraFixed = ((uint)samus.HorizontalSpeed.ExtraRunSpeed << 16) | samus.HorizontalSpeed.ExtraRunSubspeed;
        if (frame == 179 && (samus.HorizontalSpeed.SpeedBoostCounter != boost[runway] ||
            extraFixed != extra[runway] || samus.Pose != (left ? SamusPoseIds.MorphBallMovingLeftPose : SamusPoseIds.MorphBallMovingRightPose)))
            throw new InvalidDataException("Temporary Blue Suit fixture failed to acquire its preceding Speedball.");
        if (frame < 187) return;
        // The run-again control restarts the ordinary booster after frame 211;
        // do not incorrectly demand that its fresh counter stay zero forever.
        if (mode != 7 || jumpAfter || frame <= 211)
        {
            bool canceled = mode == 0 || mode is >= 4 and <= 6 && frame >= 195 ||
                mode == 7 && !jumpAfter && frame == 211;
            if (samus.HorizontalSpeed.SpeedBoostCounter != (canceled ? 0 : boost[runway]))
                throw new InvalidDataException("Temporary Blue Suit retained/canceled at the wrong input boundary.");
        }
        if (frame <= 209 || !jumpAfter && mode != 7)
        {
            if (samus.HorizontalSpeed.BaseFixed != 0 || extraFixed != 0 ||
                samus.HorizontalSpeed.AccelerationMode != 0 || samus.Kinematics.YFixed != 0x01f0ffff)
                throw new InvalidDataException("Unmorphed stationary boost must retain its stage, not numeric horizontal velocity.");
        }
        if (frame == 187)
        {
            int[] rightPose = [0x27, 0x71, 0x73, 0x85, 0x71, 0x73, 0x85, 0x71];
            if (samus.Pose != rightPose[mode] + (left ? 1 : 0))
                throw new InvalidDataException("Unmorph did not choose the native held-angle crouching pose.");
        }
        if (jumpAfter && frame == 240 && samus.Kinematics.YFixed >= 0x01f00000)
            throw new InvalidDataException("Temporary Blue Suit control did not actually leave the floor.");
    }
}
