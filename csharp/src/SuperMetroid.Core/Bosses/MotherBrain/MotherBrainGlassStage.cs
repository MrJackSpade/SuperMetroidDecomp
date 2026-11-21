namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// One stage of Mother Brain's glass breaking (<see cref="MotherBrainGlassStageDefinitions"/>), each stage leading to the
/// one it breaks into (#2553): a stage that throws no shards always breaks into another, so the last stage bursts.
/// </summary>
internal abstract partial record MotherBrainGlassStage
{
    /// <summary>Allows only the stages below.</summary>
    /// <param name="image">The glass the stage shows, redrawn on every update the stage holds.</param>
    /// <param name="breaksAtHits">The hits the glass must have taken for the stage to break.</param>
    private MotherBrainGlassStage(MotherBrainGlassDraw image, int breaksAtHits)
    {
        Image = image;
        BreaksAtHits = breaksAtHits;
    }

    /// <summary>The glass the stage shows, redrawn on every update the stage holds.</summary>
    internal MotherBrainGlassDraw Image { get; }

    /// <summary>The hits the glass must have taken for the stage to break.</summary>
    internal int BreaksAtHits { get; }

    /// <summary>A stage that throws no shards: it breaks straight into <see cref="Next"/>.</summary>
    internal sealed record Cracks : MotherBrainGlassStage
    {
        /// <summary>Creates a stage that breaks straight into <paramref name="next"/>.</summary>
        /// <param name="image">The glass the stage shows.</param>
        /// <param name="breaksAtHits">The hits the glass must have taken for the stage to break.</param>
        /// <param name="next">The stage it breaks into.</param>
        internal Cracks(MotherBrainGlassDraw image, int breaksAtHits, MotherBrainGlassStage next)
            : base(image, breaksAtHits) => Next = Ensure.NotNull(next);

        /// <summary>The stage it breaks into.</summary>
        internal MotherBrainGlassStage Next { get; }
    }

    /// <summary>A stage that throws <see cref="Burst"/> as it breaks, around the image that follows it.</summary>
    internal abstract record Bursts : MotherBrainGlassStage
    {
        /// <summary>Allows only the bursting stages below.</summary>
        /// <param name="image">The glass the stage shows.</param>
        /// <param name="breaksAtHits">The hits the glass must have taken for the stage to break.</param>
        /// <param name="burst">The shards the stage throws as it breaks.</param>
        private Bursts(MotherBrainGlassDraw image, int breaksAtHits, MotherBrainGlassBurst burst)
            : base(image, breaksAtHits) => Burst = Ensure.NotNull(burst);

        /// <summary>The shards the stage throws as it breaks.</summary>
        internal MotherBrainGlassBurst Burst { get; }

        /// <summary>A bursting stage that breaks into <see cref="Next"/>, its shards thrown around the next stage's image.</summary>
        internal sealed record Into : Bursts
        {
            /// <summary>Creates a bursting stage that breaks into <paramref name="next"/>.</summary>
            /// <param name="image">The glass the stage shows.</param>
            /// <param name="breaksAtHits">The hits the glass must have taken for the stage to break.</param>
            /// <param name="burst">The shards the stage throws as it breaks.</param>
            /// <param name="next">The stage it breaks into.</param>
            internal Into(MotherBrainGlassDraw image, int breaksAtHits, MotherBrainGlassBurst burst, MotherBrainGlassStage next)
                : base(image, breaksAtHits, burst) => Next = Ensure.NotNull(next);

            /// <summary>The stage it breaks into.</summary>
            internal MotherBrainGlassStage Next { get; }
        }

        /// <summary>
        /// The last stage: its shards are thrown around the shattered glass
        /// (<see cref="MotherBrainGlassStageDefinitions.ShatteredImage"/>), which then shows before the destroyed event.
        /// </summary>
        internal sealed record Last : Bursts
        {
            /// <summary>Creates the last stage.</summary>
            /// <param name="image">The glass the stage shows.</param>
            /// <param name="breaksAtHits">The hits the glass must have taken for the stage to break.</param>
            /// <param name="burst">The shards the stage throws as it breaks.</param>
            internal Last(MotherBrainGlassDraw image, int breaksAtHits, MotherBrainGlassBurst burst)
                : base(image, breaksAtHits, burst)
            {
            }
        }
    }
}
