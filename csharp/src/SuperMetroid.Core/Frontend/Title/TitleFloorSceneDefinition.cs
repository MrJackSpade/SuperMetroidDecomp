namespace SuperMetroid.Core.Frontend.Title;

/// <summary>The floor transform one title scene starts with.</summary>
/// <param name="Phase">Pan or zoom phase the scene runs in.</param>
/// <param name="startingZoom">Starting zoom, no smaller than the floor's own size; checked once as the catalog builds.</param>
/// <param name="Position">Starting background offset in signed pixels, before the pan moves it.</param>
public readonly record struct TitleFloorSceneDefinition(
    TitleSequencePhase Phase,
    FloorZoom startingZoom,
    Offset16<FloorPixels> Position)
{
    /// <summary>Starting zoom, no larger than the floor's own size.</summary>
    public TitleFloorZoom Zoom { get; } = TitleFloorZoom.Starting(startingZoom);
}
