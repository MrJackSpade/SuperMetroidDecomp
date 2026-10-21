using SuperMetroid.Core.Enemies.Yard;

namespace SuperMetroid.Verification.Enemies.Yard;

/// <summary>The movement, disabled and address of one yard turn definitions record.</summary>
/// <param name="Movement">The movement.</param>
/// <param name="Disabled">The disabled.</param>
/// <param name="Address">The address.</param>
internal readonly record struct YardTurnDefinitionsRecord(YardMovementFunction Movement, bool Disabled, int Address);
