namespace SuperMetroid.Verification.Areas.Escape;

/// <summary>The pointer and id of one lookup escape gate stock frame.</summary>
/// <param name="Pointer">The pointer.</param>
/// <param name="Id">The id.</param>
internal readonly record struct LookupEscapeGateStockFrame(ushort Pointer, string Id);
