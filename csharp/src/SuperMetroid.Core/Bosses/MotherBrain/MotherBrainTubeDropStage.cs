namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Where the tube the collapse about Mother Brain's tank is on stands in its drop (#2258).</summary>
internal enum MotherBrainTubeDropStage
{
    /// <summary>The tube still stands; it falls next.</summary>
    Standing,
    /// <summary>The tube has fallen; the room is cleared where it stood next.</summary>
    Fallen,
}
