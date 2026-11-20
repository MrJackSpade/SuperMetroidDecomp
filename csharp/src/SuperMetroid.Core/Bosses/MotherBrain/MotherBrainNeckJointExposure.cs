namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Whether Samus can touch a joint of Mother Brain's neck (#2556).</summary>
internal enum MotherBrainNeckJointExposure
{
    /// <summary>Samus can touch the joint.</summary>
    Exposed,

    /// <summary>The joint lies inside her body, so Samus cannot touch it.</summary>
    InsideBody,

    /// <summary>Her brain's own hit area covers the joint, so Samus touches the brain instead.</summary>
    UnderBrain,
}
