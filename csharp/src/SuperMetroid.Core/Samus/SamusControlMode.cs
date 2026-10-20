namespace SuperMetroid.Core.Samus;

/// <summary>The frame handler's installed alpha and beta handlers.</summary>
/// <param name="Alpha">The alpha handler, which Samus commands install.</param>
/// <param name="Beta">
/// The beta handler: as installed, in <see cref="SamusControl.Mode"/>; as the frame dispatches it, in a frame's mode,
/// where the death states and the attract demo stand in for it.
/// </param>
internal readonly record struct SamusControlMode(SamusAlphaHandler Alpha, SamusBetaHandler Beta)
{
    /// <summary>The normal pair that Samus command one installs.</summary>
    internal static SamusControlMode Normal => new(SamusAlphaHandler.Normal, SamusBetaHandler.Normal);
}
