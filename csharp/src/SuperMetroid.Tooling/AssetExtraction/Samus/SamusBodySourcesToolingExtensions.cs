namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Development-tool members of <see cref="SamusBodySources"/>, which audits and checks read (#1585).</summary>
internal static class SamusBodySourcesToolingExtensions
{
    /// <summary>The half pictures a pose's animation frame selects, for audits and checks.</summary>
    extension(SamusBodySources self)
    {
        /// <summary>The half pictures <paramref name="pose"/>'s <paramref name="animationFrame"/> selects.</summary>
        /// <exception cref="InvalidDataException">The pose has no record, or the frame is not installed.</exception>
        internal SamusBodyFramePictures Pictures(SuperMetroid.Core.Samus.Poses.SamusPoseId pose, int animationFrame) =>
            self.TryPictures(pose, animationFrame, out SamusBodyFramePictures pictures)
                ? pictures
                : throw new InvalidDataException($"Samus pose {pose} animation frame {animationFrame} selects no installed body pictures.");
    }
}
