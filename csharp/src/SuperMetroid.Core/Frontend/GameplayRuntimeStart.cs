using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>Everything the session hands a new gameplay runtime at its start boundary.</summary>
internal sealed record GameplayRuntimeStart
{
    /// <summary>Why the runtime is created; a selected game carries <see cref="Selected"/>, an attract demo does not.</summary>
    public required GameplayRuntimePurpose Purpose { get; init; }

    /// <summary>The host's tester conveniences; an attract demo's are <see cref="HostGameplayOverrides.None"/>.</summary>
    public required HostGameplayOverrides Overrides { get; init; }

    /// <summary>The session's random-number word, which the runtime's system state reads and advances.</summary>
    public required SharedRandom Random { get; init; }

    /// <summary>The boot-time main-loop carry the session captured at power-on.</summary>
    public required bool MainGameLoopCarry { get; init; }

    /// <summary>The menu's retained NMI counters and layer-1 position, published to the new runtime.</summary>
    public required MenuLayer1Retention Layer1 { get; init; }

    /// <summary>The installed catalogs; the runtime loads their initial palette and binds every catalog.</summary>
    public required GameArtwork Artwork { get; init; }

    /// <summary>The session's frame clock, which counts across runtimes.</summary>
    public required FrameClock Clock { get; init; }

    /// <summary>The session's Mode 7 transfer queue residue, which outlives each runtime as WRAM does.</summary>
    public required SuperMetroid.Core.Compatibility.FloorUpdateResidue FloorQueueResidue { get; init; }

    /// <summary>The menu choices of a selected game; <see langword="null"/> for an attract demo.</summary>
    public required SelectedGameplayOptions? Selected { get; init; }
}
