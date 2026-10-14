namespace SuperMetroid.Core.Game;

/// <summary>Botwoon's health-dependent movement, body-history spacing and spit speed.</summary>
public readonly record struct BotwoonSpeedDefinition(ushort MovementSpeed, ushort SegmentSpacingBytes, ushort SpitSpeed);
