namespace SuperMetroid.Core.Frontend;

/// <summary>Which step of an attract demo (<see cref="Progression.SuperMetroidGameState.AttractDemo"/>) runs.</summary>
internal enum AttractDemoStep
{
    /// <summary>The demo's room and its recorded starting state load behind a black screen.</summary>
    Loading,
    /// <summary>The demo's first gameplay pass, after which the room is shown fully bright and playback begins.</summary>
    Starting,
    /// <summary>The recording plays until its timer or a newly pressed real button ends it, then its final image holds.</summary>
    Playing,
    /// <summary>The demo's gameplay is released while the schedule decides whether another demo plays.</summary>
    Unloading,
    /// <summary>The next demo loads, or the title returns: from its beginning, or at its title screen when a button ended the demo.</summary>
    Routing,
}
