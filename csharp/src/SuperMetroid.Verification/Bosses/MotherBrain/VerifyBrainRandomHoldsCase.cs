namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>The random and holds of one verify brain random holds case.</summary>
/// <param name="Random">The random.</param>
/// <param name="Holds">The holds.</param>
internal readonly record struct VerifyBrainRandomHoldsCase(ushort Random, bool Holds);
