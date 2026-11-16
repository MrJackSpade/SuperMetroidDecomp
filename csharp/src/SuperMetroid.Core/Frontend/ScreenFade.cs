using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// A screen's brightness and the cadence a fade steps it at (#2081): every frontend scene, and the gameplay session's
/// pause, death, escape and arrival fades, fade with this one type.
/// </summary>
/// <remarks>
/// The brightness is a level from black, zero, through <see cref="DisplayBrightness.Full"/>. Each fade update first counts
/// down the wait: while updates remain to wait it only counts; otherwise it reloads the wait from the step delay and moves
/// the level by its step, stopping at black and at full. A step delay and first wait of <c>n</c> therefore step on the
/// update <c>n + 1</c> updates from the start and every <c>n + 1</c> updates after it (<see cref="StepEvery"/>).
/// </remarks>
public sealed class ScreenFade
{
    /// <summary>A fade at <paramref name="level"/> that steps on every update.</summary>
    /// <param name="level">The brightness, from black through <see cref="DisplayBrightness.Full"/>.</param>
    public ScreenFade(int level) => Show(level);

    /// <summary>A fade at <paramref name="level"/> with the given cadence (<see cref="SetCadence"/>).</summary>
    /// <param name="level">The brightness, from black through <see cref="DisplayBrightness.Full"/>.</param>
    /// <param name="stepDelay">The updates waited between one step and the next.</param>
    /// <param name="firstWait">The updates waited before the first step.</param>
    public ScreenFade(int level, int stepDelay, int firstWait)
    {
        Show(level);
        SetCadence(stepDelay, firstWait);
    }

    /// <summary>The screen's brightness, from black, zero, through <see cref="DisplayBrightness.Full"/>.</summary>
    [IntegerQuantity("screen brightness level, black through full")]
    public int Level { get; private set; }

    /// <summary>The updates waited between one step and the next.</summary>
    [IntegerQuantity("updates waited between fade steps")]
    private int stepDelay;

    /// <summary>The updates still to wait before the next step.</summary>
    [IntegerQuantity("updates still waited before the next fade step")]
    private int wait;

    /// <summary>Whether the screen is black.</summary>
    public bool IsBlack => Level == 0;

    /// <summary>Whether the screen is fully lit.</summary>
    public bool IsFullyLit => Level == DisplayBrightness.Full;

    /// <summary>Shows the screen at <paramref name="level"/>, leaving the cadence as it is.</summary>
    /// <param name="level">The brightness, from black through <see cref="DisplayBrightness.Full"/>.</param>
    public void Show(int level) => Level = DisplayBrightness.Require(level);

    /// <summary>Sets the cadence: the first step waits <paramref name="firstWait"/> updates, each later one <paramref name="stepDelay"/>.</summary>
    /// <param name="stepDelay">The updates waited between one step and the next, zero or more.</param>
    /// <param name="firstWait">The updates waited before the first step, zero or more.</param>
    public void SetCadence(int stepDelay, int firstWait)
    {
        this.stepDelay = Ensure.BetweenInclusive(0, int.MaxValue, stepDelay);
        wait = Ensure.BetweenInclusive(0, int.MaxValue, firstWait);
    }

    /// <summary>Steps once every <paramref name="updates"/> updates from now, the first step on the last of them.</summary>
    /// <param name="updates">The updates each step takes, one or more.</param>
    public void StepEvery(int updates)
    {
        int waited = Ensure.BetweenInclusive(1, int.MaxValue, updates) - 1;
        SetCadence(waited, waited);
    }

    /// <summary>Shows the screen at <paramref name="level"/> and waits <paramref name="stepDelay"/> updates before each step.</summary>
    /// <param name="level">The brightness, from black through <see cref="DisplayBrightness.Full"/>.</param>
    /// <param name="stepDelay">The updates waited before the first step and between each step and the next.</param>
    public void Restart(int level, int stepDelay)
    {
        Show(level);
        SetCadence(stepDelay, stepDelay);
    }

    /// <summary>One update of fading in by one level; returns whether the screen is fully lit after it.</summary>
    public bool Brighten() => Brighten(1);

    /// <summary>One update of fading in by <paramref name="levels"/> levels a step; returns whether the screen is fully lit after it.</summary>
    /// <param name="levels">The levels one step brightens by, one or more.</param>
    public bool Brighten(int levels)
    {
        Ensure.BetweenInclusive(1, DisplayBrightness.Full, levels);
        if (!Waits())
            Level = Math.Min(DisplayBrightness.Full, Level + levels);
        return IsFullyLit;
    }

    /// <summary>One update of fading out by one level; returns whether the screen is black after it.</summary>
    public bool Darken() => Darken(1);

    /// <summary>One update of fading out by <paramref name="levels"/> levels a step; returns whether the screen is black after it.</summary>
    /// <param name="levels">The levels one step darkens by, one or more.</param>
    public bool Darken(int levels)
    {
        Ensure.BetweenInclusive(1, DisplayBrightness.Full, levels);
        if (!Waits())
            Level = Math.Max(0, Level - levels);
        return IsBlack;
    }

    /// <summary>Counts one update of the wait: true while it only waits; otherwise reloads the wait for the next step.</summary>
    private bool Waits()
    {
        if (wait > 0)
        {
            wait--;
            return true;
        }
        wait = stepDelay;
        return false;
    }
}
