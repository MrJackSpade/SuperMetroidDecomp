using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Frontend;

/// <summary>Everything the session hands a new gameplay runtime at its start boundary.</summary>
internal sealed record GameplayRuntimeStart
{
    /// <summary>Why the runtime is created; a selected game carries <see cref="Selected"/>, an attract demo does not.</summary>
    public required GameplayRuntimePurpose Purpose { get; init; }

    /// <summary>The assists the runtime plays under; an attract demo's are <see cref="GameplayAssists.None"/>.</summary>
    public required GameplayAssists Assists { get; init; }

    /// <summary>What the session keeps across runtimes: its clock, random number generator, unauthored Speed Booster cadence, power-on rumble delay and camera fractions.</summary>
    public required GameplaySessionCarryover Carryover { get; init; }

    /// <summary>The installed catalogs; the runtime loads their initial palette and binds every catalog.</summary>
    public required GameArtwork Artwork { get; init; }

    /// <summary>The menu choices of a selected game; <see langword="null"/> for an attract demo.</summary>
    public required SelectedGameplayOptions? Selected { get; init; }
}
