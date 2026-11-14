namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Whether and how the intro shows Samus. The cinematic step updates Samus only while she is shown, and
/// the cinematic draw layers Samus and the cinematic objects by how she is shown.
/// </summary>
internal enum IntroSamusDisplay
{
    /// <summary>Samus is neither processed nor drawn.</summary>
    Hidden,

    /// <summary>Samus is shown under the cinematic objects, which draw first and so over her.</summary>
    ObjectsFirst,

    /// <summary>Samus is shown over the cinematic objects: she and her projectiles draw first.</summary>
    SamusFirst,
}
