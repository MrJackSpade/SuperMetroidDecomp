using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// Room-level register shadows an accepted NMI uploaded (<c>$80:91EE</c>): BG mode, character
/// bases, the layer-blending selectors behind TM/TS/CGWSEL/CGADSUB, and the scrolling sky's
/// BG2 map and vertical scroll. Main-loop writes to these shadows are displayed one frame later.
/// </summary>
/// <param name="CeresElevatorMode7">Whether BGMODE selects the Ceres elevator shaft's Mode 7.</param>
/// <param name="Bg12CharacterWord">BG1/BG2 character base word selected by BG12NBA.</param>
/// <param name="HudCharacterWord">BG3 character base word selected by BG34NBA.</param>
/// <param name="FxType">Room FX type whose layer-blending handler wrote the blend shadows.</param>
/// <param name="LayerBlendConfiguration">Layer-blending configuration resolved into the blend shadows.</param>
/// <param name="ScrollingSky">Scrolling-sky BG2 map and vertical-scroll shadows, when the room has a sky.</param>
/// <param name="RoomPointer">Loaded room whose X-ray blending branch wrote the X-ray blend shadows, or null before a room exists.</param>
/// <param name="BossId">Area boss identifier read by the X-ray blending branch.</param>
public readonly record struct GameplayLatchedSceneRegisters(
    bool CeresElevatorMode7,
    ushort Bg12CharacterWord,
    ushort HudCharacterWord,
    RoomFxType FxType,
    LayerBlendingConfiguration LayerBlendConfiguration,
    ScrollingSkyRegisterSnapshot? ScrollingSky,
    ushort? RoomPointer,
    ushort BossId);
