using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.AssetExtraction.Bosses.Botwoon.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Draygon;
using SuperMetroid.AssetExtraction.Bosses.Draygon.Artwork;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Phantoon.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork;
using SuperMetroid.AssetExtraction.Bosses.SporeSpawn.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Kraid;
using SuperMetroid.AssetExtraction.Bosses.Kraid.Artwork;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain;

namespace SuperMetroid.AssetExtraction;

/// <summary>The installed content identities of the bosses' colour catalogs, which only the import pipeline and its diagnostics compare (#1502).</summary>
internal static class BossColourContentIdentity
{
    /// <summary>The installed content identity of a <see cref="BotwoonColorCatalog"/>.</summary>
    extension(BotwoonColorCatalog colors)
    {
        /// <summary>Hashes every health band's sixteen colours in band order.</summary>
        public string ContentIdentity =>
            SelectedPresentationHash.Create("BotwoonColorCatalog-v1", content =>
            {
                content.Append("health", BotwoonHealthBandDefinitions.BandCount);
                Span<RgbColor> bandColors = stackalloc RgbColor[BotwoonColorFormat.ColorsPerPalette];
                for (int band = 0; band < BotwoonHealthBandDefinitions.BandCount; band++)
                {
                    for (int color = 0; color < bandColors.Length; color++) bandColors[color] = colors.HealthColor(band, color);
                    content.AppendColors("row", bandColors);
                }
            });
    }

    /// <summary>The installed content identity of a <see cref="CrocomireColorCatalog"/>.</summary>
    extension(CrocomireColorCatalog colors)
    {
        /// <summary>Hashes each complete resolved transfer in the original color order.</summary>
        public string ContentIdentity =>
            SelectedPresentationHash.Create("CrocomireColorCatalog-v1", content =>
            {
                Span<RgbColor> buffer = stackalloc RgbColor[CrocomireColorCatalog.Counts.Values.Max()];
                foreach ((CrocomireColorCatalog.Band band, string label) in CrocomireIdentityOrder)
                {
                    Span<RgbColor> transfer = buffer[..CrocomireColorCatalog.Counts[band]];
                    for (int color = 0; color < transfer.Length; color++) transfer[color] = colors.Get(band, color);
                    content.AppendColors(label, transfer);
                }
            });
    }

    /// <summary>The installed content identity of a <see cref="DraygonColorCatalog"/>.</summary>
    extension(DraygonColorCatalog colors)
    {
        /// <summary>Hashes the opening, background, sprite and flash colours, then every health band's colours.</summary>
        public string ContentIdentity =>
            SelectedPresentationHash.Create("DraygonColorCatalog-v1", content =>
            {
                Span<RgbColor> introWords = stackalloc RgbColor[DraygonColorDefinitions.IntroCount];
                for (int color = 0; color < introWords.Length; color++) introWords[color] = colors.ResolveIntro(color);
                content.AppendColors("intro", introWords);
                Span<RgbColor> backgroundWords = stackalloc RgbColor[DraygonColorDefinitions.BackgroundCount];
                for (int color = 0; color < backgroundWords.Length; color++) backgroundWords[color] = colors.ResolveBackground(color);
                content.AppendColors("background", backgroundWords);
                Span<RgbColor> spriteWords = stackalloc RgbColor[DraygonColorDefinitions.SpriteCount];
                for (int color = 0; color < spriteWords.Length; color++) spriteWords[color] = colors.ResolveSprite(color);
                content.AppendColors("sprite", spriteWords);
                Span<RgbColor> flash = stackalloc RgbColor[DraygonColorDefinitions.WhiteFlashCount];
                for (int color = 0; color < flash.Length; color++) flash[color] = colors.ResolveWhiteFlash(color);
                content.AppendColors("whiteFlash", flash);
                content.Append("healthBands", DraygonColorDefinitions.HealthBandCount);
                Span<RgbColor> bandColors = stackalloc RgbColor[DraygonColorDefinitions.HealthBandColorCount];
                for (int band = 0; band < DraygonColorDefinitions.HealthBandCount; band++)
                {
                    for (int color = 0; color < bandColors.Length; color++) bandColors[color] = colors.ResolveHealthBand(band, color);
                    content.AppendColors("row", bandColors);
                }
            });
    }

    /// <summary>The installed content identity of a <see cref="KraidColorCatalog"/>.</summary>
    extension(KraidColorCatalog colors)
    {
        /// <summary>Hashes the five sources, each tagged by its position: room backdrop, initial target, health, secondary, death arm.</summary>
        public string ContentIdentity =>
            SelectedPresentationHash.Create("enemy-kraid-colors-v1", content =>
            {
                for (int tag = 0; tag < KraidIdentityOrder.Length; tag++)
                {
                    content.Append("source", tag);
                    content.AppendColors("colors", colors.sources[KraidIdentityOrder[tag]].ToArray());
                }
            });
    }

    /// <summary>The installed content identity of a <see cref="BabyMetroidCutsceneColorCatalog"/>.</summary>
    extension(BabyMetroidCutsceneColorCatalog colors)
    {
        /// <summary>Hashes the initial image, then the six resolved fade frames.</summary>
        public string ContentIdentity =>
            SelectedPresentationHash.Create("BabyMetroidCutsceneColorCatalog-v1", content =>
            {
                Span<RgbColor> initial = stackalloc RgbColor[BabyMetroidCutsceneColorFormat.InitialColorCount];
                for (int color = 0; color < initial.Length; color++) initial[color] = colors.InitialColor(color);
                content.AppendColors("initial", initial);
                content.Append("fade", BabyMetroidCutsceneColorDefinitions.FadeFrameCount);
                Span<RgbColor> frameColors = stackalloc RgbColor[BabyMetroidCutsceneColorFormat.FadeColorCount];
                for (int frame = 0; frame < BabyMetroidCutsceneColorDefinitions.FadeFrameCount; frame++)
                {
                    for (int color = 0; color < frameColors.Length; color++) frameColors[color] = colors.FadeColor(frame + 1, color);
                    content.AppendColors("row", frameColors);
                }
            });
    }

    /// <summary>The installed content identity of a <see cref="MotherBrainDeathColorCatalog"/>.</summary>
    extension(MotherBrainDeathColorCatalog colors)
    {
        /// <summary>Hashes the body, leg and corpse fades.</summary>
        public string ContentIdentity =>
            SelectedPresentationHash.Create("MotherBrainDeathColorCatalog-v2", content =>
            {
                AppendFade(content, "bodyFade", MotherBrainDeathColorFile.BodyFadeFrameCount, MotherBrainDeathColorFormat.BodyColorCount, colors.BodyColor);
                AppendFade(content, "legFade", MotherBrainDeathColorFile.BodyFadeFrameCount, MotherBrainDeathColorFormat.BodyColorCount, colors.LegColor);
                AppendFade(content, "corpseFade", MotherBrainDeathColorFile.CorpseFadeFrameCount, MotherBrainDeathColorFormat.CorpseColorCount, colors.CorpseColor);
            });
    }

    /// <summary>The installed content identity of a <see cref="ShitroidColorCatalog"/>.</summary>
    extension(ShitroidColorCatalog colors)
    {
        /// <summary>Hashes the Sidehopper, Shitroid and dead-Sidehopper targets, then the eight normal pulse phases.</summary>
        public string ContentIdentity =>
            SelectedPresentationHash.Create("ShitroidColorCatalog-v1", content =>
            {
                Span<RgbColor> selectedSidehopper = stackalloc RgbColor[ShitroidColorDefinitions.TargetColorCount];
                for (int color = 0; color < selectedSidehopper.Length; color++) selectedSidehopper[color] = colors.TargetColor(ShitroidColorTarget.Sidehopper, color);
                content.AppendColors("sidehopper", selectedSidehopper);
                Span<RgbColor> selectedShitroid = stackalloc RgbColor[ShitroidColorDefinitions.TargetColorCount];
                for (int color = 0; color < selectedShitroid.Length; color++) selectedShitroid[color] = colors.TargetColor(ShitroidColorTarget.Shitroid, color);
                content.AppendColors("shitroid", selectedShitroid);
                Span<RgbColor> selectedCorpse = stackalloc RgbColor[ShitroidColorDefinitions.TargetColorCount];
                for (int color = 0; color < selectedCorpse.Length; color++) selectedCorpse[color] = colors.TargetColor(ShitroidColorTarget.DeadSidehopper, color);
                content.AppendColors("deadSidehopper", selectedCorpse);
                content.Append("normal", ShitroidColorDefinitions.NormalFrameCount);
                for (int frame = 0; frame < ShitroidColorDefinitions.NormalFrameCount; frame++)
                {
                    var phaseColors = new RgbColor[ShitroidColorDefinitions.NormalColorsPerFrame];
                    for (int color = 0; color < phaseColors.Length; color++) phaseColors[color] = colors.NormalColor(frame, color);
                    content.AppendColors("row", phaseColors);
                }
            });
    }

    /// <summary>The installed content identity of a <see cref="PhantoonColorCatalog"/>.</summary>
    extension(PhantoonColorCatalog colors)
    {
        /// <summary>Hashes the fade-out and power-on targets, then every health band.</summary>
        public string ContentIdentity =>
            SelectedPresentationHash.Create("PhantoonColorCatalog-v1", content =>
            {
                Span<RgbColor> target = stackalloc RgbColor[PhantoonColorFormat.FadeOutCount];
                for (int color = 0; color < target.Length; color++) target[color] = colors.ResolveFadeOut(color);
                content.AppendColors("fadeOut", target);
                var power = new List<RgbColor>(PhantoonColorFormat.PoweredParts.Length * PhantoonColorFormat.PoweredPartColourCount);
                foreach (WreckedShipPoweredPart part in PhantoonColorFormat.PoweredParts)
                    for (int color = 0; color < PhantoonColorFormat.PoweredPartColourCount; color++) power.Add(colors.ResolvePowerOn(part, color));
                content.AppendColors("powerOn", power.ToArray());
                var frames = new RgbColor[PhantoonColorDefinitions.HealthBandCount][];
                for (int band = 0; band < frames.Length; band++)
                {
                    frames[band] = new RgbColor[PhantoonColorFormat.HealthBandColorCount];
                    for (int color = 0; color < frames[band].Length; color++) frames[band][color] = colors.ResolveHealth(band, color);
                }
                content.AppendColorFrames("healthBands", frames);
            });
    }

    /// <summary>The installed content identity of a <see cref="NorfairRidleyColorCatalog"/>.</summary>
    extension(NorfairRidleyColorCatalog colors)
    {
        /// <summary>Hashes the initial colours, then the fifteen reveal steps.</summary>
        public string ContentIdentity =>
            SelectedPresentationHash.Create("NorfairRidleyColorCatalog-v1", content =>
            {
                Span<RgbColor> initialWords = stackalloc RgbColor[NorfairRidleyColorFormat.InitialColorCount];
                for (int color = 0; color < initialWords.Length; color++) initialWords[color] = colors.ResolveInitial(color);
                content.AppendColors("initial", initialWords);
                RgbColor[][] revealWords = new RgbColor[NorfairRidleyRevealDefinitions.RevealStepCount][];
                for (int step = 0; step < revealWords.Length; step++)
                {
                    revealWords[step] = new RgbColor[NorfairRidleyColorFormat.RevealColorCount];
                    for (int color = 0; color < revealWords[step].Length; color++) revealWords[step][color] = colors.ResolveReveal(step, color);
                }
                content.AppendColorFrames("reveal", revealWords);
            });
    }

    /// <summary>The installed content identity of a <see cref="SporeSpawnColorCatalog"/>.</summary>
    extension(SporeSpawnColorCatalog colors)
    {
        /// <summary>Hashes the spores, then the health bands and the death-sprite, death-level and death-background fade steps.</summary>
        public string ContentIdentity =>
            SelectedPresentationHash.Create("SporeSpawnColorCatalog-v1", content =>
            {
                content.AppendColors("spores", Enumerable.Range(0, SporeSpawnColorFormat.ColorsPerFrame).Select(colors.ResolveSpore).ToArray());
                content.AppendColorFrames("health", Enumerable.Range(0, SporeSpawnColorDefinitions.HealthFrameCount)
                    .Select(frame => Enumerable.Range(0, SporeSpawnColorFormat.ColorsPerFrame)
                        .Select(color => colors.ResolveHealth(frame, color)).ToArray()).ToArray());
                content.AppendColorFrames("deathSprite", Enumerable.Range(0, SporeSpawnColorFormat.DeathSpriteFrameCount)
                    .Select(frame => Enumerable.Range(0, SporeSpawnColorFormat.ColorsPerFrame)
                        .Select(color => colors.ResolveDeathSprite(frame, color)).ToArray()).ToArray());
                content.AppendColorFrames("deathLevel", Enumerable.Range(0, SporeSpawnColorFormat.DeathSceneFrameCount)
                    .Select(frame => Enumerable.Range(0, SporeSpawnColorFormat.ColorsPerFrame)
                        .Select(color => colors.ResolveDeathLevel(frame, color)).ToArray()).ToArray());
                content.AppendColorFrames("deathBackground", Enumerable.Range(0, SporeSpawnColorFormat.DeathSceneFrameCount)
                    .Select(frame => Enumerable.Range(0, SporeSpawnColorFormat.ColorsPerFrame)
                        .Select(color => colors.ResolveDeathBackground(frame, color)).ToArray()).ToArray());
            });
    }

    /// <summary>The installed content identity of a <see cref="ZebetiteColorCatalog"/>.</summary>
    extension(ZebetiteColorCatalog colors)
    {
        /// <summary>Hashes the eight resolved pulse frames.</summary>
        public string ContentIdentity =>
            SelectedPresentationHash.Create("ZebetiteColorCatalog-v1", content =>
            {
                content.Append("frames", ZebetiteColorFormat.FrameCount);
                Span<RgbColor> frameColors = stackalloc RgbColor[ZebetiteColorFormat.ColorsPerFrame];
                for (int frame = 0; frame < ZebetiteColorFormat.FrameCount; frame++)
                {
                    for (int color = 0; color < frameColors.Length; color++) frameColors[color] = colors.Resolve(frame, color);
                    content.AppendColors("row", frameColors);
                }
            });
    }

    /// <summary>Appends one Mother Brain death fade's installed frames to a content identity, labelled <paramref name="label"/>.</summary>
    private static void AppendFade(SelectedPresentationHash content, string label, int frameCount, int colorCount, Func<int, int, RgbColor> colour)
    {
        content.Append(label, frameCount);
        Span<RgbColor> frameColors = stackalloc RgbColor[colorCount];
        for (int frame = 0; frame < frameCount; frame++)
        {
            for (int color = 0; color < colorCount; color++)
                frameColors[color] = colour(frame, color);
            content.AppendColors("row", frameColors);
        }
    }

    /// <summary>Kraid's colour sources in the order its identity tags them.</summary>
    private static readonly KraidPaletteSource[] KraidIdentityOrder =
    [
        KraidPaletteSource.RoomBackdrop, KraidPaletteSource.InitialTarget, KraidPaletteSource.Health,
        KraidPaletteSource.Secondary, KraidPaletteSource.DeathArm,
    ];

    /// <summary>Crocomire's five transfers in authored order, each with the label its content identity hashes it under.</summary>
    private static readonly CrocomireTransferLabel[] CrocomireIdentityOrder =
    [
        new(CrocomireColorCatalog.Band.FightBody, "fightBody"),
        new(CrocomireColorCatalog.Band.InitialWall, "initialWall"),
        new(CrocomireColorCatalog.Band.InitialProjectile, "initialProjectile"),
        new(CrocomireColorCatalog.Band.SkeletonArm, "skeletonArm"),
        new(CrocomireColorCatalog.Band.WallSpikes, "wallSpikes"),
    ];
}
