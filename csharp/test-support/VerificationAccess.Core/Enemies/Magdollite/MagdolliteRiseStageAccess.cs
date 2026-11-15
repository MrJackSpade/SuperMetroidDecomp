namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>Verification access to <see cref="MagdolliteRiseStage"/>: a stage by its place in the rise, the setup first.</summary>
internal static class MagdolliteRiseStageAccess
{
    /// <summary>Names the cartridge's phase numbers as rise stages.</summary>
    extension(MagdolliteRiseStage)
    {
        /// <summary>The stage <paramref name="place"/> stages up the rise from its setup, the cartridge's phase number.</summary>
        /// <exception cref="InvalidDataException">The rise has no stage there.</exception>
        internal static MagdolliteRiseStage At(int place)
        {
            MagdolliteRiseStage stage = MagdolliteRiseStage.Setup;
            for (int step = 0; step < place; step++)
                stage = stage.Higher;
            return place >= 0 ? stage : MagdolliteRiseStage.Setup.Lower;
        }
    }
}
