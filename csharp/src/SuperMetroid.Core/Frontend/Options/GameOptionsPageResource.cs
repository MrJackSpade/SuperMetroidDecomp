namespace SuperMetroid.Core.Frontend.Options;

/// <summary>One compressed options-page resource and its diagnostic name.</summary>
/// <param name="Address">24-bit cartridge byte address of the bank-$97 compressed tilemap stream, used by asset extraction.</param>
/// <param name="Description">Human-readable page and language identity for extraction diagnostics.</param>
public readonly record struct GameOptionsPageResource(int Address, string Description);
