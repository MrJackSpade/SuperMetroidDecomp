using SuperMetroid.Core.Bosses.Phantoon.Artwork;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>One Phantoon part frame: what it shows and its pose.</summary>
/// <param name="Kind">What the frame shows.</param>
/// <param name="Pose">The eyelid, gaze, tentacle or mouth pose; zero for the body and the single eye poses.</param>
internal readonly record struct PhantoonPartFrame(PhantoonPartFrameKind Kind, int Pose)
{
}