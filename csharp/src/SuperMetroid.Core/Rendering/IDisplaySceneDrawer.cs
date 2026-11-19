namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Draws each kind of displayed scene (#2505): a backend implements one method for each, and a scene calls the one for its
/// kind (<see cref="DisplayScene.DrawWith"/>), so every kind is drawn and none is left to a fallback.
/// </summary>
public interface IDisplaySceneDrawer
{
    /// <summary>Draws a layered scene.</summary>
    /// <param name="scene">The scene.</param>
    void DrawLayered(LayeredScene scene);

    /// <summary>Draws a turning floor with the sprites in front of it.</summary>
    /// <param name="scene">The scene.</param>
    void DrawTurningFloor(FloorScene scene);

    /// <summary>Fills the screen with one colour.</summary>
    /// <param name="scene">The scene.</param>
    void DrawSolidColour(SolidColourScene scene);
}
