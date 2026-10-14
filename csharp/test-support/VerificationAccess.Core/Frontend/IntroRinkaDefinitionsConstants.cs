using SuperMetroid.Core.Frontend;

/// <summary>Cartridge values of <see cref="IntroRinkaDefinitions"/> that only verification reads.</summary>
internal static class IntroRinkaDefinitionsConstants
{
    /// <summary><c>$8B:B8B5</c>, four Rinka initial X positions.</summary>
    public const int InitialXReferenceAddress = 0x8bb8b5;
    /// <summary><c>$8B:B8BD</c>, four Rinka initial Y positions before the fixed eight-pixel subtraction.</summary>
    public const int InitialYReferenceAddress = 0x8bb8bd;
    /// <summary>Bank containing the native actor definitions and physical initializer tables.</summary>
    public const int NativeBank = 0x8b0000;
    /// <summary>Number of parameter-selected Rinka initializer rows at <c>$8B:B8B5</c>.</summary>
    public const int RinkaCount = 4;
    /// <summary><c>$8B:B985</c>, four signed whole-pixel X velocity components.</summary>
    public const int XWholeVelocityReferenceAddress = 0x8bb985;

    /// <summary>Provides intro Rinka count, initial positions, velocity, and definition bank for actor checks.</summary>
    extension(IntroRinkaDefinitions)
    {
        /// <inheritdoc cref="IntroRinkaDefinitionsConstants.InitialXReferenceAddress"/>
        internal static int InitialXReferenceAddress => IntroRinkaDefinitionsConstants.InitialXReferenceAddress;
        /// <inheritdoc cref="IntroRinkaDefinitionsConstants.InitialYReferenceAddress"/>
        internal static int InitialYReferenceAddress => IntroRinkaDefinitionsConstants.InitialYReferenceAddress;
        /// <inheritdoc cref="IntroRinkaDefinitionsConstants.NativeBank"/>
        internal static int NativeBank => IntroRinkaDefinitionsConstants.NativeBank;
        /// <inheritdoc cref="IntroRinkaDefinitionsConstants.RinkaCount"/>
        internal static int RinkaCount => IntroRinkaDefinitionsConstants.RinkaCount;
        /// <inheritdoc cref="IntroRinkaDefinitionsConstants.XWholeVelocityReferenceAddress"/>
        internal static int XWholeVelocityReferenceAddress => IntroRinkaDefinitionsConstants.XWholeVelocityReferenceAddress;
    }
}
