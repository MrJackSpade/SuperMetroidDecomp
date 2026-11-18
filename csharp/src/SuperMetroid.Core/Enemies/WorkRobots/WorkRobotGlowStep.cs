namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>
/// One step of the powered Work Robots' glow (#2380): where the glow's brightest red stands on its run along the robots'
/// red parts, which it crosses from one end to the other and back, resting longest at either end. Each step's colours are
/// the installed shade the import pre-colours for it (<see cref="Artwork.WorkRobotShades"/>); the order and holds of the
/// steps are <see cref="WorkRobotGlowAnimations"/>.
/// </summary>
internal enum WorkRobotGlowStep
{
    /// <summary>The brightest red at the run's first end, where the glow rests longest.</summary>
    AtFirstEnd,

    /// <summary>The brightest red leaving the first end on its way across.</summary>
    LeavingFirstEnd,

    /// <summary>The brightest red nearing the far end on its way across.</summary>
    NearingFarEnd,

    /// <summary>The brightest red at the run's far end, where the glow rests longest.</summary>
    AtFarEnd,

    /// <summary>The brightest red leaving the far end on its way back.</summary>
    LeavingFarEnd,

    /// <summary>The brightest red nearing the first end on its way back.</summary>
    NearingFirstEnd,
}
