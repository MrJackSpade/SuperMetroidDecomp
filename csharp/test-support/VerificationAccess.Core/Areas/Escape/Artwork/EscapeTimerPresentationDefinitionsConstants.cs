using SuperMetroid.Core.Escape.Artwork;

namespace SuperMetroid.Core.Areas.Escape.Artwork;

/// <summary>Cartridge values of <see cref="EscapeTimerPresentationDefinitions"/> that only verification reads.</summary>
internal static class EscapeTimerPresentationDefinitionsConstants
{
    /// <summary>Bank-$80 table at <c>$80:9FD4</c> containing one spritemap pointer per decimal digit.</summary>
    public const int DigitPointerTable = 0x809fd4;

    /// <summary>Provides the native escape timer digit pointer table for display checks.</summary>
    extension(EscapeTimerPresentationDefinitions)
    {
        /// <inheritdoc cref="EscapeTimerPresentationDefinitionsConstants.DigitPointerTable"/>
        internal static int DigitPointerTable => EscapeTimerPresentationDefinitionsConstants.DigitPointerTable;
    }
}
