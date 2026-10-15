namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The eye door's PLM programs (#1296): the eye's, and the passive body and bottom components' shared ones.</summary>
internal enum EyeDoorPlmProgram
{
    /// <summary><c>InstList_PLM_EyeDoorEyeFacing*_0</c>: an eye whose door was opened before becomes the blue cap at once.</summary>
    EyeStart,
    /// <summary><c>InstList_PLM_EyeDoorEyeFacing*_1</c>: the closed eye waits for Samus to come near.</summary>
    EyeIdle,
    /// <summary><c>InstList_PLM_EyeDoorEyeFacing*_2</c>: the eye opens and arms its missile wake.</summary>
    EyeActivate,
    /// <summary><c>InstList_PLM_EyeDoorEyeFacing*_3</c>: the open eye fires three shots, then blinks.</summary>
    EyeWatch,
    /// <summary><c>InstList_PLM_EyeDoorEyeFacing*_4</c>: Samus in front of the eye makes it close for a moment.</summary>
    EyeClose,
    /// <summary><c>InstList_PLM_EyeDoorEyeFacing*_5</c>: a missile hit: smoke, a counted hit, a flinch and sweat.</summary>
    EyeHit,
    /// <summary><c>InstList_PLM_EyeDoorEyeFacing*_6</c>: the third hit: smoke and the blue cap's collision.</summary>
    EyeDestroyed,
    /// <summary><c>InstList_PLM_EyeDoorEyeFacing*_7</c>: the eye flashes into the blue cap, then hands off to it.</summary>
    EyeOpening,
    /// <summary><c>InstList_PLM_EyeDoorEyeFacing*_8</c>: an opened door's eye becomes the closed blue cap.</summary>
    EyeOpened,
    /// <summary><c>InstList_PLM_EyeDoor{,Bottom}Facing*_0</c>: a component of an opened door deletes itself.</summary>
    ComponentStart,
    /// <summary><c>InstList_PLM_EyeDoor{,Bottom}Facing*_1</c>: the still component waits for Samus to come near.</summary>
    ComponentIdle,
    /// <summary><c>InstList_PLM_EyeDoor{,Bottom}Facing*_2</c>: the component arms its door-bit wake.</summary>
    ComponentActivate,
    /// <summary><c>InstList_PLM_EyeDoor{,Bottom}Facing*_3</c>: the component pulses while Samus stays near.</summary>
    ComponentCycle,
    /// <summary><c>InstList_PLM_EyeDoor{,Bottom}Facing*_4</c>: the opened door's component deletes itself.</summary>
    ComponentDelete,
}
