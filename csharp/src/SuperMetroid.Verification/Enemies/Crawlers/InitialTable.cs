using SuperMetroid.Core.Enemies.Crawlers;

namespace SuperMetroid.Verification.Enemies.Crawlers;

/// <summary>One crawler family's native initial table and the surface family its species initializer fixes.</summary>
/// <param name="Family">The family.</param>
/// <param name="Address">The table's cartridge address.</param>
/// <param name="SpeciesSurfaceFamily">The surface family the species initializer fixes, or none when the entry names it.</param>
internal readonly record struct InitialTable(CrawlerAnimationFamily Family, int Address, CrawlerAnimationFamily? SpeciesSurfaceFamily);
