namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>A Powamp body's balloon half: its enemy slot and the state holding its spawn point and typed animation.</summary>
/// <param name="Slot">The balloon's enemy slot, the literal slot before its body.</param>
/// <param name="State">The balloon's Powamp state.</param>
internal readonly record struct PowampBalloon(RoomEnemySlot Slot, PowampEnemyState State);
