namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The walk credit after one shot reaction and whether it started recoil.</summary>
/// <param name="WalkCredit">The credit remaining after the hit's cost.</param>
/// <param name="HyperBeamRecoil">Whether a final-phase Hyper Beam hit overdrew the credit and starts the recoil.</param>
[IndependentFlag("whether this shot reaction starts the hyper-beam recoil")]
internal readonly record struct MotherBrainShotReactionResult(MotherBrainWalkCredit WalkCredit, bool HyperBeamRecoil);
