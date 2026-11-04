namespace SuperMetroid.Core.Assets;

/// <summary>
/// How far the room's encounters have played their artwork sequences (#1466, #1513), which the Ceres Ridley arena's
/// backgrounds follow: they show the escape's pages once the getaway has brought them up.
/// </summary>
public interface IEncounterArtworkView
{
    /// <summary>Whether the room's encounter has played <paramref name="sequence"/> through step <paramref name="step"/>.</summary>
    bool Reached(EncounterArtworkSequence sequence, int step);
}
