using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Shutters;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.World;

/// <summary>The enemies' colour effect glows and flashes over the species that show them (#1512).</summary>
/// <param name="bombTorizo">The Bomb and Golden Torizo, whose bellies glow.</param>
/// <param name="statue">Tourian's entrance statue, whose sprite statues grey out.</param>
/// <param name="slots">The room's enemy places, whose destroyable shutters show the escape's shutter flash.</param>
internal sealed class EnemyColourEffectShows(
    BombTorizoBehavior bombTorizo,
    TourianEntranceStatueBehavior statue,
    EnemySlotTable slots) : IEnemyColourEffectShows
{
    /// <inheritdoc/>
    /// <remarks>
    /// The shutter flash shows on the destroyable shutters the room holds, and on nothing once both are destroyed, as the
    /// record's colours are then shown by no draw. Mother Brain's exploded door's fragments are pre-coloured in their own
    /// colours and do not show it (#1544).
    /// </remarks>
    public void Show(ColourEffectColours colours, ColourEffectFrame record)
    {
        switch (colours)
        {
            case ColourEffectColours.TorizoBelly:
                bombTorizo.ShowBellyGlow(record);
                return;
            case ColourEffectColours.RidleyStatue:
            case ColourEffectColours.PhantoonStatue:
                statue.ShowStatueGreyOut(colours, record);
                return;
            case ColourEffectColours.TourianShutters:
                // The flash recolours the destroyable shutters in the first escape room, whose FX record also starts it.
                ShuttersBehavior.ShowEscapeShutterFlash(slots.Slots, record);
                return;
            case ColourEffectColours.GunshipGlow:
                // The gunship's glow keeps the gunship whose placement started it and shows on it (#1783).
                throw new InvalidOperationException("The gunship's glow shows on the gunship that started it, never through the room's enemies.");
            case ColourEffectColours.TitleLogo:
            case ColourEffectColours.NintendoCopyright:
            case ColourEffectColours.BabyMetroidTubeLight:
            case ColourEffectColours.TitleScreenDisplays:
            case ColourEffectColours.CutsceneGunshipEngine:
            case ColourEffectColours.CeresNavigationLightsSprite:
            case ColourEffectColours.CeresNavigationLightsBackground:
            case ColourEffectColours.PlanetZebesText:
            case ColourEffectColours.OldMotherBrainBackgroundLights:
            case ColourEffectColours.ZoomedOutExplodingZebes:
            case ColourEffectColours.WideZebesExplosionForeground:
            case ColourEffectColours.WideZebesExplosionBackground:
            case ColourEffectColours.ZebesExplosionFinale:
            case ColourEffectColours.SpaceDuringZebesExplosion:
            case ColourEffectColours.ZebesPlanetAfterglow:
            case ColourEffectColours.ExplodingZebesLava:
            case ColourEffectColours.ExplodingZebesCrust:
            case ColourEffectColours.ExplodingZebesGreyClouds:
            case ColourEffectColours.GunshipEmergingFromExplosion:
            case ColourEffectColours.HyperBeam:
            case ColourEffectColours.SamusLoadingSuit:
            case ColourEffectColours.PostCreditsSuperMetroidIcon:
            case ColourEffectColours.DraygonStatue:
            case ColourEffectColours.KraidStatue:
            case ColourEffectColours.HeatSuitGlow:
            case ColourEffectColours.WreckedShipGreenLights:
            case ColourEffectColours.LandingSiteLightning:
            case ColourEffectColours.BrinstarBlueSpores:
            case ColourEffectColours.BrinstarRedGlow:
            case ColourEffectColours.Beacon:
            case ColourEffectColours.NorfairHeatForeground:
            case ColourEffectColours.NorfairGreenForeground:
            case ColourEffectColours.NorfairPurpleForeground:
            case ColourEffectColours.NorfairDarkForeground:
            case ColourEffectColours.MaridiaSandPits:
            case ColourEffectColours.MaridiaSandFalls:
            case ColourEffectColours.MaridiaBackgroundWaterfalls:
            case ColourEffectColours.TourianArkanoidBlocksAndOrbs:
            case ColourEffectColours.TourianBackground:
            case ColourEffectColours.TourianLevel:
            case ColourEffectColours.OldTourianLevel:
            case ColourEffectColours.OldTourianRailings:
            case ColourEffectColours.OldTourianPanels:
            case ColourEffectColours.UpperCrateriaLevel:
            case ColourEffectColours.CrateriaEscapeLightning:
            case ColourEffectColours.ParlorEscapeSky:
                throw new InvalidOperationException($"Colour effect colours {colours} name no enemy that shows them.");
            default:
                throw new ArgumentOutOfRangeException(nameof(colours), colours, "Colour effect colours are a defined group.");
        }
    }
}
