namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>What a scroll extension block writes: an extension leading one block back toward the trigger it extends.</summary>
/// <param name="TowardTrigger">The way back to the trigger.</param>
internal readonly record struct ScrollExtension(ScrollExtensionDirection TowardTrigger);
