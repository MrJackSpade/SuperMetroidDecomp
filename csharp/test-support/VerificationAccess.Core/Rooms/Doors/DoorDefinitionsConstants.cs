namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Cartridge values of <see cref="DoorDefinitions"/> that only verification reads.</summary>
internal static class DoorDefinitionsConstants
{
    /// <summary>Every retail door, in the cartridge's record order.</summary>
    public static IReadOnlyList<DoorDefinition> All => DoorDefinitions.Definitions;
}
