using SuperMetroid.Core.Frontend.Intro;
namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>One ending reward cinematic-object definition.</summary>
internal readonly record struct EndingRewardActorDefinition(
    EndingRewardInitialization Initialization,
    CinematicSpriteFrameBehavior FrameBehavior,
    CinematicSpriteSegment Animation);
