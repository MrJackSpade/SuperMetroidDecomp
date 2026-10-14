using SuperMetroid.Core.Frontend;

/// <summary>Cartridge values of <see cref="IntroMotherBrainExplosionInstructionDefinitions"/> that only verification reads.</summary>
internal static class IntroMotherBrainExplosionInstructionDefinitionsConstants
{
    /// <summary>One native six-frame large loop occupies 52 frames including blank hold.</summary>
    public const int BigLoopFrames = 52;
    /// <summary>One native six-frame small loop occupies 34 frames including blank hold.</summary>
    public const int SmallLoopFrames = 34;

    /// <summary>Provides the large and small intro Mother Brain explosion loop lengths for animation checks.</summary>
    extension(IntroMotherBrainExplosionInstructionDefinitions)
    {
        /// <inheritdoc cref="IntroMotherBrainExplosionInstructionDefinitionsConstants.BigLoopFrames"/>
        internal static int BigLoopFrames => IntroMotherBrainExplosionInstructionDefinitionsConstants.BigLoopFrames;
        /// <inheritdoc cref="IntroMotherBrainExplosionInstructionDefinitionsConstants.SmallLoopFrames"/>
        internal static int SmallLoopFrames => IntroMotherBrainExplosionInstructionDefinitionsConstants.SmallLoopFrames;
    }
}
