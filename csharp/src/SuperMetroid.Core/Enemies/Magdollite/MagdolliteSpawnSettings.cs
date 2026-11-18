namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// Which part of a Magdollite a placement is (#1472, #1519, #2023): its head, its rising body or its tracking overlay,
/// each its own settings (#2474), so only the head's carry how it rises.
/// </summary>
internal abstract record MagdolliteSpawnSettings : EnemySpawnSettings;

/// <summary>A Magdollite head's placement: how it and its arm rise and how close Samus must come before they do.</summary>
/// <param name="Speed">The speed, in pixels per update, at which the head's arm rises and sinks.</param>
/// <param name="Range">The horizontal distance in pixels Samus must come within.</param>
internal sealed record MagdolliteHeadSpawnSettings(SignedSixteenSixteen Speed, int Range) : MagdolliteSpawnSettings;

/// <summary>A Magdollite's rising body: the arm that grows and shrinks the lava pillar under its head.</summary>
internal sealed record MagdolliteBodySpawnSettings : MagdolliteSpawnSettings;

/// <summary>A Magdollite's tracking overlay: the hand that follows the pillar's top and throws the lava.</summary>
internal sealed record MagdolliteOverlaySpawnSettings : MagdolliteSpawnSettings;
