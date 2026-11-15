namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>How the exploding planet's stars move with the gunship's flyaway (#1903).</summary>
internal enum EndingStarsMotion
{
    /// <summary>Waiting for the flyaway to start.</summary>
    WaitingForFlyaway,

    /// <summary>Moving across the screen, slowing each update.</summary>
    Moving,
}
