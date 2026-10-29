namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Where an ending reward actor starts and the screen colours it draws in.</summary>
/// <param name="Position">The starting screen position.</param>
/// <param name="Colouring">The screen colours the actor draws in.</param>
internal readonly record struct EndingRewardActorInitializationDefinition(Position16<ScreenPixels> Position, ScreenColourGroup Colouring);
