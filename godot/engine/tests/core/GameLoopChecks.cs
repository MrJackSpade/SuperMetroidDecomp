using Engine.Core;
using Engine.Input;
using Engine.Tests.Input;

namespace Engine.Tests.Core;

/// <summary>Checks that the loop plays whole updates in order, hands control between modes and keeps the session's state.</summary>
public sealed class GameLoopChecks
{
    [Check]
    public void EachStepRunsTheModeOnceAndMovesTheClockOn()
    {
        var loop = new GameLoop();
        loop.StartStandIn(new StandInDefinition { UpdatesBeforeHandOver = 0 }, new ScriptedInputSource());
        var mode = (StandInMode)loop.Session.Mode;

        loop.Step();
        loop.Step();
        loop.Step();

        Expect.Equal(1, mode.TimesEntered);
        Expect.Equal(3, mode.UpdatesRun);
        Expect.True(mode.ClockAtEachUpdate.SequenceEqual([0L, 1L, 2L]));
        Expect.Equal(3L, loop.Session.Clock.Updates);
        loop.Free();
    }

    [Check]
    public void AModeHandsOverToTheModeItNames()
    {
        var loop = new GameLoop();
        loop.StartStandIn(new StandInDefinition { UpdatesBeforeHandOver = 2 }, new ScriptedInputSource());
        GameMode first = loop.Session.Mode;

        loop.Step();
        Expect.True(ReferenceEquals(first, loop.Session.Mode));
        loop.Step();

        var second = (StandInMode)loop.Session.Mode;
        Expect.False(ReferenceEquals(first, second));
        Expect.Equal(1, second.TimesEntered);
        Expect.Equal(0, second.UpdatesRun);
        Expect.Equal(1, loop.GetChildren().OfType<GameMode>().Count());
        Expect.True(ReferenceEquals(second, loop.GetChild(0)));

        loop.Step();
        Expect.Equal(1, second.UpdatesRun);
        loop.Free();
    }

    [Check]
    public void TheModeSeesTheUpdatesInput()
    {
        var loop = new GameLoop();
        loop.StartStandIn(
            new StandInDefinition { UpdatesBeforeHandOver = 0 },
            new ScriptedInputSource([GameAction.Jump], [GameAction.Jump], [], [GameAction.Jump]));
        var mode = (StandInMode)loop.Session.Mode;

        for (int update = 0; update < 4; update++)
            loop.Step();

        Expect.Equal(2, mode.JumpPresses);
        loop.Free();
    }

    [Check]
    public void ASessionKeepsItsOwnProgressAndRandomSequence()
    {
        var first = new GameLoop();
        var second = new GameLoop();
        first.StartStandIn(new StandInDefinition { RandomSeed = 7 }, new ScriptedInputSource());
        second.StartStandIn(new StandInDefinition { RandomSeed = 7 }, new ScriptedInputSource());

        first.Session.Progress.Record("example fact");
        Expect.True(first.Session.Progress.Has("example fact"));
        Expect.False(second.Session.Progress.Has("example fact"));
        Expect.Equal(first.Session.Random.Next(), second.Session.Random.Next());
        first.Free();
        second.Free();
    }

    [Check]
    public void ALoopThatWasNotStartedRefusesToStep()
    {
        var loop = new GameLoop();
        Expect.Throws<InvalidOperationException>(loop.Step);
        loop.Free();
    }
}
