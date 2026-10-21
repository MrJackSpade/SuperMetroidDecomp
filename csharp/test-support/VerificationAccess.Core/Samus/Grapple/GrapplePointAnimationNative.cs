namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Verification's native view of the grapple endpoint animation: the bank-$9A tiles each frame shows.</summary>
internal static class GrapplePointAnimationNative
{
    /// <summary>Bank $9A, which holds the grapple character graphics.</summary>
    internal const int CharacterDataBank = 0x9a0000;

    /// <summary>The endpoint frames by their bank-$9A tile source.</summary>
    extension(GrapplePointAnimationDefinitions)
    {
        /// <summary>$9B:C342: the bank-$9A tile source of <paramref name="frame"/>, from $9A:8200 in $0200-byte frames within the bank.</summary>
        internal static int SourceAddress(int frame) => 0x9a0000 | unchecked((ushort)(0x8200 + frame * 0x200));
    }
}
