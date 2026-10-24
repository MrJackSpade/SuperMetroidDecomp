using SuperMetroid.Core.Samus.Movement;


namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Cartridge values of <see cref="XrayScopeDefinitions.Window"/> that only verification reads.</summary>
internal static class XrayScopeDefinitionsWindowConstants
{
    /// <summary><c>$91:C9D4</c>, 129 absolute tangent words in unsigned 8.8 format.</summary>
    /// <remarks>
    /// Physical alias of <see cref="AbsoluteTangentDefinitions.Sample"/>.
    /// The exact bounded stock algorithm and all 129-word parity proof are
    /// documented there. Investigation: #625 / #906.
    /// </remarks>
    public const int AbsoluteTangentTable = 0x91c9d4;
    /// <summary>Number of words in the inclusive quarter-turn tangent table.</summary>
    public const int AbsoluteTangentWordCount = 129;

    /// <summary>Provides the native absolute tangent table and length used to verify the X-ray window.</summary>
    extension(XrayScopeDefinitions.Window)
    {
        /// <inheritdoc cref="XrayScopeDefinitionsWindowConstants.AbsoluteTangentTable"/>
        internal static int AbsoluteTangentTable => XrayScopeDefinitionsWindowConstants.AbsoluteTangentTable;
        /// <inheritdoc cref="XrayScopeDefinitionsWindowConstants.AbsoluteTangentWordCount"/>
        internal static int AbsoluteTangentWordCount => XrayScopeDefinitionsWindowConstants.AbsoluteTangentWordCount;
    }
}
