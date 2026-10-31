namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The game's two frame counters, which advance once for every frame the game shows. They are separate counters, not
/// one counter and its low part: a door's scroll restarts only the short one (<see cref="RestartShortCounter"/>). Enemy
/// cadences, animated tiles, blinking and other periodic gameplay sample their low bits, so each counter's wrap at its
/// width, the frames a door's music load counts without a gameplay update, and the door scroll's restart of the short
/// counter are all gameplay-observable.
/// </summary>
/// <remarks>
/// Created once per game session by its composition root (<c>FrontendSession</c>) and handed to every gameplay runtime
/// the session creates, so the counters run across menus and runtimes; only power-on and a soft reset clear them. Advanced at each
/// update's frame boundary, when the controller is latched (or by the frontend while no runtime exists); advanced again
/// by the frames a door's music load counts (<see cref="IDoorMusicLoadFrames"/>) when the door starts the destination's
/// music; restarted by <see cref="DoorTransitionFrameCounting"/> on the door scroll's updates. Evidence: the original
/// counts both counters on every shown frame, including the frames its music upload stalls the main loop for; the SMV
/// replays' input-consumption traces establish those stalled frames, and the Ridley full-movie check reads the short
/// counter on every update of a door transition.
/// </remarks>
[CompatibilityService("the two frame counters, which wrap at their widths, count the frames a door's music load takes and are restarted by a door's scroll")]
internal sealed class FrameClock
{
    /// <summary>How many frames <see cref="ShortCounter"/> counts before it returns to zero.</summary>
    internal const int ShortCounterPeriod = 256;

    /// <summary>How many frames <see cref="Counter"/> counts before it returns to zero.</summary>
    internal const int CounterPeriod = 65536;

    /// <summary>
    /// The short frame counter, 0 through 255. It returns to zero after 255, and gameplay that samples its low bits sees
    /// that wrap.
    /// </summary>
    public int ShortCounter { get; private set; }

    /// <summary>
    /// The long frame counter, 0 through 65535. It returns to zero after 65535, and gameplay that samples its low bits
    /// sees that wrap.
    /// </summary>
    public int Counter { get; private set; }

    /// <summary>Advances both counters for one shown frame, wrapping at their periods.</summary>
    internal void AdvanceAcceptedFrame() => Advance(1);

    /// <summary>
    /// Counts the frames that pass while a door's new music loads, which advance both counters without a gameplay
    /// update; <paramref name="musicLoad"/> says how many there are.
    /// </summary>
    /// <param name="musicLoad">The compatibility service counting the frames a door's music load takes.</param>
    internal void CountDoorMusicLoad(IDoorMusicLoadFrames musicLoad) =>
        Advance(Ensure.NotNull(musicLoad).FramesCountedDuringMusicLoad());

    /// <summary>Advances both counters by <paramref name="frames"/>, wrapping each at its period.</summary>
    private void Advance(int frames)
    {
        Ensure.AtLeastZero(frames);
        ShortCounter = (ShortCounter + frames % ShortCounterPeriod) % ShortCounterPeriod;
        Counter = (Counter + frames % CounterPeriod) % CounterPeriod;
    }

    /// <summary>
    /// Restarts the short counter while the long counter keeps running, as a door's scroll does on each of its updates
    /// (<see cref="DoorTransitionFrameCounting"/>): the next frame observes zero there.
    /// </summary>
    internal void RestartShortCounter() => ShortCounter = 0;

    /// <summary>Clears both counters, as power-on and a soft reset do.</summary>
    internal void Reset() => Import(0, 0);

    /// <summary>
    /// Takes over the counters another owner kept, such as the frontend that counted before this runtime existed or an
    /// imported snapshot. The game has one pair of counters; creating a runtime never resets them.
    /// </summary>
    internal void Import(int shortCounter, int counter)
    {
        ShortCounter = Ensure.BetweenInclusive(0, ShortCounterPeriod - 1, shortCounter);
        Counter = Ensure.BetweenInclusive(0, CounterPeriod - 1, counter);
    }
}
