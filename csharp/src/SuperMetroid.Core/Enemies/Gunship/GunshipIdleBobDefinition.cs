namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>One timer and signed whole-pixel delta in the gunship idle-bob cycle.</summary>
internal readonly record struct GunshipIdleBobDefinition(int Timer, int YDelta);
