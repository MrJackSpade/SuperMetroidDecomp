using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Verification access to <see cref="EndingColours"/> members production does not use.</summary>
internal static class EndingColoursAccess
{
    /// <summary>Reads every shade the running ending scene shows, whoever it is authored for.</summary>
    extension(EndingColours self)
    {
        /// <summary>Each shade the running scene shows, in order, before any actor's or scenery's own are chosen from them (#1544).</summary>
        internal PictureShading Shading => new([.. self.shown], []);
    }
}
