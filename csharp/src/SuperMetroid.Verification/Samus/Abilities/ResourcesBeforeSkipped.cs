namespace SuperMetroid.Verification.Samus.Abilities;

/// <summary>The health, missiles, super missiles and power bombs of one resources before skipped.</summary>
/// <param name="Health">The health.</param>
/// <param name="Missiles">The missiles.</param>
/// <param name="SuperMissiles">The super missiles.</param>
/// <param name="PowerBombs">The power bombs.</param>
internal readonly record struct ResourcesBeforeSkipped(int Health, int Missiles, int SuperMissiles, int PowerBombs);
