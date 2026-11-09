namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The three independently authored room objects which together form one eye door.</summary>
public enum EyeDoorComponent
{
    /// <summary>Live eye room object: receives missile hits, fires attack/sweat actors, sets the persistent opened-door bit, and ultimately becomes the ordinary blue-door cap.</summary>
    Eye,
    /// <summary>Passive main door body: installs special-solid collision and follows the shared opened-door bit rather than accepting the eye's projectile hits itself.</summary>
    Door,
    /// <summary>Separately animated lower component: installs special-solid collision and waits for the eye-owned opened-door bit before its terminal removal.</summary>
    Bottom,
}
