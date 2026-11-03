using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Verification.Bosses;

/// <summary>
/// The colours the bosses' installed colour catalogs show once the import has pre-coloured their pictures in them
/// (#1512), written into a verification colour table at the cartridge's colour positions so the checks that compared a
/// catalog's colour copies with the cartridge's compare the import's shades instead: each operation lays the shade a
/// boss's colour state names for it, and fails as the import does when the import made no such shade.
/// </summary>
internal static class ImportedBossLooks
{
    /// <summary>Mother Brain's rainbow colours, as the import's shades show them.</summary>
    extension(MotherBrainRainbowPalettePresentation self)
    {
        /// <summary>Shows rainbow phase <paramref name="frame"/> over <paramref name="cgram"/>.</summary>
        internal void ApplyRainbow(GameplayColours cgram, int frame) => self.ShowImported(cgram, MotherBrainShades.Rainbow(frame));

        /// <summary>Shows her restored colours over <paramref name="cgram"/>.</summary>
        internal void ApplyNormal(GameplayColours cgram) => self.ShowImported(cgram, MotherBrainShades.Restored);

        /// <summary>Shows step <paramref name="frame"/> of the drain to grey over <paramref name="cgram"/>.</summary>
        internal void ApplyToGrey(GameplayColours cgram, int frame) => self.ShowImported(cgram, MotherBrainShades.ToGrey(frame));

        /// <summary>Shows step <paramref name="frame"/> of the revival from grey over <paramref name="cgram"/>.</summary>
        internal void ApplyFromGrey(GameplayColours cgram, int frame) => self.ShowImported(cgram, MotherBrainShades.FromGrey(frame));

        /// <summary>Shows step <paramref name="frame"/> of the fake death's fade to grey over <paramref name="cgram"/>.</summary>
        internal void ApplyFakeDeathToGrey(GameplayColours cgram, int frame) => self.ShowImported(cgram, MotherBrainShades.FakeDeathToGrey(frame));

        /// <summary>Shows step <paramref name="frame"/> of the fake death's revival over <paramref name="cgram"/>.</summary>
        internal void ApplyFakeDeathFromGrey(GameplayColours cgram, int frame) => self.ShowImported(cgram, MotherBrainShades.FakeDeathFromGrey(frame));

        /// <summary>Lays <paramref name="shade"/> over <paramref name="cgram"/>.</summary>
        internal void ShowImported(GameplayColours cgram, PictureShade shade) =>
            Lay(cgram, MotherBrainSheetShades.RainbowBackground(self), MotherBrainSheetShades.RainbowSprites(self), shade);
    }

    /// <summary>Mother Brain's room colours, as the import's shades show them.</summary>
    extension(MotherBrainRoomColorPresentation self)
    {
        /// <summary>Shows step <paramref name="frame"/> of the room's flash over <paramref name="cgram"/>.</summary>
        internal void ApplyFlash(GameplayColours cgram, int frame) => self.ShowImported(cgram, MotherBrainShades.Flash(frame));

        /// <summary>Shows the room's steady grey over <paramref name="cgram"/>.</summary>
        internal void ApplyFinal(GameplayColours cgram) => self.ShowImported(cgram, MotherBrainShades.FinalRoom);

        /// <summary>Shows her phase-two attack and rear-leg colours over <paramref name="cgram"/>.</summary>
        internal void ApplyPhaseTwoInitial(GameplayColours cgram) => self.ShowImported(cgram, MotherBrainShades.PhaseTwo);

        /// <summary>Shows the room entry's glass and tube colours over <paramref name="cgram"/>.</summary>
        internal void ApplyRoomEntry(GameplayColours cgram) => self.ShowImported(cgram, MotherBrainShades.RoomEntry);

        /// <summary>Shows step <paramref name="frame"/> of the room lights' recovery over <paramref name="cgram"/>.</summary>
        internal void ApplyRecoveryLights(GameplayColours cgram, int frame) => self.ShowImported(cgram, MotherBrainShades.RecoveryLights(frame));

        /// <summary>Lays <paramref name="shade"/> over <paramref name="cgram"/>.</summary>
        internal void ShowImported(GameplayColours cgram, PictureShade shade) =>
            Lay(cgram, MotherBrainSheetShades.RoomBackground(self), MotherBrainSheetShades.RoomSprites(self), shade);
    }

    /// <summary>Mother Brain's health colours, as the import's shades show them.</summary>
    extension(MotherBrainHealthPalettePresentation self)
    {
        /// <summary>Shows health band <paramref name="band"/> over <paramref name="cgram"/>.</summary>
        internal void ApplyBand(GameplayColours cgram, int band) =>
            Lay(cgram, MotherBrainSheetShades.HealthBackground(self), MotherBrainSheetShades.HealthSprites(self), MotherBrainShades.Health(band));
    }

    /// <summary>Draygon's colours, as the import's shades show them.</summary>
    extension(DraygonColorCatalog self)
    {
        /// <summary>Shows health band <paramref name="band"/>'s background colours over <paramref name="cgram"/>.</summary>
        internal void ApplyHealthBand(GameplayColours cgram, int band) => self.ShowImported(cgram, DraygonShades.Health(band));

        /// <summary>Shows a hurt update over <paramref name="cgram"/>: his material colours or their white flash, then <paramref name="healthBand"/> unless negative.</summary>
        internal void ApplyHurt(GameplayColours cgram, bool whiteFrame, int healthBand)
        {
            self.ShowImported(cgram, whiteFrame ? DraygonShades.BackgroundWhite : DraygonShades.Background);
            self.ShowImported(cgram, whiteFrame ? DraygonShades.SpriteWhite : DraygonShades.Sprite);
            if (healthBand >= 0)
                self.ShowImported(cgram, DraygonShades.Health(healthBand));
        }

        /// <summary>Shows his opening's colours over <paramref name="cgram"/>.</summary>
        internal void ApplyIntro(GameplayColours cgram) => self.ShowImported(cgram, DraygonShades.Intro);

        /// <summary>Lays <paramref name="shade"/> over <paramref name="cgram"/>.</summary>
        internal void ShowImported(GameplayColours cgram, PictureShade shade)
        {
            var bosses = new BossSourceArtwork { DraygonColors = self };
            Lay(cgram, BossSheetShades.DraygonBackground(bosses), BossSheetShades.OwnRow(bosses, EnemyDefinitionId.DraygonBody, 0), shade);
        }
    }

    /// <summary>Lays <paramref name="shade"/> over <paramref name="cgram"/> from whichever of the import's lists holds it.</summary>
    /// <exception cref="InvalidDataException">The import made no such shade.</exception>
    private static void Lay(GameplayColours cgram, IReadOnlyList<SheetShade> background, IReadOnlyList<SheetShade> sprites, PictureShade shade)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        bool found = false;
        foreach ((IReadOnlyList<SheetShade> made, int offset) in new[] { (background, BossShadeColours.Room), (sprites, BossShadeColours.Sprites) })
        {
            if (made.FirstOrDefault(candidate => candidate.Shade == shade) is not { } laid)
                continue;
            found = true;
            for (int index = 0; index < laid.Inks.Length; index++)
                cgram.AsPaletteColors().SetColor(offset + laid.Inks[index], laid.Colours[index]);
        }
        if (!found)
            throw new InvalidDataException($"The import pre-colours no shade {shade.Key} step {shade.Step}.");
    }
}
