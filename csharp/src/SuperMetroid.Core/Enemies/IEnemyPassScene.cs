using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A scene an encounter runs beside the enemy pass and shows among the actors (#2571, #2573): a step as the enemy pass ends,
/// a step after the actors' drawing steps, and the pictures it shows once at their own depth. The enemy phase and the actor
/// drawing run every such scene the room's species compose, in their order; showing it changes no simulation state.
/// </summary>
internal interface IEnemyPassScene
{
    /// <summary>Runs the part of the scene's animation that runs as the enemy pass ends.</summary>
    void StepWithEnemyPass();

    /// <summary>Runs the part of the scene's animation that runs after the actors' drawing steps.</summary>
    void StepAfterActors();

    /// <summary>Shows the scene's pictures where they are, once, at their own depth among the actors.</summary>
    /// <param name="sprites">The frame's sprites.</param>
    /// <param name="camera">The camera the frame draws from.</param>
    void Publish(SpriteFrameBuffer sprites, Position16<RoomPixels> camera);
}
