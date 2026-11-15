namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>How a Magdollite's head rises and how close Samus must come before it does (#2023).</summary>
/// <param name="Speed">The speed, in pixels per update, at which the head and its arm rise and sink.</param>
/// <param name="Range">The horizontal distance in pixels Samus must come within.</param>
internal sealed record MagdolliteHeadRise(SignedSixteenSixteen Speed, int Range);
