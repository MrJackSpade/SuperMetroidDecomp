namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// One stage of Mother Brain's drained stagger (#2315): one walk forward or back, then a pause. The stage her rainbow beam
/// runs out at is told by its place in the stagger, since the one after it walks alike.
/// </summary>
internal sealed partial class MotherBrainStaggerStage
{
    /// <summary>Defines a stage.</summary>
    /// <param name="speed">How fast she staggers.</param>
    /// <param name="neckTurn">How far her neck turns each update.</param>
    /// <param name="pauseUpdates">The updates she pauses once the walk reaches its end.</param>
    /// <param name="direction">Which way the stage walks her.</param>
    /// <param name="beamRunsOut">Whether her drained rainbow beam runs out as the stage begins.</param>
    internal MotherBrainStaggerStage(MotherBrainWalkSpeed speed, RotationAngle neckTurn, int pauseUpdates, MotherBrainStaggerDirection direction,
        bool beamRunsOut = false)
    {
        Speed = speed;
        NeckTurn = neckTurn;
        PauseUpdates = pauseUpdates;
        Direction = direction;
        BeamRunsOut = beamRunsOut;
    }

    /// <summary>Which way the stage walks her.</summary>
    internal MotherBrainStaggerDirection Direction { get; }

    /// <summary>Whether her drained rainbow beam runs out as the stage begins.</summary>
    internal bool BeamRunsOut { get; }
    /// <summary>How fast she staggers.</summary>
    internal MotherBrainWalkSpeed Speed { get; }

    /// <summary>How far her neck turns each update.</summary>
    internal RotationAngle NeckTurn { get; }

    /// <summary>The updates she pauses once the walk reaches its end.</summary>
    internal int PauseUpdates { get; }
}
