namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>Where each native bank-$84 list of one facing's eye-door body or bottom begins (#1554).</summary>
/// <param name="Start">The first look: the opened-door branch, falling into <paramref name="Idle"/>.</param>
/// <param name="Idle">The still part looking for Samus.</param>
/// <param name="Activate">The delete link and the opened-door wake, falling into <paramref name="Cycle"/>.</param>
/// <param name="Cycle">The pulse and its pulse-again branch.</param>
/// <param name="Delete">The delete.</param>
internal sealed record NativeEyeDoorPartLists(ushort Start, ushort Idle, ushort Activate, ushort Cycle, ushort Delete);
