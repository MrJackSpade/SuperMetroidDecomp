namespace SuperMetroid.Core.Progression;

/// <summary>
/// The installed key of each <see cref="SaveStartupRoute"/> (#2520), written out so renaming a member never changes
/// a name the installation, an installed document or a save spells it by.
/// </summary>
internal static class SaveStartupRouteKeys
{
    /// <summary>Each SaveStartupRoute member's installed key.</summary>
    internal static InstalledKeys<SaveStartupRoute> Installed { get; } = new(new Dictionary<SaveStartupRoute, string>
    {
        [SaveStartupRoute.OpeningCinematic] = "OpeningCinematic",
        [SaveStartupRoute.MainGame] = "MainGame",
        [SaveStartupRoute.CeresElevatorArrival] = "CeresElevatorArrival",
        [SaveStartupRoute.CeresDestruction] = "CeresDestruction",
    });
}
