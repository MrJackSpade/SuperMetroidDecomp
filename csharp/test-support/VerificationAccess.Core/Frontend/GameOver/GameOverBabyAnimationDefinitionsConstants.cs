using SuperMetroid.AssetExtraction.Frontend;
namespace SuperMetroid.Core.Frontend.GameOver;

/// <summary>Cartridge values of <see cref="GameOverBabyAnimationDefinitions"/> that only verification reads.</summary>
internal static class GameOverBabyAnimationDefinitionsConstants
{
    /// <summary>Sixty six-byte frame records plus three two-byte callbacks.</summary>
    public const int InstructionCount = (2 + 4 + 3) * 4 + 3 * 8;

    /// <summary>Provides the expected game-over Baby animation instruction count.</summary>
    extension(GameOverBabyAnimationDefinitions)
    {
        /// <inheritdoc cref="GameOverBabyAnimationDefinitionsConstants.InstructionCount"/>
        internal static int InstructionCount => GameOverBabyAnimationDefinitionsConstants.InstructionCount;
    }
}
