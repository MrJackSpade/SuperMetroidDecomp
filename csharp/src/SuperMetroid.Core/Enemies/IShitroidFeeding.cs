namespace SuperMetroid.Core.Enemies;

/// <summary>The Shitroid's feeding on its Tourian victim, as the victim asks about it when it is placed.</summary>
internal interface IShitroidFeeding
{
    /// <summary>Whether the room's Shitroid has already fed, so its victim is gone.</summary>
    bool HasAlreadyFed { get; }
}
