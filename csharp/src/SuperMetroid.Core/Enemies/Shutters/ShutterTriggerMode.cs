namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>What starts a shutter moving, from its population's first parameter.</summary>
public enum ShutterTriggerMode
{
    /// <summary>Moves once its initial wait expires.</summary>
    Timer,

    /// <summary>Moves once Samus comes within the horizontal distance, and waits for her again each time it returns to its primary end.</summary>
    HorizontalProximity,

    /// <summary>Moves at once.</summary>
    Immediate,

    /// <summary>Stays put until the first shot or Power Bomb, which moves it once.</summary>
    OneShotReaction,

    /// <summary>Stays put until a shot or Power Bomb, and reacts again after every stop.</summary>
    RepeatableReaction,
}
