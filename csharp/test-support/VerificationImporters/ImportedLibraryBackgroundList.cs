using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.TestSupport.VerificationImporters;

/// <summary>
/// One bank-$8F background command list decoded for comparison (#1649): its commands under the name diagnostics give it.
/// A decoded list builds no library background of its own, so it names none.
/// </summary>
/// <param name="Name">The list's name in diagnostics: its place in bank $8F.</param>
/// <param name="Instructions">Its nonterminating commands in execution order.</param>
internal sealed record ImportedLibraryBackgroundList(string Name, IReadOnlyList<LibraryBackgroundInstruction> Instructions);
