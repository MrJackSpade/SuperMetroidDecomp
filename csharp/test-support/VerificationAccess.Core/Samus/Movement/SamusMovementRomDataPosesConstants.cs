
namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Cartridge values of <see cref="SamusMovementRomData.Poses"/> that only verification reads.</summary>
internal static class SamusMovementRomDataPosesConstants
{
    /// <summary><c>$91:B010</c>, one bank-$91 animation-delay-list pointer per pose.</summary>
    public const int AnimationDelayListPointers = 0x91b010;
    /// <summary><c>$91:9EE2</c>, one bank-$91 input-transition-list pointer per pose.</summary>
    public const int TransitionListPointers = 0x919ee2;

    /// <summary>Provides Samus pose animation-delay and transition-list pointer tables.</summary>
    extension(SamusMovementRomData.Poses)
    {
        /// <inheritdoc cref="SamusMovementRomDataPosesConstants.AnimationDelayListPointers"/>
        internal static int AnimationDelayListPointers => SamusMovementRomDataPosesConstants.AnimationDelayListPointers;
        /// <inheritdoc cref="SamusMovementRomDataPosesConstants.TransitionListPointers"/>
        internal static int TransitionListPointers => SamusMovementRomDataPosesConstants.TransitionListPointers;
    }
}
