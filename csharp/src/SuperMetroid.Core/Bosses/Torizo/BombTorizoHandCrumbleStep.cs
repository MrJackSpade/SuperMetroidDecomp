namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>One step of the Bomb Torizo hand's crumble (<see cref="BombTorizoHandCrumbleDefinitions"/>).</summary>
/// <param name="Image">What the hand shows.</param>
/// <param name="Updates">How many updates the image stays before the next step.</param>
/// <param name="FragmentBefore">The statue fragment that drops just before the image shows; null for none.</param>
internal sealed record BombTorizoHandCrumbleStep(BombTorizoHandDraw Image, int Updates, int? FragmentBefore);
