namespace Engine.Core;

/// <summary>A sound gameplay asked for and was granted.</summary>
/// <param name="Sound">The sound, by name.</param>
/// <param name="Channel">The channel it sounds on.</param>
public readonly record struct AcceptedSound(string Sound, string Channel);
