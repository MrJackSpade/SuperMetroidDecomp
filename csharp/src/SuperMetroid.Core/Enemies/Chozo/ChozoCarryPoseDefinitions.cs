namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// The Chozo statues' carry, pose by pose (Wrecked Ship and Lower Norfair): the walking speed and where Samus is held.
/// </summary>
internal static class ChozoCarryPoseDefinitions
{
    /// <summary>
    /// Each carry pose's speed and hold, for a statue facing right. The four poses that take and hold Samus stand still,
    /// lifting her from 32 pixels up and 28 out as the hand opens to 23 up and 32 out once it grips. The stride walks 2,
    /// 3, 14 and 8 pixels an update through its four steps while Samus bobs one pixel up, two up, and back.
    /// </summary>
    internal static IReadOnlyDictionary<ChozoCarryPose, ChozoCarryPoseDefinition> Poses { get; } =
        new Dictionary<ChozoCarryPose, ChozoCarryPoseDefinition>
        {
            [ChozoCarryPose.Reaching] = new(SignedEightEight.Zero, new(28, -32)),
            [ChozoCarryPose.Closing] = new(SignedEightEight.Zero, new(30, -25)),
            [ChozoCarryPose.Gripping] = new(SignedEightEight.Zero, new(32, -23)),
            [ChozoCarryPose.Holding] = new(SignedEightEight.Zero, new(32, -23)),
            [ChozoCarryPose.StrideWeightShift] = new(SignedEightEight.FromPixels(2), new(32, -23)),
            [ChozoCarryPose.StrideLift] = new(SignedEightEight.FromPixels(3), new(32, -24)),
            [ChozoCarryPose.StrideSwing] = new(SignedEightEight.FromPixels(14), new(32, -25)),
            [ChozoCarryPose.StridePlant] = new(SignedEightEight.FromPixels(8), new(32, -24)),
        };
}
