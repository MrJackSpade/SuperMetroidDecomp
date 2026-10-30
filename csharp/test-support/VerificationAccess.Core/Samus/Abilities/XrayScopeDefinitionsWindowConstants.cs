

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Cartridge values of <see cref="XrayScopeDefinitions.Window"/> that only verification reads.</summary>
internal static class XrayScopeDefinitionsWindowConstants
{
    /// <summary><c>$91:C9D4</c>, 129 absolute tangent words in unsigned 8.8 format.</summary>
    /// <remarks>The X-ray's cone is drawn from its edge angles (#1468); the table is only range-checked.</remarks>
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
