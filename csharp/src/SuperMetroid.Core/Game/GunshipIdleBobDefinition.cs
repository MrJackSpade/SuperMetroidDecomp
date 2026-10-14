namespace SuperMetroid.Core.Game;

/// <summary>One timer and signed whole-pixel delta in the gunship idle-bob cycle.</summary>
internal readonly record struct GunshipIdleBobDefinition(byte Timer, sbyte YDelta);
