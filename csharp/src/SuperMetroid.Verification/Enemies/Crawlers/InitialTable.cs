using SuperMetroid.Core.Enemies.Crawlers;

namespace SuperMetroid.Verification.Enemies.Crawlers;

/// <summary>One crawler species' native initial table and the family whose crawls it selects.</summary>
/// <param name="Family">The family.</param>
/// <param name="Address">The table's cartridge address.</param>
/// <param name="OrangeZoomer">
/// Whether the table is the orange Zoomer's own, which its separate setup reads and which selects the shared family's
/// crawls (#2369).
/// </param>
internal readonly record struct InitialTable(CrawlerAnimationFamily Family, int Address, bool OrangeZoomer = false);
