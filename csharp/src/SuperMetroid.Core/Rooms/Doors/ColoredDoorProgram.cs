namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>A coloured door cap's PLM programs (#1296), one set per colour and facing.</summary>
internal enum ColoredDoorProgram
{
    /// <summary>The cap closes behind Samus (the header's second list), then runs into <see cref="Waiting"/>.</summary>
    Closing,
    /// <summary>Becomes a blue cap once opened before; otherwise links the hit, arms the shot wake and draws the cap.</summary>
    Waiting,
    /// <summary>Sleeps until the right projectile wakes the cap at <see cref="Hit"/>.</summary>
    Sleep,
    /// <summary>Counts the hit, opening the cap at its threshold; otherwise flashes and sleeps again.</summary>
    Hit,
    /// <summary>The cap opens and deletes.</summary>
    Open,
}
