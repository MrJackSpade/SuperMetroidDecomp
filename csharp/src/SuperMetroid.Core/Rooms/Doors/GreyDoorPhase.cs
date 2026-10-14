namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Debugger-visible phase of one resident grey-door PLM.</summary>
public enum GreyDoorPhase : byte
{
    /// <summary>The closed grey cap polls its condition and consumes rejected hits; the hit present on the unlock pass is also cleared.</summary>
    Locked,
    /// <summary>The condition has unlocked the door and its flash list repeats until a later projectile trigger starts opening.</summary>
    Flashing,
    /// <summary>A post-unlock hit has recorded the persistent door bit and handed opening sound, timed draws, and deletion to the shared interpreter.</summary>
    Opening,
    /// <summary>Room setup found the door already opened and will install the closed blue cap and BTS, then release the resident slot.</summary>
    ConvertToBlue,
    /// <summary>The room-entry closing list runs in the shared interpreter before returning this resident actor to its locked initial list.</summary>
    Closing,
}
