namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>A reserve tank visual and its installed name.</summary>
/// <param name="Name">The name.</param>
/// <param name="Id">The id.</param>
public readonly record struct PauseReserveTankFrame(string Name, PauseReserveTankVisual Id);
