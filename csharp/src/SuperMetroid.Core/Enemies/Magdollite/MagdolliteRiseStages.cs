namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>What each <see cref="MagdolliteRiseStage"/> shows and which stages it steps to (#2343).</summary>
public static partial class MagdolliteRiseStages
{
    /// <summary>A stage's phase, from <see cref="MagdollitePhaseDefinitions.Phases"/>, and its neighbours in <see cref="MagdollitePhaseDefinitions.RiseOrder"/>.</summary>
    extension(MagdolliteRiseStage stage)
    {
        /// <summary>The stage's distance threshold, body animation and overlay offset.</summary>
        internal MagdollitePhaseDefinition Phase => MagdollitePhaseDefinitions.Phases[stage];

        /// <summary>The next stage up the rise.</summary>
        /// <exception cref="InvalidDataException">This is the rise's last stage.</exception>
        public MagdolliteRiseStage Higher => Beside(stage, 1, "above");

        /// <summary>The next stage down the rise.</summary>
        /// <exception cref="InvalidDataException">This is the setup.</exception>
        public MagdolliteRiseStage Lower => Beside(stage, -1, "below");
    }

    /// <summary>The stage <paramref name="by"/> places from <paramref name="stage"/> in the rise.</summary>
    /// <exception cref="InvalidDataException">The rise has no stage there.</exception>
    private static MagdolliteRiseStage Beside(MagdolliteRiseStage stage, int by, string side)
    {
        int place = Array.IndexOf(MagdollitePhaseDefinitions.RiseOrder, stage) + by;
        return place >= 0 && place < MagdollitePhaseDefinitions.RiseOrder.Length
            ? MagdollitePhaseDefinitions.RiseOrder[place]
            : throw new InvalidDataException($"A Magdollite arm's rise has no stage {side} {stage}.");
    }
}
