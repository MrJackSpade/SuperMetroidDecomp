using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Rooms;


namespace SuperMetroid.Core.Hud;

/// <summary>Verification access to <see cref="GameplayMessageId"/> members production does not use.</summary>
internal static class GameplayMessageIdAccess
{
    /// <summary>Converts native message bytes into the closed gameplay catalog with source-aware failure details.</summary>
    extension(GameplayMessageId)
    {
        /// <summary>
        /// Converts a byte read from a translated cartridge owner into the closed retail
        /// message catalog. The diagnostic identifies both the raw ID and the owner that read
        /// it, rather than failing later at an unrelated definition-table access.
        /// </summary>
        internal static GameplayMessageId FromCartridge(byte value, string sourceContext) =>
            GameplayMessageNumbers.Numbers.TryDecode(value, out GameplayMessageId id)
                ? id
                : throw new NotSupportedException($"Gameplay message ${value:X2} from {sourceContext} is not translated.");
    }
}
