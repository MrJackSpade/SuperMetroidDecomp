using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Shutters;
using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.World;

/// <summary>The enemies' palette-FX glows and flashes over the species that show them (#1512).</summary>
/// <param name="bombTorizo">The Bomb and Golden Torizo, whose bellies glow.</param>
/// <param name="statue">Tourian's entrance statue, whose sprite statues grey out.</param>
/// <param name="motherBrain">Mother Brain's encounter, whose exploded door shows the escape's shutter flash.</param>
/// <param name="slots">The room's enemy places, whose destroyable shutters show the escape's shutter flash.</param>
internal sealed class EnemyPaletteFxShows(
    BombTorizoBehavior bombTorizo,
    TourianEntranceStatueBehavior statue,
    MotherBrainEncounter motherBrain,
    EnemySlotTable slots) : IEnemyPaletteFxShows
{
    /// <inheritdoc/>
    /// <remarks>
    /// The shutter flash shows on whichever of Mother Brain and the destroyable shutters the room holds, and on nothing once
    /// both shutters are destroyed, as the record's colours are then shown by no draw.
    /// </remarks>
    public void Show(PaletteFxColours colours, PaletteFxFrame record)
    {
        switch (colours)
        {
            case PaletteFxColours.TorizoBelly:
                bombTorizo.ShowBellyGlow(record);
                return;
            case PaletteFxColours.RidleyStatue:
            case PaletteFxColours.PhantoonStatue:
                statue.ShowStatueGreyOut(colours, record);
                return;
            case PaletteFxColours.TourianShutters:
                // The flash recolours Mother Brain's exploded door in her room and the destroyable shutters in the first
                // escape room, whose FX record also starts the flash.
                if (motherBrain.MotherBrain is not null)
                    motherBrain.ShowEscapeShutterFlash(record);
                ShuttersBehavior.ShowEscapeShutterFlash(slots.Slots, record);
                return;
            case PaletteFxColours.TitleLogo:
            case PaletteFxColours.NintendoBootLogo:
            case PaletteFxColours.NintendoCopyright:
            case PaletteFxColours.BabyMetroidTubeLight:
            case PaletteFxColours.TitleScreenDisplays:
            case PaletteFxColours.CutsceneGunshipEngine:
            case PaletteFxColours.CeresNavigationLightsSprite:
            case PaletteFxColours.CeresNavigationLightsBackground:
            case PaletteFxColours.PlanetZebesText:
            case PaletteFxColours.OldMotherBrainBackgroundLights:
            case PaletteFxColours.GunshipGlow:
            case PaletteFxColours.ZoomedOutExplodingZebes:
            case PaletteFxColours.WideZebesExplosionForeground:
            case PaletteFxColours.WideZebesExplosionBackground:
            case PaletteFxColours.ZebesExplosionFinale:
            case PaletteFxColours.SpaceDuringZebesExplosion:
            case PaletteFxColours.ZebesPlanetAfterglow:
            case PaletteFxColours.ExplodingZebesLava:
            case PaletteFxColours.ExplodingZebesCrust:
            case PaletteFxColours.ExplodingZebesGreyClouds:
            case PaletteFxColours.GunshipEmergingFromExplosion:
            case PaletteFxColours.UnusedCinematicFade:
            case PaletteFxColours.HyperBeam:
            case PaletteFxColours.SamusLoadingSuit:
            case PaletteFxColours.PostCreditsSuperMetroidIcon:
            case PaletteFxColours.DraygonStatue:
            case PaletteFxColours.KraidStatue:
            case PaletteFxColours.HeatSuitGlow:
            case PaletteFxColours.WreckedShipGreenLights:
            case PaletteFxColours.LandingSiteLightning:
            case PaletteFxColours.UnusedDarkLightning:
            case PaletteFxColours.BrinstarBlueSpores:
            case PaletteFxColours.BrinstarRedGlow:
            case PaletteFxColours.Beacon:
            case PaletteFxColours.NorfairHeatForeground:
            case PaletteFxColours.NorfairForegroundPalette4:
            case PaletteFxColours.NorfairForegroundPalette5:
            case PaletteFxColours.NorfairForegroundPalette6:
            case PaletteFxColours.MaridiaSandPits:
            case PaletteFxColours.MaridiaSandFalls:
            case PaletteFxColours.MaridiaBackgroundWaterfalls:
            case PaletteFxColours.TourianArkanoidBlocksAndOrbs:
            case PaletteFxColours.TourianBackground:
            case PaletteFxColours.TourianLevel:
            case PaletteFxColours.OldTourianLevel:
            case PaletteFxColours.OldTourianRailings:
            case PaletteFxColours.OldTourianPanels:
            case PaletteFxColours.UpperCrateriaLevel:
            case PaletteFxColours.CrateriaEscapeLightning:
            case PaletteFxColours.CreBlockPixel:
                throw new InvalidOperationException($"Palette-FX colours {colours} name no enemy that shows them.");
            default:
                throw new ArgumentOutOfRangeException(nameof(colours), colours, "Palette-FX colours are a defined group.");
        }
    }
}
