using SuperMetroid.Core.Enemies.Crawlers;

namespace SuperMetroid.Verification.Enemies.Crawlers;

/// <summary>One crawler family's native initial table.</summary>
/// <param name="Family">The family.</param>
/// <param name="Address">The table's cartridge address.</param>
internal readonly record struct InitialTable(CrawlerAnimationFamily Family, int Address);
