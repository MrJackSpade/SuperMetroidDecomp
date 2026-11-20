namespace SuperMetroid.Core.Samus.Poses;

/// <summary>A frame shown for <paramref name="Ticks"/> animation ticks before the animation moves to the next frame.</summary>
/// <param name="Ticks">The authored delay, in ticks; Samus's extra animation delay is added to it when the frame starts.</param>
internal sealed record SamusAnimationDelay(int Ticks) : SamusPoseAnimationStep
{
    /// <summary>The most updates one step of Samus's animations is shown for.</summary>
    internal const int LongestTicks = 127;
}
