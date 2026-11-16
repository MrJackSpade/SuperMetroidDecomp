namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>What the Ceres cinematics' animations have their owner do: the PLANET ZEBES title's cues (#2197).</summary>
internal enum CeresCue
{
    /// <summary>Fades the PLANET ZEBES title in.</summary>
    FadeInZebesTitle,
    /// <summary>Spawns the Japanese title when Japanese text is selected.</summary>
    SpawnJapaneseTitleIfNeeded,
    /// <summary>Fades the PLANET ZEBES title out.</summary>
    FadeOutZebesTitle,
    /// <summary>Starts the flight to Zebes.</summary>
    StartFlyingToZebes,
}
