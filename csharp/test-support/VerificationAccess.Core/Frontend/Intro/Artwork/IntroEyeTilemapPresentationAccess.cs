using System.Text.Json;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>Verification access to <see cref="IntroEyeTilemapPresentation"/> members production does not use.</summary>
internal static class IntroEyeTilemapPresentationAccess
{
    /// <summary>Exposes <see cref="IntroEyeTilemapPresentation"/> state that only verification reads.</summary>
    extension(IntroEyeTilemapPresentation self)
    {
        /// <summary>Identity of every selected eye rectangle in its compiled blink-selector order.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(IntroEyeTilemapPresentation), content =>
        {
            content.Append("frames", self.frames.Length);
            foreach (BackgroundTile[] frame in self.frames)
                content.AppendTiles("frame", frame);
        });
    }
}
