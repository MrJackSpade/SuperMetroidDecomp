namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>The collides and xdisplacement of one expected mother brain contact result.</summary>
/// <param name="Collides">The collides.</param>
/// <param name="XDisplacement">The xdisplacement.</param>
internal readonly record struct ExpectedMotherBrainContactResult(bool Collides, ushort XDisplacement);
