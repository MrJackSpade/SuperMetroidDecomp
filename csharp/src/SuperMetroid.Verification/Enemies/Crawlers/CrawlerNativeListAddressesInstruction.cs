using SuperMetroid.Core.Enemies.Crawlers;

namespace SuperMetroid.Verification.Enemies.Crawlers;

/// <summary>The kind and operands of one crawler native list addresses instruction.</summary>
/// <param name="Kind">The kind.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct CrawlerNativeListAddressesInstruction(CrawlerCueKind Kind, string Operands);
