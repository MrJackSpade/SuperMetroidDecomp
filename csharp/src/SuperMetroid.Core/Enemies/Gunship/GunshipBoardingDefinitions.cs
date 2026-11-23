namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>How Samus boards and leaves the landed gunship, and how it restores her while she is aboard (#2043).</summary>
internal static partial class GunshipBoardingDefinitions
{
    /// <summary>How far either side of the ship's middle Samus stands, in pixels, to board it by pressing down.</summary>
    internal const int EntranceHalfWidth = 8;

    /// <summary>How far above the ship's top Samus stands at most, in pixels and exclusive, to board it by pressing down.</summary>
    internal const int EntranceHeight = 64;

    /// <summary>How long the entrance pad takes to open or to close before the ship's next phase, in updates.</summary>
    internal const int PadMovementDuration = 144;

    /// <summary>How far Samus sinks into the ship each update as she boards it, in pixels.</summary>
    internal const int BoardingStep = 2;

    /// <summary>How far Samus rises out of the ship each update as she leaves it, in pixels.</summary>
    internal const int LeavingStep = 2;

    /// <summary>How far Samus rises out of the ship each update as she arrives on Zebes, in pixels.</summary>
    internal const int ArrivalRiseStep = 1;

    /// <summary>
    /// How far below the landed top hull's resting height Samus has sunk once she is aboard, in pixels: the pad
    /// closes once she reaches it.
    /// </summary>
    internal const int BoardedDepth = 18;

    /// <summary>
    /// How far above the landed top hull's resting height Samus stands once she is out, in pixels: the pad closes once
    /// she rises above it.
    /// </summary>
    internal const int ExitHeight = 30;

    /// <summary>How much energy, reserve energy and ammunition of each kind the ship restores each update while Samus is aboard.</summary>
    internal const int RefillStep = 2;
}
