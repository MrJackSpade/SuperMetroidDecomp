using SuperMetroid.Core.Enemies.Yard;

namespace SuperMetroid.Verification.Enemies.Yard;

/// <summary>One Yard turn record of the cartridge: the crawl it serves, whether it is the suppressed turn, and where it lies.</summary>
/// <param name="Crawl">The way the Yard crawls.</param>
/// <param name="TurnsSuppressed">Whether the record is the turn the crawl takes while the Yard's turns are suppressed.</param>
/// <param name="Address">The cartridge address of the record.</param>
internal readonly record struct YardTurnDefinitionsRecord(YardDirection Crawl, bool TurnsSuppressed, int Address);
