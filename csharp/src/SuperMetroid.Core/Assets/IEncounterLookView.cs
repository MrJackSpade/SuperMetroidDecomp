namespace SuperMetroid.Core.Assets;

/// <summary>
/// The looks the room's encounters show (#1466, #1538), which the Ceres Ridley arena's backgrounds follow: they show the
/// escape's timer and warning once the getaway has brought them up.
/// </summary>
public interface IEncounterLookView
{
    /// <summary>Whether the room's encounter shows <paramref name="look"/> now.</summary>
    bool Shows(EncounterLook look);
}
