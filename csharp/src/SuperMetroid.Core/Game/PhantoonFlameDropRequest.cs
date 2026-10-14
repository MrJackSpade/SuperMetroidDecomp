namespace SuperMetroid.Core.Game;

/// <summary>
/// One pickup request emitted by projectile instruction <c>$86:980E</c> after Samus shoots
/// a destroyable Phantoon flame. Native passes the Phantoon-eye enemy header to the common
/// drop selector; retaining both that header and its table pointer makes the otherwise
/// implicit choice explicit to the eventual pickup-system owner.
/// </summary>
public readonly record struct PhantoonFlameDropRequest();
