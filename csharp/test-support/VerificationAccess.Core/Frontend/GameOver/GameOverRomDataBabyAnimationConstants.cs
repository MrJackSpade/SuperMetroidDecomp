namespace SuperMetroid.Core.Frontend.GameOver;

/// <summary>Cartridge values of <see cref="GameOverRomData.BabyAnimation"/> that only verification reads.</summary>
internal static class GameOverRomDataBabyAnimationConstants
{
    /// <summary>First bank-$8B instruction at $BC27 in the game-over Baby animation.</summary>
    public const ushort FirstInstruction = 0xbc27;

    /// <summary>Provides the first native game-over Baby animation instruction for script verification.</summary>
    extension(GameOverRomData.BabyAnimation)
    {
        /// <inheritdoc cref="GameOverRomDataBabyAnimationConstants.FirstInstruction"/>
        internal static ushort FirstInstruction => GameOverRomDataBabyAnimationConstants.FirstInstruction;
    }
}
