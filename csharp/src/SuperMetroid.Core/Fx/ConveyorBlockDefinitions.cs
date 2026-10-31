namespace SuperMetroid.Core.Fx;

/// <summary>How far a conveyor block carries Samus each frame she stands in it.</summary>
internal static class ConveyorBlockDefinitions
{
    /// <summary>The whole pixels a rightward conveyor carries Samus each frame.</summary>
    public const int RightwardCarry = 2;

    /// <summary>The whole pixels a leftward conveyor carries Samus each frame.</summary>
    public const int LeftwardCarry = -2;
}
