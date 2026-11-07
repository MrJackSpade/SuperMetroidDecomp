namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The eye door's PLM programs (#1296): the eye's, and the passive body and bottom components' shared ones.</summary>
internal enum EyeDoorObjectProgram
{
    /// <summary>An eye whose door was opened before becomes the blue cap at once.</summary>
    EyeStart,
    /// <summary>The closed eye waits for Samus to come near.</summary>
    EyeIdle,
    /// <summary>The eye opens and arms its missile wake.</summary>
    EyeActivate,
    /// <summary>The open eye fires three shots, then blinks.</summary>
    EyeWatch,
    /// <summary>Samus in front of the eye makes it close for a moment.</summary>
    EyeClose,
    /// <summary>A missile hit: smoke, a counted hit, a flinch and sweat.</summary>
    EyeHit,
    /// <summary>The third hit: smoke and the blue cap's collision.</summary>
    EyeDestroyed,
    /// <summary>The eye flashes into the blue cap, then hands off to it.</summary>
    EyeOpening,
    /// <summary>An opened door's eye becomes the closed blue cap.</summary>
    EyeOpened,
    /// <summary>A component of an opened door deletes itself.</summary>
    ComponentStart,
    /// <summary>The still component waits for Samus to come near.</summary>
    ComponentIdle,
    /// <summary>The component arms its door-bit wake.</summary>
    ComponentActivate,
    /// <summary>The component pulses while Samus stays near.</summary>
    ComponentCycle,
    /// <summary>The opened door's component deletes itself.</summary>
    ComponentDelete,
}
