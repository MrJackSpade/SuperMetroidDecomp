namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>Verification's native view of the escape warning text and timer spritemaps.</summary>
internal static class EscapeCartridgeNative
{
    /// <summary>The warning programs by their bank-$A6 source.</summary>
    extension(EscapeTypewriterDefinitions)
    {
        /// <summary>Native <c>TypewriterText_CeresEscapeTimer</c> program source at <c>$A6:C450</c>.</summary>
        internal static int CeresSourceAddress => 0xa6c450;
        /// <summary>Native <c>TypewriterText_ZebesEscapeTimer</c> program source at <c>$A6:C49C</c>.</summary>
        internal static int ZebesSourceAddress => 0xa6c49c;

        /// <summary>Gets the full 24-bit native source identity for an escape-warning program.</summary>
        internal static int SourceAddress(EscapeTypewriterProgramId id) => id switch
        {
            EscapeTypewriterProgramId.Ceres => 0xa6c450,
            EscapeTypewriterProgramId.Zebes => 0xa6c49c,
            _ => throw new ArgumentOutOfRangeException(nameof(id), id, "No native escape text source exists."),
        };
    }

    /// <summary>The timer spritemaps by bank-$80 address.</summary>
    extension(EscapeTimerPresentationDefinitions)
    {
        /// <summary>Resolves the $80:9FD4 decimal digit selection from consecutive two-object records.</summary>
        internal static ushort DigitSpritemapPointer(int digit) => (uint)digit < 10
            ? (ushort)(0x9fe8 + digit * 12)
            : throw new ArgumentOutOfRangeException(nameof(digit));
        /// <summary>The five-part <c>TIME</c> label spritemap at <c>$80:A060</c>.</summary>
        internal static int LabelSpritemap => 0x80a060;
        /// <summary>The timer renderer inherits its spritemap pointers from bank $80.</summary>
        internal static int SpritemapBank => 0x800000;
    }
}
