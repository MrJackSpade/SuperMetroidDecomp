namespace SuperMetroid.Core.Assets;

/// <summary>The attributes and first character that identify a cached small object frame.</summary>
/// <param name="Attributes">The attributes.</param>
/// <param name="FirstCharacter">The first character.</param>
public readonly record struct SmallObjectKey(int Attributes, int FirstCharacter);
