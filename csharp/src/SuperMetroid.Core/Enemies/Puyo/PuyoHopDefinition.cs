namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>One native hop's fixed mechanics; the delta also serves as packed 8.8 dropping speed.</summary>
public readonly record struct PuyoHopDefinition(ushort Height, ushort XSpeed, ushort YIndexDelta, PuyoAirborneFunction Function);
