using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// Room-level register shadows an accepted NMI uploaded (<c>$80:91EE</c>): BG mode, character
/// bases, the layer-blending selectors behind TM/TS/CGWSEL/CGADSUB, and the scrolling sky's
/// BG2 map and vertical scroll. Main-loop writes to these shadows are displayed one frame later.
/// </summary>
/// <param name="CeresElevatorMode7">Whether BGMODE selects the Ceres elevator shaft's Mode 7.</param>
/// <param name="UsesCeresRidleyCharacterBase">Whether BG12NBA selects the Ceres Ridley arena's character base.</param>
/// <param name="HudCharacterWord">BG3 character base VRAM word address selected by BG34NBA.</param>
/// <param name="FxType">Room FX type whose layer-blending handler wrote the blend shadows.</param>
/// <param name="LayerBlendConfiguration">Layer-blending configuration resolved into the blend shadows.</param>
/// <param name="ScrollingSky">Scrolling-sky BG2 map and vertical-scroll shadows, when the room has a sky.</param>
/// <param name="Room">Loaded room whose X-ray blending branch wrote the X-ray blend shadows, or null before a room exists.</param>
/// <param name="BossId">Area boss identifier read by the X-ray blending branch, 0 through 65535; only compared for equality.</param>
public readonly record struct GameplayLatchedSceneRegisters(
    bool CeresElevatorMode7,
    bool UsesCeresRidleyCharacterBase,
    int HudCharacterWord,
    RoomFxType FxType,
    LayerBlendingConfiguration LayerBlendConfiguration,
    ScrollingSkyRegisterSnapshot? ScrollingSky,
    RoomHeaderDefinition? Room,
    int BossId)
{
    /// <summary>BG3 character base VRAM word address selected by BG34NBA; a value outside VRAM is rejected.</summary>
    public int HudCharacterWord { get; init; } = Hardware.SnesPpuLayout.RequireVramWord(HudCharacterWord);
}
