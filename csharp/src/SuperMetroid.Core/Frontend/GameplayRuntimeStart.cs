using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.Frontend;

/// <summary>Everything the session hands a new gameplay runtime at its start boundary.</summary>
internal sealed record GameplayRuntimeStart
{
    /// <summary>Why the runtime is created; a selected game carries <see cref="Selected"/>, an attract demo does not.</summary>
    public required GameplayRuntimePurpose Purpose { get; init; }

    /// <summary>The host's tester conveniences; an attract demo's are <see cref="HostGameplayOverrides.None"/>.</summary>
    public required HostGameplayOverrides Overrides { get; init; }

    /// <summary>What the session keeps across runtimes: its clock, random word, floor residue and power-on rumble delay.</summary>
    public required GameplaySessionCarryover Carryover { get; init; }

    /// <summary>The menu's retained NMI counters and layer-1 position, published to the new runtime.</summary>
    public required MenuLayer1Retention Layer1 { get; init; }

    /// <summary>The installed catalogs; the runtime loads their initial palette and binds every catalog.</summary>
    public required GameArtwork Artwork { get; init; }

    /// <summary>The menu choices of a selected game; <see langword="null"/> for an attract demo.</summary>
    public required SelectedGameplayOptions? Selected { get; init; }
}
