
namespace SuperMetroid.Core.Hud;

/// <summary>Verification access to <see cref="GameplayMessageIds"/> members production does not use.</summary>
internal static class GameplayMessageIdsAccess
{
    /// <summary>Converts native message bytes into the closed gameplay catalog with source-aware failure details.</summary>
    extension(GameplayMessageIds)
    {
        /// <summary>
        /// Converts a byte read from a translated cartridge owner into the closed retail
        /// message catalog. The diagnostic identifies both the raw ID and the owner that read
        /// it, rather than failing later at an unrelated definition-table access.
        /// </summary>
        internal static GameplayMessageId FromCartridge(byte value, string sourceContext)
        {
            if (
                value is (< (byte)GameplayMessageId.EnergyTank or > (byte)GameplayMessageId.GravitySuit) and
                not ((byte)GameplayMessageId.GunshipSaveConfirmation))
            {
                throw new NotSupportedException(
                    $"Gameplay message ${value:X2} from {sourceContext} is not translated.");
            }

            return (GameplayMessageId)value;
        }
    }
}
