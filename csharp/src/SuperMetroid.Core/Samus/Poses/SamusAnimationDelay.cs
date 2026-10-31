namespace SuperMetroid.Core.Samus.Poses;

/// <summary>A frame shown for <paramref name="Ticks"/> animation ticks before the animation moves to the next frame.</summary>
/// <param name="Ticks">The authored delay, 0 through 127 ticks; the liquid buffer is added to it when the frame starts.</param>
internal sealed record SamusAnimationDelay(int Ticks) : SamusAnimationProgramStep;
