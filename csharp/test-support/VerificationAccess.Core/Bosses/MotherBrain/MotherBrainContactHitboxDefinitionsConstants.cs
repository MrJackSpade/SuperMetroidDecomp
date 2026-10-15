
namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Cartridge values of <see cref="MotherBrainContactHitboxDefinitions"/> that only verification reads.</summary>
internal static class MotherBrainContactHitboxDefinitionsConstants
{
    /// <summary><c>$A9:B427</c>, two rectangles attached to the standing body.</summary>
    public const int BodySourceAddress = 0xa9b427;
    /// <summary><c>$A9:B439</c>, two rectangles attached to the independently moving brain.</summary>
    public const int BrainSourceAddress = 0xa9b439;
    /// <summary><c>$A9:B44B</c>, one rectangle reused by neck joints one through three.</summary>
    public const int NeckSourceAddress = 0xa9b44b;

    /// <summary>Provides native Mother Brain body, brain, and neck contact hitbox source addresses.</summary>
    extension(MotherBrainContactHitboxDefinitions)
    {
        /// <inheritdoc cref="MotherBrainContactHitboxDefinitionsConstants.BodySourceAddress"/>
        internal static int BodySourceAddress => MotherBrainContactHitboxDefinitionsConstants.BodySourceAddress;
        /// <inheritdoc cref="MotherBrainContactHitboxDefinitionsConstants.BrainSourceAddress"/>
        internal static int BrainSourceAddress => MotherBrainContactHitboxDefinitionsConstants.BrainSourceAddress;
        /// <inheritdoc cref="MotherBrainContactHitboxDefinitionsConstants.NeckSourceAddress"/>
        internal static int NeckSourceAddress => MotherBrainContactHitboxDefinitionsConstants.NeckSourceAddress;
    }
}
