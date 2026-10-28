namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>The lifetime of the Morph Ball eye's beam.</summary>
public enum MorphBallEyeBeamPhase
{
    /// <summary>No beam exists; its width, growth and colour are at their reset values.</summary>
    Inactive,
    /// <summary>The eye AI requested a beam, which appears on the next beam update, before the next enemy AI pass.</summary>
    PendingInitialization,
    /// <summary>The beam accelerates its 16.16 angular half-width until it reaches four of the 256 angle units per full turn.</summary>
    Widening,
    /// <summary>The four-unit half-width beam follows the eye's aim and cycles sixteen yellow brightness steps while the eye stays activated.</summary>
    Full,
    /// <summary>The beam's colour fades toward black; once its green has faded out the beam is removed on the following update.</summary>
    Deactivating,
}
