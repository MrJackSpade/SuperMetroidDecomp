namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The scripted room PLMs' programs (#1296), in place of their lists.</summary>
internal enum ScriptedRoomObjectProgram
{
    /// <summary>The elevator platform's idle loop.</summary>
    ElevatorPlatform,
    /// <summary>Shaktool's room controller: installs its pre-instruction and sleeps.</summary>
    ShaktoolRoomController,
    /// <summary>The old Tourian escape wall waits for Samus.</summary>
    OldTourianWallWait,
    /// <summary>The old Tourian escape wall opens once Samus passes.</summary>
    OldTourianWallOpen,
    /// <summary>Mother Brain's escape gate.</summary>
    MotherBrainEscapeGate,
    /// <summary>Mother Brain's escape gate closing.</summary>
    MotherBrainEscapeGateClosing,
}
