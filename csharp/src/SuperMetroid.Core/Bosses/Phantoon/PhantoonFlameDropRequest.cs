namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// One pickup request emitted by the flame's drop step after Samus shoots
/// a destroyable Phantoon flame. Passes the Phantoon eye's enemy definition to the common
/// drop selector; retaining that definition makes the otherwise
/// implicit choice explicit to the eventual pickup-system owner.
/// </summary>
public readonly record struct PhantoonFlameDropRequest();
