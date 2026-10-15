namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>Etecoon's quake-owned parameter bit.</summary>
internal static class EtecoonParameterMasks
{
    /// <summary>Parameter-two bit 15, set over the kept low byte while an earthquake pauses the Etecoon.</summary>
    internal const ushort QuakePausedBit = 0x8000;
}
