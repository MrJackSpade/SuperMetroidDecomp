namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Where one Torizo artwork swap's characters land among the sprite characters.</summary>
/// <param name="FirstCharacter">The sprite character the swap's first character is written to.</param>
internal readonly record struct TorizoSwapDestination(int FirstCharacter);
