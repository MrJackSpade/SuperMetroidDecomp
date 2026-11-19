namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Whether and how the intro shows Samus. The cinematic step updates Samus only while she is shown, and
/// the cinematic draw layers Samus and the cinematic actors by how she is shown.
/// </summary>
internal enum IntroSamusDisplay
{
    /// <summary>Samus is neither processed nor drawn.</summary>
    Hidden,

    /// <summary>Samus is shown behind the cinematic actors.</summary>
    BehindActors,

    /// <summary>Samus and her projectiles are shown in front of the cinematic actors.</summary>
    InFrontOfActors,
}
