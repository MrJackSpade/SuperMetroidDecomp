namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// The Chozo statues' carry, pose by pose (Wrecked Ship and Lower Norfair): the walking speed and where Samus is held.
/// </summary>
internal static partial class ChozoCarryPoseDefinitions
{
    /// <summary>
    /// Each carry pose's speed and hold, for a statue facing right. The poses that take and hold Samus stand still,
    /// moving her from where the open hand holds her to where its grip does. The stride walks through its steps at their
    /// own speeds while Samus bobs up and back.
    /// </summary>
    internal static IdentityTable<ChozoCarryPose, ChozoCarryPoseDefinition> Poses { get; } = new(new Dictionary<ChozoCarryPose, ChozoCarryPoseDefinition>
        {
            [ChozoCarryPose.Reaching] = new(SignedEightEight.Zero, new(28, -32)),
            [ChozoCarryPose.Closing] = new(SignedEightEight.Zero, new(30, -25)),
            [ChozoCarryPose.Gripping] = new(SignedEightEight.Zero, new(32, -23)),
            [ChozoCarryPose.Holding] = new(SignedEightEight.Zero, new(32, -23)),
            [ChozoCarryPose.StrideWeightShift] = new(SignedEightEight.FromPixels(2), new(32, -23)),
            [ChozoCarryPose.StrideLift] = new(SignedEightEight.FromPixels(3), new(32, -24)),
            [ChozoCarryPose.StrideSwing] = new(SignedEightEight.FromPixels(14), new(32, -25)),
            [ChozoCarryPose.StridePlant] = new(SignedEightEight.FromPixels(8), new(32, -24)),
        });
}
