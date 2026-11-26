using Engine.Shared;
using Engine.Tests.Core;

namespace Engine.Tests.Shared;

/// <summary>Checks of the countdown, the wrapping counter, the clock, the random generator and facing.</summary>
public sealed class CountersChecks
{
    [Check]
    public void ACountdownEndsAfterItsUpdatesAndStaysEnded()
    {
        Countdown countdown = Countdown.Of(2);
        Expect.True(countdown.IsRunning);
        countdown = countdown.Stepped();
        Expect.True(countdown.IsRunning);
        countdown = countdown.Stepped();
        Expect.True(countdown.IsOver);
        Expect.Equal(Countdown.Over, countdown.Stepped());
    }

    [Check]
    public void TheSameSeedGivesTheSameDraws()
    {
        var first = new GameRandom(new PlainRandomSequence(12345));
        var second = new GameRandom(new PlainRandomSequence(12345));
        for (int draw = 0; draw < 50; draw++)
            Expect.Equal(first.Next(), second.Next());
    }

    [Check]
    public void ADrawBelowACountStaysBelowIt()
    {
        var random = new GameRandom(new PlainRandomSequence(99));
        for (int draw = 0; draw < 200; draw++)
        {
            int value = random.NextBelow(7);
            Expect.True(value >= 0 && value < 7);
        }
    }

    [Check]
    public void FacingTurnsDistancesItsOwnWay()
    {
        Pixels step = Pixels.FromWhole(3);
        Expect.Equal(step, Facing.Right.Toward(step));
        Expect.Equal(-step, Facing.Left.Toward(step));
        Expect.Equal(Facing.Left, Facing.Right.Opposite());
    }
}
