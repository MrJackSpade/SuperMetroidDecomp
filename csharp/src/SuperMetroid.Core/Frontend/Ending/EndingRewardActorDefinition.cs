namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>One ending reward actor's definition: where it starts, in which colouring, and its animation.</summary>
internal readonly partial record struct EndingRewardActorDefinition(
    EndingRewardStart Start,
    EndingRewardAnimation Animation);
