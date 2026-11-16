using SuperMetroid.Core.Frontend.Intro;
namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>One ending reward actor's definition: where it starts, in which colouring, and its animation.</summary>
internal readonly record struct EndingRewardActorDefinition(
    EndingRewardStart Start,
    CinematicSpriteSegment Animation);
