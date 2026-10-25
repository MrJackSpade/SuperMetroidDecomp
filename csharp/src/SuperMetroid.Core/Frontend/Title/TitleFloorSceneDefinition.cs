namespace SuperMetroid.Core.Frontend.Title;

/// <summary>The floor transform one title scene starts with.</summary>
/// <param name="Phase">Pan or zoom phase the scene runs in.</param>
/// <param name="Scale">Starting zoom in 1/256 units; 256 is unscaled.</param>
/// <param name="Position">Starting background offset in signed pixels, before the pan moves it.</param>
public readonly record struct TitleFloorSceneDefinition(
    TitleSequencePhase Phase,
    int Scale,
    Offset16<FloorPixels> Position);
