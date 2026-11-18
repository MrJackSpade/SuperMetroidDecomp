using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// A Torizo standing facing one way with one foot forward (#2411): its spewing poses, from its lowered head to its arm drawn
/// back, and its two arms' poses as it throws or swipes. Each attack is written once over a stance.
/// </summary>
/// <param name="HeadLowered">Its head lowered, before it spews, throws or swipes.</param>
/// <param name="HeadRaised">Its head raised as it spews.</param>
/// <param name="MouthOpening">Its mouth opening as it spews.</param>
/// <param name="ArmThrust">Its arm thrust out as it spews.</param>
/// <param name="ArmRaised">Its arm raised as it spews.</param>
/// <param name="ArmDrawnBack">Its arm drawn back, held while its orbs fly.</param>
/// <param name="NearArm">The arm on the viewer's side.</param>
/// <param name="FarArm">The arm away from the viewer.</param>
internal sealed record TorizoStance(
    EnemyFrameId HeadLowered, EnemyFrameId HeadRaised, EnemyFrameId MouthOpening, EnemyFrameId ArmThrust, EnemyFrameId ArmRaised,
    EnemyFrameId ArmDrawnBack, TorizoArmPoses NearArm, TorizoArmPoses FarArm);
