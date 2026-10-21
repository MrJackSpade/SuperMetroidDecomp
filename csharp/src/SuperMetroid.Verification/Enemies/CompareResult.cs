namespace SuperMetroid.Verification.Enemies;

/// <summary>The deleted and updates of one compare result.</summary>
/// <param name="Deleted">The deleted.</param>
/// <param name="Updates">The updates.</param>
internal readonly record struct CompareResult(bool Deleted, int Updates);
