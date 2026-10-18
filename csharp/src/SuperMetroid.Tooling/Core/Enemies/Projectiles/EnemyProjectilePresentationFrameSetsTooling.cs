namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Development-tool members of <see cref="EnemyProjectilePresentationFrameSets"/>; never linked by player hosts.</summary>
internal static class EnemyProjectilePresentationFrameSetsTooling
{
    /// <summary>Checks whether a bank-$86 operand names a program frame of the current published artwork catalog.</summary>
    internal static bool Contains(ushort operandAddress) =>
        Array.BinarySearch(EnemyProjectilePresentationFrameSets.Frames, new EnemyProjectilePresentationFrameDefinition(operandAddress, string.Empty), AddressComparer.Instance) >= 0;
    /// <summary>Orders frame definitions by physical bank-$86 operand address for binary search.</summary>
    internal sealed class AddressComparer : IComparer<EnemyProjectilePresentationFrameDefinition>
    {
        /// <summary>Shared stateless address comparer for current-catalog lookups.</summary>
        internal static readonly AddressComparer Instance = new();
        /// <summary>Compares only operand addresses; family names do not affect lookup identity.</summary>
        public int Compare(EnemyProjectilePresentationFrameDefinition left, EnemyProjectilePresentationFrameDefinition right) =>
            left.OperandAddress.CompareTo(right.OperandAddress);
    }
}
