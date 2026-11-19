namespace SuperMetroid.Core.Bosses;

/// <summary>
/// A countdown of a boss fight that holds once it has run out (#2538): it runs out on the step of its last update, and
/// from then on a step leaves it run out and reports nothing, until it is started again; a countdown never started holds
/// the same way. The hold is what the fight does with these waits, unlike a <see cref="BossCountdown"/>, which runs out
/// again on every later step:
/// <list type="bullet">
/// <item>Kraid's thinking: grown Kraid's roar returns him to the wait it ran out for, so from then on only a hit opens his
/// mouth, and a hurt roar in his first form returns him to the wait as it stood.</item>
/// <item>Kraid's head's hold on the pose it shows: once an animation has ended, the head keeps the pose it ended on and
/// plays nothing further until an animation is started.</item>
/// <item>Fake Kraid's walk steps between spits: once they have run out, his next action choice spits and starts them
/// again.</item>
/// </list>
/// </summary>
public sealed class HeldBossCountdown
{
    /// <summary>The updates the countdown has left, the one it runs out on included: none once it has run out.</summary>
    public int UpdatesLeft { get; private set; }

    /// <summary>Whether the countdown has run out, or was never started.</summary>
    internal bool HasRunOut => UpdatesLeft == 0;

    /// <summary>Starts the countdown so that it runs out on the step of its <paramref name="updates"/>th update.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The countdown would last no update.</exception>
    internal void LastFor(int updates) => UpdatesLeft = Ensure.BetweenInclusive(1, int.MaxValue, updates);

    /// <summary>Steps a running countdown one update; one that has run out holds.</summary>
    internal void Step()
    {
        if (!HasRunOut)
            UpdatesLeft--;
    }

    /// <summary>
    /// Steps the countdown one update: whether it ran out on this step. One that has already run out holds, and this
    /// step is not the one it ran out on.
    /// </summary>
    internal bool StepRunsOut()
    {
        if (HasRunOut)
            return false;
        UpdatesLeft--;
        return HasRunOut;
    }
}
