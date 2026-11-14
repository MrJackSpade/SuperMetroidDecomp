namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>A moving shutter's speed, direction, trigger and rests (#1471, #1472, #1519).</summary>
/// <param name="Speed">Its speed in pixels per update; it moves equally fast either way.</param>
/// <param name="PrimaryDirection">The end it first heads for; its reaction direction derives from it.</param>
/// <param name="TriggerMode">What starts it moving.</param>
/// <param name="TravelDistance">The pixels it travels between its ends.</param>
/// <param name="ProximityDistance">
/// The horizontal distance in pixels Samus must come within, for a <see cref="ShutterTriggerMode.HorizontalProximity"/>
/// shutter; zero for every other trigger.
/// </param>
/// <param name="WaitTime">The updates a <see cref="ShutterTriggerMode.Timer"/> shutter waits; zero for every other trigger.</param>
/// <param name="RestAfterMovingUp">How long it rests at its upper end, or that it stops there for good.</param>
/// <param name="RestAfterMovingDown">How long it rests at its lower end, or that it stops there for good.</param>
internal sealed record ShutterSpawnSettings(
    SignedSixteenSixteen Speed, ShutterHeading PrimaryDirection, ShutterTriggerMode TriggerMode, int TravelDistance,
    int ProximityDistance, int WaitTime, ShutterRest RestAfterMovingUp, ShutterRest RestAfterMovingDown) : EnemySpawnSettings;
