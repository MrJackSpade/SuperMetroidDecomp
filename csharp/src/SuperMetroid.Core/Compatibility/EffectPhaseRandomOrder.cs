namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Where the room's per-line effect work falls in the sequence of random-number draws. Each update that runs it advances
/// the room liquid's shared wave and the Tourian statues' descent before the update draws its random number, and the
/// statues' rumble reads the random number as it stands then. Which updates run it is part of the behaviour: every
/// update that draws its random number runs the effect work first, except the door transition's updates from the update
/// that reads the door's header to the update that loads the destination room, during which the room's per-line effects
/// are stopped and the update draws its random number alone. The statues' rumble, and every decision later drawn from the
/// random sequence, depend on that order.
/// </summary>
/// <remarks>
/// Consulted at the start of every update that draws a random number without a message box (gameplay, a message box's
/// restoring update, the door transition's waits and fades, and the update before the pause screen opens), before the
/// draw. Stopped by the door transition after its header update's draw; resumed when the destination room has loaded.
/// Evidence: the original main loop runs its per-line effect objects (<c>$88:84B9</c>) and then draws its random number
/// (<c>$82:894F</c>) on every pass; the door header routine (<c>$82:E2F7</c>) switches the per-line effects off, and the
/// door's updates up to the destination's load draw their random number without them. The Ridley full-movie check
/// compares the random number on every update of a door transition.
/// </remarks>
[CompatibilityService("the room's per-line effects advance before each update's random draw, and stop from a door's header update until its destination loads")]
public sealed class EffectPhaseRandomOrder
{
    /// <summary>Whether an update that draws its random number runs the per-line effect work first.</summary>
    public bool RunsEffectWork { get; private set; } = true;

    /// <summary>Stops the per-line effects: the door transition has read the door's header.</summary>
    public void StopForDoorLoading() => RunsEffectWork = false;

    /// <summary>Resumes the per-line effects: the door's destination room has loaded its own.</summary>
    public void ResumeAfterRoomLoad() => RunsEffectWork = true;
}
