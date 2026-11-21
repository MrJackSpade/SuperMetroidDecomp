namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>What each <see cref="MagdolliteRiseStage"/> shows and which stages it steps to (#2343).</summary>
public static class MagdolliteRiseStages
{
    /// <summary>The rise's stages from its setup to its top, in the order an arm rises through them.</summary>
    private static readonly MagdolliteRiseStage[] RiseOrder =
    [
        MagdolliteRiseStage.Setup, MagdolliteRiseStage.OneSegment, MagdolliteRiseStage.TwoSegments,
        MagdolliteRiseStage.ThreeSegments, MagdolliteRiseStage.FourSegments, MagdolliteRiseStage.FiveSegments,
        MagdolliteRiseStage.SixSegments, MagdolliteRiseStage.SevenSegments, MagdolliteRiseStage.EightSegments,
    ];

    /// <summary>Where <paramref name="stage"/> stands in the rise.</summary>
    private static int PlaceInRise(MagdolliteRiseStage stage)
    {
        int at = Array.IndexOf(RiseOrder, stage);
        return at >= 0 ? at : throw new ArgumentOutOfRangeException(nameof(stage), stage, "Undefined Magdollite rise stage.");
    }

    /// <summary>A stage's phase, from <see cref="MagdollitePhaseDefinitions.Phases"/>, and its neighbours in <see cref="MagdollitePhaseDefinitions.RiseOrder"/>.</summary>
    extension(MagdolliteRiseStage stage)
    {
        /// <summary>The stage's distance threshold, body animation and overlay offset.</summary>
        internal MagdollitePhaseDefinition Phase => MagdollitePhaseDefinitions.Phases[stage];

        /// <summary>The next stage up the rise.</summary>
        /// <exception cref="InvalidDataException">This is the rise's last stage.</exception>
        public MagdolliteRiseStage Higher
        {
            get
            {
                int at = PlaceInRise(stage);
                return at < RiseOrder.Length - 1 ? RiseOrder[at + 1]
                    : throw new InvalidDataException($"A Magdollite arm's rise has no stage above {stage}.");
            }
        }

        /// <summary>The next stage down the rise.</summary>
        /// <exception cref="InvalidDataException">This is the setup.</exception>
        public MagdolliteRiseStage Lower
        {
            get
            {
                int at = PlaceInRise(stage);
                return at > 0 ? RiseOrder[at - 1]
                    : throw new InvalidDataException($"A Magdollite arm's rise has no stage below {stage}.");
            }
        }
    }
}
