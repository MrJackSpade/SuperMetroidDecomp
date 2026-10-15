namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The three independently authored PLMs which together form one eye door.</summary>
public enum EyeDoorComponent : byte
{
    /// <summary>Live eye PLM, native headers $84:DB48/$DB56: receives missile hits, fires attack/sweat actors, sets the persistent opened-door bit, and ultimately becomes the ordinary blue-door cap.</summary>
    Eye,
    /// <summary>Passive main door body, native headers $84:DB4C/$DB5A: installs special-solid collision and follows the shared opened-door bit rather than accepting the eye's projectile hits itself.</summary>
    Door,
    /// <summary>Separately animated lower component, native headers $84:DB52/$DB60: installs special-solid collision and waits for the eye-owned opened-door bit before its terminal removal.</summary>
    Bottom,
}
