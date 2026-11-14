namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>One hop's fixed mechanics; the delta also serves as packed 1/256-pixel dropping speed.</summary>
public readonly record struct PuyoHopDefinition(int Height, int XSpeed, int YIndexDelta, PuyoAirborneMovement Movement);
