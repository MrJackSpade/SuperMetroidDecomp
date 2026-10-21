namespace SuperMetroid.Verification.Enemies;

/// <summary>The pointer and count of one verify corpse metadata definitions case.</summary>
/// <param name="Pointer">The pointer.</param>
/// <param name="Count">The count.</param>
internal readonly record struct VerifyCorpseMetadataDefinitionsCase(ushort Pointer, int Count);
