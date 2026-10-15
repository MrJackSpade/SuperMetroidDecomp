namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>A resident door cap's typed program, which a door transition can close around Samus (#1296).</summary>
internal interface IResidentDoorAnimation
{
    /// <summary>Restarts the door at its closing program, as the transition installs the header's closing list.</summary>
    void StartClosing();
}
