namespace SuperMetroid.Core.Enemies;

/// <summary>Population parameter bits shared by several enemy families.</summary>
internal static class EnemyParameterMasks
{
    /// <summary>
    /// Parameter-one bit 15: the Rio, Norfair Rio, Lower Norfair Rio or Norfair lava jumper follows a parent rather than
    /// leading.
    /// </summary>
    internal const ushort FollowerBit = 0x8000;
}
