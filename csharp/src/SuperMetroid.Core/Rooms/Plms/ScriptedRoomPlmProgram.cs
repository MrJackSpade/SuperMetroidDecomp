namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The scripted room PLMs' programs (#1296), in place of their bank-$84 lists.</summary>
internal enum ScriptedRoomPlmProgram
{
    /// <summary>The elevator platform's idle loop.</summary>
    ElevatorPlatform,
    /// <summary>Shaktool's room controller: installs its pre-instruction and sleeps.</summary>
    ShaktoolRoomController,
    /// <summary>The Maridia elevatube.</summary>
    MaridiaElevatube,
    /// <summary>The old Tourian escape wall waits for Samus.</summary>
    OldTourianWallWait,
    /// <summary>The old Tourian escape wall opens once Samus passes.</summary>
    OldTourianWallOpen,
    /// <summary>The escape animals' block breaking.</summary>
    EscapeAnimalReaction,
    /// <summary>The Crateria escape passage opening.</summary>
    CrateriaPassage,
    /// <summary>Mother Brain's escape gate.</summary>
    MotherBrainEscapeGate,
    /// <summary>Mother Brain's escape gate closing.</summary>
    MotherBrainEscapeGateClosing,
    /// <summary>Terrain an enemy breaks.</summary>
    EnemyBreakableTerrain,
    /// <summary>The Lower Norfair Chozo room plug crumbling.</summary>
    ChozoCrumblePlug,
    /// <summary>The Lower Norfair Chozo hand: waits for Samus, or continues to the lowered-acid program.</summary>
    LowerNorfairChozoHand,
    /// <summary>The Lower Norfair Chozo hand once its acid is lowered.</summary>
    LowerNorfairChozoHandLowered,
    /// <summary>The Wrecked Ship Chozo slope access cleared.</summary>
    ChozoClearSlope,
    /// <summary>The Wrecked Ship Chozo slope access blocked.</summary>
    ChozoBlockSlope,
}
