using SuperMetroid.Core.Game;

/// <summary>
/// Native-observed landing windows for the controller-acquired boost fixture.
/// These assertions distinguish a retained fast roll from a later ordinary roll,
/// and do not infer block breaking or Blue Suit conversion from movement alone.
/// </summary>
internal static class SpeedballSequenceAssertions
{
    public static bool IsSoftMorph(int timing, int runway, bool fullJump)
    {
        int first = fullJump ? (runway == 3 ? 7 : 8) : new[] { 10, 12, 12, 14 }[runway];
        return timing >= first && timing <= first + (fullJump ? 6 : 7);
    }

    public static bool VerifySample(SamusState samus, int frame, int timing, int runway, bool left, bool fullJump)
    {
        int[] runFrames = [64, 80, 96, 112];
        int[] fullMorphOffsets = [96, 117, 117, 140];
        uint[] acquiredExtra = [0x3f000, 0x4f000, 0x5f000, 0x6f000];
        ushort[] acquiredBoost = [0x0201, 0x0301, 0x0401, 0x0401];
        int launch = runFrames[runway];
        int firstRoll = launch + (fullJump ? fullMorphOffsets[runway] : 20) + timing + 7;
        uint extra = ((uint)samus.HorizontalSpeed.ExtraRunSpeed << 16) | samus.HorizontalSpeed.ExtraRunSubspeed;
        bool soft = IsSoftMorph(timing, runway, fullJump);
        // All cases acquire momentum by running, without injecting a boost word.
        // Only soft landings promise to retain it through the landing transient.
        if ((frame == launch || soft && frame > launch && frame <= firstRoll) &&
            (extra != acquiredExtra[runway] || samus.HorizontalSpeed.SpeedBoostCounter != acquiredBoost[runway]))
            throw new InvalidDataException("Speedball lost controller-acquired momentum/stage before its soft landing.");
        if (soft && frame <= firstRoll && samus.MorphBallBounceState != 0)
            throw new InvalidDataException("Soft Speedball entered a hard-morph rebound.");
        ushort rollingPose = left ? SamusPoseIds.MorphBallMovingLeftPose : SamusPoseIds.MorphBallMovingRightPose;
        if (soft && frame == firstRoll &&
            (samus.Pose != rollingPose || samus.Kinematics.YFixed != 0x01f9ffff))
            throw new InvalidDataException("Speedball entered grounded rolling at the wrong frame or floor position.");
        return frame > launch && samus.Pose == rollingPose && extra != 0;
    }
}
