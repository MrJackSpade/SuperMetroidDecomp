namespace SuperMetroid.Core.Rooms;

/// <summary>Native bank-$8F room-main callback offsets present in retail room states.</summary>
/// <remarks>
/// A zero room-main word means no callback and is intentionally absent. These constants
/// identify code; mutable timers and room-main variables remain with their owning systems.
/// </remarks>
public static class RoomMainCodePointers
{
    /// <summary><c>$8F:C116 MainASM_ScrollingSkyLand</c>.</summary>
    public const ushort ScrollingSkyLand = 0xc116;
    /// <summary><c>$8F:C11B MainASM_ScrollingSkyOcean</c>.</summary>
    public const ushort ScrollingSkyOcean = 0xc11b;
    /// <summary><c>$8F:C120 MainASM_ScrollingSkyLand_ZebesTimebombSet</c>.</summary>
    public const ushort ScrollingSkyLandZebesTimebombSet = 0xc120;
    /// <summary><c>$8F:C124 MainASM_SetScreenShaking_GenerateRandomExplosions</c>.</summary>
    public const ushort SetScreenShakingAndGenerateRandomExplosions = 0xc124;
    /// <summary><c>$8F:C1E6 MainASM_ScrollScreenRightInDachoraRoom</c>.</summary>
    public const ushort ScrollScreenRightInDachoraRoom = 0xc1e6;
    /// <summary><c>$8F:E2B6 MainASM_Elevatube</c>.</summary>
    public const ushort MaridiaElevatube = 0xe2b6;
    /// <summary><c>$8F:E51F MainASM_CeresElevatorShaft</c>.</summary>
    public const ushort CeresElevatorShaft = 0xe51f;
    /// <summary><c>$8F:E524 RTS_8FE524</c>: an explicit no-op main callback.</summary>
    public const ushort Return = 0xe524;
    /// <summary><c>$8F:E525 MainASM_SpawnCeresPreElevatorHallFallingDebris</c>.</summary>
    public const ushort SpawnCeresPreElevatorHallFallingDebris = 0xe525;
    /// <summary><c>$8F:E571 MainASM_HandleCeresRidleyGetawayCutscene</c>.</summary>
    public const ushort HandleCeresRidleyGetawayCutscene = 0xe571;
    /// <summary><c>$8F:E57C MainASM_ShakeScreenSwitchingBetweenLightHorizAndMediumDiag</c>.</summary>
    public const ushort ShakeScreenLightHorizontalAndMediumDiagonal = 0xe57c;
    /// <summary><c>$8F:E5A0 MainASM_GenerateRandomExplosionOnEveryFourthFrame</c>.</summary>
    public const ushort GenerateRandomExplosionEveryFourthFrame = 0xe5a0;
    /// <summary><c>$8F:E5A4 MainASM_ShakeScreenSwitchingBetweenMediumHorizAndStrongDiag</c>.</summary>
    public const ushort ShakeScreenMediumHorizontalAndStrongDiagonal = 0xe5a4;
    /// <summary><c>$8F:E8CD MainASM_CrocomiresRoomShaking</c>.</summary>
    public const ushort CrocomireRoomShaking = 0xe8cd;
    /// <summary><c>$8F:E950 MainASM_RidleysRoomShaking</c>.</summary>
    public const ushort RidleyRoomShaking = 0xe950;
}
