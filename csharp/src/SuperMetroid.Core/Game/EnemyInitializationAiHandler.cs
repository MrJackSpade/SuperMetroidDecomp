using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>One translated enemy initialization AI routine (the definition's init pointer).</summary>
internal delegate void EnemyInitializationAiHandler(
    RoomEnemySlot slot, RoomLevelData? level, SamusState? samus, ushort controllerInput, ushort cameraX);
