using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Artwork;

/// <summary>The bosses' installed artwork as a whole: its presentation identity and the check that it is complete.</summary>
internal static class BossArtwork
{
    /// <summary>Hashes each boss artwork entry's identity, in a fixed order; absent entries hash as absent.</summary>
    internal static string ContentIdentity(BossArtworkCatalog artwork) =>
        SelectedPresentationHash.Create("boss-bundle-v1", content =>
        {
            content.AppendIdentity("CrocomireMelting", artwork.CrocomireMelting?.ContentIdentity);
            content.AppendIdentity("CrocomireSkeleton", artwork.CrocomireSkeleton?.ContentIdentity);
            content.AppendIdentity("CrocomireBg2Frames", artwork.CrocomireBg2Frames?.ContentIdentity);
            content.AppendIdentity("CrocomireColors", artwork.CrocomireColors?.ContentIdentity);
            content.AppendIdentity("TorizoInstructionVram", artwork.TorizoSwaps?.ContentIdentity);
            content.AppendIdentity("PhantoonBg2Frames", artwork.PhantoonBg2Frames?.ContentIdentity);
            content.AppendIdentity("PhantoonColors", artwork.PhantoonColors?.ContentIdentity);
            content.AppendIdentity("DraygonBg2Frames", artwork.DraygonBg2Frames?.ContentIdentity);
            content.AppendIdentity("DraygonColors", artwork.DraygonColors?.ContentIdentity);
            content.AppendIdentity("KraidBackground", artwork.KraidBackground?.ContentIdentity);
            content.AppendIdentity("KraidColors", artwork.KraidColors?.ContentIdentity);
            content.AppendIdentity("MotherBrainBodyBg2Frames", artwork.MotherBrainBodyBg2Frames?.ContentIdentity);
            content.AppendIdentity("MotherBrainCorpse", artwork.MotherBrainCorpse?.ContentIdentity);
            content.AppendIdentity("MotherBrainEscapeText", artwork.MotherBrainEscapeText?.ContentIdentity);
            content.AppendIdentity("MotherBrainSpecialSprites", artwork.MotherBrainSpecialSprites?.ContentIdentity);
            content.AppendIdentity("MotherBrainDeathColors", artwork.MotherBrainDeathColors?.ContentIdentity);
            content.AppendIdentity("BabyMetroidCutsceneColors", artwork.BabyMetroidCutsceneColors?.ContentIdentity);
            content.AppendIdentity("ShitroidColors", artwork.ShitroidColors?.ContentIdentity);
            content.AppendIdentity("ZebetiteColors", artwork.ZebetiteColors?.ContentIdentity);
            content.AppendIdentity("SporeSpawnColors", artwork.SporeSpawnColors?.ContentIdentity);
            content.AppendIdentity("BotwoonColors", artwork.BotwoonColors?.ContentIdentity);
            content.AppendIdentity("NorfairRidleyColors", artwork.NorfairRidleyColors?.ContentIdentity);
        });

    /// <summary>Returns installed boss artwork, which must carry the Torizo transfer provider the room-entry queue resolves.</summary>
    /// <exception cref="InvalidDataException">The Torizo transfer provider is absent.</exception>
    internal static BossArtworkCatalog Installed(BossArtworkCatalog artwork)
    {
        Ensure.NotNull(artwork);
        if (artwork.TorizoSwaps is null)
            throw new InvalidDataException("Installed boss artwork requires the Torizo transfer provider.");
        return artwork;
    }
}
