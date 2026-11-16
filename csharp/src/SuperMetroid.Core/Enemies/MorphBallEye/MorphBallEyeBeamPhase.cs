namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>The lifetime of the Morph Ball eye's beam.</summary>
public enum MorphBallEyeBeamPhase
{
    /// <summary>No beam exists; its width, growth and colour are at their reset values.</summary>
    Inactive,
    /// <summary>The eye AI requested a beam, which appears on the next beam update, before the next enemy AI pass.</summary>
    PendingInitialization,
    /// <summary>The beam widens ever faster until it spreads four steps either side of the eye's aim.</summary>
    Widening,
    /// <summary>The beam, spread four steps either side of the eye's aim, follows that aim and cycles sixteen yellow brightness steps while the eye stays activated.</summary>
    Full,
    /// <summary>The beam's colour fades toward black; once its green has faded out the beam is removed on the following update.</summary>
    Deactivating,
}
