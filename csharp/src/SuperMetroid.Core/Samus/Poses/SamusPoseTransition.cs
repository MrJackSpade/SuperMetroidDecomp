namespace SuperMetroid.Core.Samus.Poses;

/// <summary>The input transition that won: the pose it chooses and the setup that installs it.</summary>
/// <param name="ProspectivePose">The pose the transition chooses.</param>
/// <param name="Setup">The setup the pose update runs to install it.</param>
public readonly record struct SamusPoseTransition(
    SamusPoseId ProspectivePose,
    SamusPoseTransitionSetup Setup);
