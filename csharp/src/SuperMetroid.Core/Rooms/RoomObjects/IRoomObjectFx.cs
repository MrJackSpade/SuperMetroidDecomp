namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The room FX as room objects use it (#1395): the liquid's motion values and the room's water physics.</summary>
public interface IRoomObjectFx : IRoomFxMotion, IRoomWaterPhysics;
