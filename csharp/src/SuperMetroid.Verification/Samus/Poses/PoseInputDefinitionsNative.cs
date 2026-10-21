namespace SuperMetroid.Verification.Samus.Poses;

/// <summary>The new, held and target of one pose input definitions native.</summary>
/// <param name="New">The new.</param>
/// <param name="Held">The held.</param>
/// <param name="Target">The target.</param>
internal readonly record struct PoseInputDefinitionsNative(ushort New, ushort Held, ushort Target);
