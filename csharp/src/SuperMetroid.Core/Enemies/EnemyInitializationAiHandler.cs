using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies;

/// <summary>One translated enemy initialization AI routine (the definition's init pointer).</summary>
internal delegate void EnemyInitializationAiHandler(
    RoomEnemySlot slot, RoomLevelData? level, SamusState? samus, SnesButton controllerInput, ushort cameraX);
