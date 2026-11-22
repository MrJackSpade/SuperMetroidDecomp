using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>Reads what Samus's arm cannon cover shows, from the cover her drawing reads.</summary>
internal static class SamusArmCannonStateAccess2
{
    /// <summary>Reads the cover.</summary>
    extension(SamusArmCannonState self)
    {
        /// <summary>The cover picture shown, or null while the cover is closed.</summary>
        internal SamusArmCannonCoverFrame? Frame => self.Cover.Shown;

        /// <summary>Whether the cover is open or opening.</summary>
        internal bool IsOpen => self.Cover.OpenOrOpening;

        /// <summary>Whether an opening or closing is still under way.</summary>
        internal bool IsTransitioning => self.Cover is SamusArmCannonCover.Opening or SamusArmCannonCover.Closing;
    }
}
