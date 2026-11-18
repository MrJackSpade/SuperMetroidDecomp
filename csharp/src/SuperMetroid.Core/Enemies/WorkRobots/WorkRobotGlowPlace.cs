namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>
/// Where the brightest red of the powered Work Robots' glow stands on its run along the robots' red parts (#2380, #2393).
/// The glow crosses from the first end to the far end and back, so each place between the ends shows twice in a cycle, once
/// each way, in the same colours. Each place's colours are the installed shade the import pre-colours for it
/// (<see cref="Artwork.WorkRobotShades"/>); the order and holds of the cycle's steps are <see cref="WorkRobotGlowAnimations"/>.
/// </summary>
internal enum WorkRobotGlowPlace
{
    /// <summary>The brightest red at the run's first end, where the glow rests longest.</summary>
    AtFirstEnd,

    /// <summary>The brightest red just off the first end: leaving it on the way across, and nearing it on the way back.</summary>
    NearFirstEnd,

    /// <summary>The brightest red just short of the far end: nearing it on the way across, and leaving it on the way back.</summary>
    NearFarEnd,

    /// <summary>The brightest red at the run's far end, where the glow rests longest.</summary>
    AtFarEnd,
}
