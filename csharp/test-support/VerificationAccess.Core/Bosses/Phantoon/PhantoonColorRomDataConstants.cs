
namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Cartridge values of <see cref="PhantoonColorRomData"/> that only verification reads.</summary>
internal static class PhantoonColorRomDataConstants
{
    /// <summary>Palette destination used when Phantoon's colors are first powered on.</summary>
    public const int PowerOnDestination = 0;

    /// <summary>Provides the power-on palette destination for Phantoon color data verification.</summary>
    extension(PhantoonColorRomData)
    {
        /// <inheritdoc cref="PhantoonColorRomDataConstants.PowerOnDestination"/>
        internal static int PowerOnDestination => PhantoonColorRomDataConstants.PowerOnDestination;
    }
}
