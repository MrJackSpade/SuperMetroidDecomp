namespace SuperMetroid.Core.Frontend.Title;

/// <summary>The floor transform one title scene starts with.</summary>
/// <param name="Phase">Pan or zoom phase the scene runs in.</param>
/// <param name="Zoom">Starting zoom.</param>
/// <param name="Position">Starting background offset in signed pixels, before the pan moves it.</param>
public readonly record struct TitleFloorSceneDefinition(
    TitleSequencePhase Phase,
    FloorZoom Zoom,
    Offset16<FloorPixels> Position);
