namespace SuperMetroid.Core.Enemies.Drawing;

/// <summary>Cartridge values and helpers of <see cref="CommonEnemyEmptyExtendedFrameToolingDefinitions"/> that only verification reads.</summary>
internal static class CommonEnemyEmptyExtendedFrameToolingDefinitionsConstants
{
    /// <summary>Bank-local extended frame root at $804F.</summary>
    internal const ushort Frame = 0x804f;
    /// <summary>Recognizes the point-hitbox extended frame record only in banks with the shared record.</summary>
    internal static bool HasFrame(byte bank, ushort pointer) =>
        pointer == Frame && CommonEnemyEmptyExtendedFrameToolingDefinitions.IsSupportedBank(bank);
}
