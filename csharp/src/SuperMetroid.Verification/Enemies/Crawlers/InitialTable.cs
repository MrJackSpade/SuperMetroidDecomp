using SuperMetroid.Core.Enemies.Crawlers;

namespace SuperMetroid.Verification.Enemies.Crawlers;

/// <summary>The family, address and offset of one initial table.</summary>
/// <param name="Family">The family.</param>
/// <param name="Address">The address.</param>
/// <param name="Offset">The offset.</param>
internal readonly record struct InitialTable(CrawlerAnimationFamily Family, int Address, ushort? Offset);
