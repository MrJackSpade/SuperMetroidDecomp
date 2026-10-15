namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>
/// Event emitted by Kago bug instruction <c>$86:D1CE</c>. Pickup selection remains owned by
/// the shared enemy-drop subsystem; this retains the exact cartridge definition and origin.
/// </summary>
public readonly record struct KagoBugDropRequest();
