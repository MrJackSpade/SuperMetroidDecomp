namespace SuperMetroid.Core.Samus.Movement;

/// <summary>What the Speed Booster echo cursor holds.</summary>
public enum SpeedEchoCursorState
{
    /// <summary>The next capture fills the first slot.</summary>
    FirstSlot,

    /// <summary>The next capture fills the second slot.</summary>
    SecondSlot,

    /// <summary>The echoes are departing toward Samus.</summary>
    Departing,
}
