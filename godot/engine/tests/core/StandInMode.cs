using Engine.Core;
using Engine.Input;

namespace Engine.Tests.Core;

/// <summary>
/// A mode for the engine's own checks. It records what happened to it, and after a set number of updates hands over to
/// another stand-in mode that never hands over.
/// </summary>
public partial class StandInMode : GameMode
{
    /// <summary>How many updates this mode runs before handing over; zero or less means it never does.</summary>
    public int UpdatesBeforeHandOver { get; set; }

    /// <summary>How many times this mode has been entered.</summary>
    public int TimesEntered { get; private set; }

    /// <summary>How many updates this mode has run.</summary>
    public int UpdatesRun { get; private set; }

    /// <summary>What the session's clock read at each of this mode's updates.</summary>
    public List<long> ClockAtEachUpdate { get; } = [];

    /// <summary>How many of this mode's updates saw the jump action newly pressed.</summary>
    public int JumpPresses { get; private set; }

    /// <inheritdoc/>
    protected override void Enter() => TimesEntered++;

    /// <inheritdoc/>
    public override GameMode Step(GameInput input)
    {
        UpdatesRun++;
        ClockAtEachUpdate.Add(Session.Clock.Updates);
        if (input.WasPressed(GameAction.Jump))
            JumpPresses++;
        return UpdatesBeforeHandOver > 0 && UpdatesRun == UpdatesBeforeHandOver
            ? new StandInMode { Name = "Second" }
            : this;
    }
}
