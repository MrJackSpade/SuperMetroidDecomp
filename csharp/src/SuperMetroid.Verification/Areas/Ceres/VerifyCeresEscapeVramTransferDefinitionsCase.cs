using SuperMetroid.Core.Bosses.Ridley;

namespace SuperMetroid.Verification.Areas.Ceres;

/// <summary>The start and cursor of one verify ceres escape vram transfer definitions case.</summary>
/// <param name="Start">The start.</param>
/// <param name="Cursor">The cursor.</param>
internal readonly record struct VerifyCeresEscapeVramTransferDefinitionsCase(ushort Start, CeresEscapeTransferCursor Cursor);
