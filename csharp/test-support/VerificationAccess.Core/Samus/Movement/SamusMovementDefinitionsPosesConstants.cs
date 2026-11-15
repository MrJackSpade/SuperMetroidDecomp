namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Cartridge places of Samus's pose tables that only verification reads.</summary>
internal static class SamusMovementDefinitionsPosesConstants
{
    /// <summary><c>$91:B010</c>, one bank-$91 animation-delay-list pointer per pose.</summary>
    public const int AnimationDelayListPointers = 0x91b010;
    /// <summary><c>$91:9EE2</c>, one bank-$91 input-transition-list pointer per pose.</summary>
    public const int TransitionListPointers = 0x919ee2;
}
