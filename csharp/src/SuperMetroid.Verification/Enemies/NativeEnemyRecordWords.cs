namespace SuperMetroid.Verification.Enemies;

/// <summary>The six actor-owned words of a native enemy record (+$30 to +$3A), as a recorded snapshot holds them.</summary>
/// <param name="A">Word A at +$30.</param>
/// <param name="B">Word B at +$32.</param>
/// <param name="C">Word C at +$34.</param>
/// <param name="D">Word D at +$36.</param>
/// <param name="E">Word E at +$38.</param>
/// <param name="F">Word F at +$3A.</param>
internal readonly record struct NativeEnemyRecordWords(ushort A, ushort B, ushort C, ushort D, ushort E, ushort F);
