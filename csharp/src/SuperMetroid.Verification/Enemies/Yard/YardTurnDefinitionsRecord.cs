using SuperMetroid.Core.Enemies.Yard;

namespace SuperMetroid.Verification.Enemies.Yard;

/// <summary>The crawl, disabled and address of one yard turn definitions record.</summary>
/// <param name="Crawl">The way the Yard crawls.</param>
/// <param name="Disabled">The disabled.</param>
/// <param name="Address">The address.</param>
internal readonly record struct YardTurnDefinitionsRecord(YardDirection Crawl, bool Disabled, int Address);
