namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Where an ending reward actor starts and the palette bits it draws with.</summary>
/// <param name="Position">The starting screen position.</param>
/// <param name="Palette">The sprite palette row.</param>
public readonly record struct EndingRewardActorInitializationDefinition(Position16<ScreenPixels> Position, int Palette);
