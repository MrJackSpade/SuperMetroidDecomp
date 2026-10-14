namespace SuperMetroid.Core.Game;

/// <summary>One bank-$88 request for the global room-shake type and timer bits.</summary>
public readonly record struct RoomFxEarthquakeRequest(
    ushort Type,
    ushort TimerBits);
