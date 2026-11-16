using SuperMetroid.Core.Frontend.Intro;
namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>One ending reward actor's definition.</summary>
internal readonly record struct EndingRewardActorDefinition(
    EndingRewardInitialization Initialization,
    EndingRewardAnimation Animation);
