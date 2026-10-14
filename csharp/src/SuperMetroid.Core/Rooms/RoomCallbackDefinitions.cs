namespace SuperMetroid.Core.Rooms;

/// <summary>Typed dispatch identities for every main/setup pointer used by retail room states.</summary>
public static class RoomCallbackDefinitions
{
    /// <summary>Resolves a bank-$8F room-main pointer to its exclusive typed dispatch identity.</summary>
    /// <param name="pointer">The room state's main-code offset; zero selects no callback.</param>
    /// <returns>The callback selected for subsequent room-main updates.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The pointer does not identify a retail room-main callback.</exception>
    public static RoomMainCallback ResolveMain(ushort pointer) =>
        Enum.IsDefined((RoomMainCallback)pointer)
            ? (RoomMainCallback)pointer
            : throw new ArgumentOutOfRangeException(nameof(pointer), pointer,
                "Pointer is not a retail room-main callback.");

    /// <summary>Resolves a bank-$8F room-setup pointer to its exclusive typed dispatch identity.</summary>
    /// <param name="pointer">The room state's setup-code offset; zero selects no callback.</param>
    /// <returns>The callback selected to run after PLM creation and door ASM, before elevator finalization.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The pointer does not identify a retail room-setup callback.</exception>
    public static RoomSetupCallback ResolveSetup(ushort pointer) =>
        Enum.IsDefined((RoomSetupCallback)pointer)
            ? (RoomSetupCallback)pointer
            : throw new ArgumentOutOfRangeException(nameof(pointer), pointer,
                "Pointer is not a retail room-setup callback.");

    /// <summary>Determines whether the selected setup callback initializes the Ceres haze effect.</summary>
    /// <param name="callback">The room's resolved setup callback.</param>
    /// <returns>Whether room loading should enable Ceres haze for this callback.</returns>
    public static bool SpawnsCeresHaze(RoomSetupCallback callback) => callback is
        RoomSetupCallback.TurnCeresDoorToSolidBlocksAndSpawnHaze or
        RoomSetupCallback.SpawnCeresHaze or
        RoomSetupCallback.SetCeresRidleyBgCharacterBaseAndSpawnHaze;
}
