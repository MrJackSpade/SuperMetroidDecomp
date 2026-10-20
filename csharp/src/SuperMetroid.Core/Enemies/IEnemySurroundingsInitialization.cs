namespace SuperMetroid.Core.Enemies;

/// <summary>
/// An enemy whose initialization AI reads the spawn's surroundings: the room, Samus, the controller or the camera. An
/// entity implements this or <see cref="IEnemyInitialization"/>, never both.
/// </summary>
internal interface IEnemySurroundingsInitialization : IEnemyEntity
{
    /// <summary>The initialization handler, or null when the header names none this build translates.</summary>
    EnemySurroundingsInitializationAiHandler? FindSurroundingsInitialization();
}
