namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>What each <see cref="MagdolliteRiseStage"/> shows and which stages it steps to (#2343).</summary>
public static class MagdolliteRiseStages
{
    /// <summary>A stage's phase and neighbours, from <see cref="MagdollitePhaseDefinitions.Phases"/>.</summary>
    extension(MagdolliteRiseStage stage)
    {
        /// <summary>The stage's distance threshold, body animation and overlay offset.</summary>
        internal MagdollitePhaseDefinition Phase => MagdollitePhaseDefinitions.Phases[stage];

        /// <summary>The next stage up the rise.</summary>
        /// <exception cref="InvalidDataException">This is the rise's last stage.</exception>
        public MagdolliteRiseStage Higher => MagdollitePhaseDefinitions.Phases[stage].Higher
            ?? throw new InvalidDataException($"A Magdollite arm's rise has no stage above {stage}.");

        /// <summary>The next stage down the rise.</summary>
        /// <exception cref="InvalidDataException">This is the setup.</exception>
        public MagdolliteRiseStage Lower => MagdollitePhaseDefinitions.Phases[stage].Lower
            ?? throw new InvalidDataException($"A Magdollite arm's rise has no stage below {stage}.");
    }
}
