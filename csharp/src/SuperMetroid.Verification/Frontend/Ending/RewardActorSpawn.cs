namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>A reward actor's spawn pointer and the address that loads it.</summary>
/// <param name="Pointer">The actor pointer.</param>
/// <param name="Load">The address of the instruction that loads it.</param>
internal readonly record struct RewardActorSpawn(ushort Pointer, int Load);
