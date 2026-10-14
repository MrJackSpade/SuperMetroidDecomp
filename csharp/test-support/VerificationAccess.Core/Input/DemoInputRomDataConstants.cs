namespace SuperMetroid.Core.Input;

/// <summary>Cartridge values of <see cref="DemoInputRomData"/> that only verification reads.</summary>
internal static class DemoInputRomDataConstants
{
    /// <summary>Bank-$91 base used to resolve native attract-demo input streams.</summary>
    public const int BankBase = 0x910000;

    /// <summary>Provides the demo input bank base for resolving native attract-mode controller records.</summary>
    extension(DemoInputRomData)
    {
        /// <inheritdoc cref="DemoInputRomDataConstants.BankBase"/>
        internal static int BankBase => DemoInputRomDataConstants.BankBase;
    }
}
