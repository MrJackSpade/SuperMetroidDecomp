namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>One picture of the pause screen's reserve-tank strip and the name its installed frame carries.</summary>
/// <param name="Name">The name of the picture's installed frame.</param>
/// <param name="Id">The picture.</param>
public readonly record struct PauseReserveTankFrame(string Name, PauseReserveTankVisual Id);
