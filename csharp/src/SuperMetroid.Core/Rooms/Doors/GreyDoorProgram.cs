namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>A grey door cap's PLM programs (#1296), one set per facing (and one for Bomb Torizo's door).</summary>
internal enum GreyDoorProgram
{
    /// <summary>The cap closes behind Samus, then runs into <see cref="Waiting"/>.</summary>
    Closing,
    /// <summary>Becomes a blue cap once opened before; otherwise links the activation, arms the unlock check and draws the cap.</summary>
    Waiting,
    /// <summary>Sleeps until the unlock condition wakes the cap at <see cref="Activate"/>.</summary>
    Sleep,
    /// <summary>Links the hit, arms the shot wake and runs into the flash.</summary>
    Activate,
    /// <summary>Flashes blue until a shot wakes the cap at <see cref="Hit"/>.</summary>
    Flash,
    /// <summary>Counts the hit and runs into the opening.</summary>
    Hit,
    /// <summary>The cap opens and deletes.</summary>
    Open,
}
