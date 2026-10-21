using SuperMetroid.Core.Frontend.Options;

namespace SuperMetroid.Verification.Harness;

/// <summary>The page, address and count of one verify stream3 options borders case.</summary>
/// <param name="Page">The page.</param>
/// <param name="Address">The address.</param>
/// <param name="Count">The count.</param>
internal readonly record struct VerifyStream3OptionsBordersCase(GameOptionsPage Page, int Address, int Count);
