namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>A moving shutter's speed, direction, trigger and rests (#1471, #1472, #1519).</summary>
/// <param name="Speed">Its speed in pixels per update; it moves equally fast either way.</param>
/// <param name="PrimaryDirection">The way it first heads; its reaction direction derives from it.</param>
/// <param name="Trigger">What starts it moving, with the wait or distance its trigger reads (#2560).</param>
/// <param name="TravelDistance">The pixels it travels between its ends.</param>
/// <param name="RestAfterMovingUp">How long it rests at its upper end, or that it stops there for good.</param>
/// <param name="RestAfterMovingDown">How long it rests at its lower end, or that it stops there for good.</param>
internal sealed record ShutterSpawnSettings(
    SignedSixteenSixteen Speed, VerticalDirection PrimaryDirection, ShutterTrigger Trigger, int TravelDistance,
    ShutterRest RestAfterMovingUp, ShutterRest RestAfterMovingDown) : EnemySpawnSettings
{
    /// <summary>Which kind of trigger starts it moving.</summary>
    internal ShutterTriggerMode TriggerMode => Trigger.Mode;
}
