namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Shared bank-$84 pre-instruction addresses.</summary>
internal static class PlmPreInstructionDefinitions
{
    /// <summary><c>$84:84E6</c>, the RTS every PLM spawn installs as its pre-instruction.</summary>
    internal const ushort SpawnDefault = 0x84e6;
}
