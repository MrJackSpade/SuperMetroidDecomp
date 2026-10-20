namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Why Mother Brain's head stretches open: an attack, which drools, or the hyper-beam recoil, which does not.</summary>
internal enum MotherBrainStretchKind
{
    /// <summary>A phase-two or phase-three attack stretch, which spawns drool.</summary>
    Attack,

    /// <summary>The recoil from Samus's hyper beam.</summary>
    HyperBeamRecoil,
}
