namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The room FX as room PLMs use it (#1395): the liquid's motion words and the room's water physics.</summary>
public interface IRoomPlmFx : IRoomFxMotion, IRoomWaterPhysics;
