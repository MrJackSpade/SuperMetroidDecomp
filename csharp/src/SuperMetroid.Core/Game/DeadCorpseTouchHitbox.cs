namespace SuperMetroid.Core.Game;

/// <summary>Native signed rectangle words retained as ushort for asymmetric overlap arithmetic.</summary>
internal readonly record struct DeadCorpseTouchHitbox(ushort Left, ushort Top, ushort Right, ushort Bottom);
