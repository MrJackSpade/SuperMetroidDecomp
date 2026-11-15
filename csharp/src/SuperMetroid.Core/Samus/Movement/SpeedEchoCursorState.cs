namespace SuperMetroid.Core.Samus.Movement;

/// <summary>What the Speed Booster echo cursor holds.</summary>
public enum SpeedEchoCursorState
{
    /// <summary>The next capture fills the first echo.</summary>
    FirstEcho,

    /// <summary>The next capture fills the second echo.</summary>
    SecondEcho,

    /// <summary>The echoes are departing toward Samus.</summary>
    Departing,
}
