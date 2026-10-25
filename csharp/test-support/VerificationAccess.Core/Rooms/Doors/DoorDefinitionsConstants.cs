namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Cartridge values of <see cref="DoorDefinitions"/> that only verification reads.</summary>
internal static class DoorDefinitionsConstants
{
    /// <summary>Number of physical door records across the two native bank-$83 blocks.</summary>
    public const int HeaderCount = 597;

    /// <summary>Provides the number of encoded native door headers expected by door import verification.</summary>
    extension(DoorDefinitions)
    {
        /// <inheritdoc cref="DoorDefinitionsConstants.HeaderCount"/>
        internal static int HeaderCount => DoorDefinitionsConstants.HeaderCount;
    }
    /// <summary>Every retail door, in the cartridge's record order.</summary>
    public static IReadOnlyList<DoorDefinition> All => DoorDefinitions.Definitions;
}
