namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>What the intro's Metroid egg waits for (#1903).</summary>
internal enum MetroidEggBehavior
{
    /// <summary>Samus's approach: once she is near enough the egg starts hatching.</summary>
    WaitsForSamus,

    /// <summary>Nothing: the egg hatches as its animation plays.</summary>
    Hatches,

    /// <summary>The end of the cross-fade from the discovery back to the narration, when the egg deletes itself.</summary>
    WaitsForCrossFadeEnd,
}
