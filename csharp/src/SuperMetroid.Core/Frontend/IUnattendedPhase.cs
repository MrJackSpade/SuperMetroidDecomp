namespace SuperMetroid.Core.Frontend;

/// <summary>A sequence whose steps run without gameplay and read no controller input, as the ending's credits do.</summary>
internal interface IUnattendedPhase : IGamePhase
{
    /// <inheritdoc/>
    PhaseGameplayNeed IGamePhase.Need => PhaseGameplayNeed.Nothing;

    /// <summary>Runs one update of the sequence.</summary>
    void Step();
}
