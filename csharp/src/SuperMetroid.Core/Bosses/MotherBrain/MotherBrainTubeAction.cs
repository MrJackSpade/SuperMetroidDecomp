namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>What a falling tube of Mother Brain's room does, in the order the main tube goes through them.</summary>
internal enum MotherBrainTubeAction
{
    /// <summary>A side tube, which falls from the start.</summary>
    NonMainTube,

    /// <summary>The main tube, waiting out its delay before it falls.</summary>
    WaitingToFall,

    /// <summary>The main tube, falling.</summary>
    Falling,
}
