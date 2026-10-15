namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Stored physical bounds before touch and shot callbacks are attached.</summary>
internal readonly record struct KraidArmCollisionGeometry(short Left, short Top, short Right, short Bottom);
