namespace SuperMetroid.Core.Progression;

/// <summary>
/// The installed key of each <see cref="SaveLoadingGameState"/> (#2520), written out so renaming a member never changes
/// a name the installation, an installed document or a save spells it by.
/// </summary>
internal static class SaveLoadingGameStateKeys
{
    /// <summary>Each SaveLoadingGameState member's installed key.</summary>
    internal static InstalledKeys<SaveLoadingGameState> Installed { get; } = new(new Dictionary<SaveLoadingGameState, string>
    {
        [SaveLoadingGameState.OpeningCinematic] = "OpeningCinematic",
        [SaveLoadingGameState.MainGame] = "MainGame",
        [SaveLoadingGameState.CeresElevatorArrival] = "CeresElevatorArrival",
        [SaveLoadingGameState.CeresDestruction] = "CeresDestruction",
    });
}
