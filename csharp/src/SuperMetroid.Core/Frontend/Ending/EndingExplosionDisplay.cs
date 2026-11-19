namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// How the ending shows Zebes's explosion (#1514, #2541): not yet, as its burst, or as its finale. The explosion's cues
/// move it on; the planet escape's phases no longer show it.
/// </summary>
internal enum EndingExplosionDisplay
{
    /// <summary>The explosion has not burst yet: the scene draws its actors in their own layers.</summary>
    NotStarted,
    /// <summary>The burst: the stars behind the planet, the planet, the debris and the burst's glow over them.</summary>
    Burst,
    /// <summary>The finale: the glow behind and over the planet, with the debris glowing.</summary>
    Finale,
}
