using SuperMetroid.Core.Input;

namespace SuperMetroid.Verification.Input;

/// <summary>A recording whose loop the cartridge authors as its own list (#1600): the input the loop starts at and the list's entry.</summary>
/// <param name="Recording">The recording.</param>
/// <param name="FirstRecord">The recording's input the loop starts at.</param>
/// <param name="Entry">The loop list's native entry.</param>
internal readonly record struct DemoInputNativeLoopEntry(DemoInputRecordingId Recording, int FirstRecord, ushort Entry);
