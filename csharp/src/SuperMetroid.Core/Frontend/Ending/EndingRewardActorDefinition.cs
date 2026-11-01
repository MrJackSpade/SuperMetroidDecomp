using SuperMetroid.Core.Frontend.Intro;
namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>One six-byte ending reward cinematic-object definition.</summary>
internal readonly record struct EndingRewardActorDefinition(
    EndingRewardInitialization Initialization,
    CinematicSpriteFrameBehavior FrameBehavior,
    CinematicSpriteProgram Program);
