using System.Collections.Frozen;
using SuperMetroid.Core.Progression;
using L = SuperMetroid.Core.Samus.Projectiles.ProjectileTrailPlacementList;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Where projectile trails start: the family each trail selector gives a beam combination, the list each family gives a
/// direction-table index, and each list's placements by animation frame.
/// </summary>
internal static class ProjectileTrailPlacementDefinitions
{
    /// <summary>
    /// The family each selector row names. Uncharged and charged rows twelve to fifteen read the following table's first
    /// rows. Null is a row that names no family: an empty row, whose direction table lies in direct-page scratch, or a row
    /// past the special beam attack table's ten, which reads list data whose words name no trail list.
    /// </summary>
    internal static readonly FrozenDictionary<ProjectileTrailSelectorRow, ProjectileTrailFamily?> Families =
        new Dictionary<ProjectileTrailSelectorRow, ProjectileTrailFamily?>
        {
            [new(ProjectileTrailFamilyGroup.Uncharged, SamusBeamCombination.Power)] = ProjectileTrailFamily.UnchargedDefault,
            [new(ProjectileTrailFamilyGroup.Uncharged, SamusBeamCombination.Wave)] = ProjectileTrailFamily.UnchargedWave,
            [new(ProjectileTrailFamilyGroup.Uncharged, SamusBeamCombination.Ice)] = ProjectileTrailFamily.UnchargedDefault,
            [new(ProjectileTrailFamilyGroup.Uncharged, SamusBeamCombination.IceWave)] = ProjectileTrailFamily.UnchargedWave,
            [new(ProjectileTrailFamilyGroup.Uncharged, SamusBeamCombination.Spazer)] = ProjectileTrailFamily.UnchargedDefault,
            [new(ProjectileTrailFamilyGroup.Uncharged, SamusBeamCombination.SpazerWave)] = ProjectileTrailFamily.UnchargedDefault,
            [new(ProjectileTrailFamilyGroup.Uncharged, SamusBeamCombination.SpazerIce)] = ProjectileTrailFamily.UnchargedIceSpazer,
            [new(ProjectileTrailFamilyGroup.Uncharged, SamusBeamCombination.SpazerIceWave)] = ProjectileTrailFamily.UnchargedWaveIceSpazer,
            [new(ProjectileTrailFamilyGroup.Uncharged, SamusBeamCombination.Plasma)] = ProjectileTrailFamily.UnchargedDefault,
            [new(ProjectileTrailFamilyGroup.Uncharged, SamusBeamCombination.PlasmaWave)] = ProjectileTrailFamily.UnchargedDefault,
            [new(ProjectileTrailFamilyGroup.Uncharged, SamusBeamCombination.PlasmaIce)] = ProjectileTrailFamily.UnchargedIcePlasma,
            [new(ProjectileTrailFamilyGroup.Uncharged, SamusBeamCombination.PlasmaIceWave)] = ProjectileTrailFamily.UnchargedWaveIcePlasma,
            [new(ProjectileTrailFamilyGroup.Uncharged, SamusBeamCombination.SpazerPlasma)] = ProjectileTrailFamily.ChargedDefault,
            [new(ProjectileTrailFamilyGroup.Uncharged, SamusBeamCombination.SpazerPlasmaWave)] = ProjectileTrailFamily.ChargedWave,
            [new(ProjectileTrailFamilyGroup.Uncharged, SamusBeamCombination.SpazerPlasmaIce)] = ProjectileTrailFamily.ChargedDefault,
            [new(ProjectileTrailFamilyGroup.Uncharged, SamusBeamCombination.SpazerPlasmaIceWave)] = ProjectileTrailFamily.ChargedWave,
            [new(ProjectileTrailFamilyGroup.Charged, SamusBeamCombination.Power)] = ProjectileTrailFamily.ChargedDefault,
            [new(ProjectileTrailFamilyGroup.Charged, SamusBeamCombination.Wave)] = ProjectileTrailFamily.ChargedWave,
            [new(ProjectileTrailFamilyGroup.Charged, SamusBeamCombination.Ice)] = ProjectileTrailFamily.ChargedDefault,
            [new(ProjectileTrailFamilyGroup.Charged, SamusBeamCombination.IceWave)] = ProjectileTrailFamily.ChargedWave,
            [new(ProjectileTrailFamilyGroup.Charged, SamusBeamCombination.Spazer)] = ProjectileTrailFamily.ChargedDefault,
            [new(ProjectileTrailFamilyGroup.Charged, SamusBeamCombination.SpazerWave)] = ProjectileTrailFamily.ChargedDefault,
            [new(ProjectileTrailFamilyGroup.Charged, SamusBeamCombination.SpazerIce)] = ProjectileTrailFamily.ChargedIceSpazer,
            [new(ProjectileTrailFamilyGroup.Charged, SamusBeamCombination.SpazerIceWave)] = ProjectileTrailFamily.ChargedWaveIceSpazer,
            [new(ProjectileTrailFamilyGroup.Charged, SamusBeamCombination.Plasma)] = ProjectileTrailFamily.ChargedDefault,
            [new(ProjectileTrailFamilyGroup.Charged, SamusBeamCombination.PlasmaWave)] = ProjectileTrailFamily.ChargedDefault,
            [new(ProjectileTrailFamilyGroup.Charged, SamusBeamCombination.PlasmaIce)] = ProjectileTrailFamily.ChargedIcePlasma,
            [new(ProjectileTrailFamilyGroup.Charged, SamusBeamCombination.PlasmaIceWave)] = ProjectileTrailFamily.ChargedWaveIcePlasma,
            [new(ProjectileTrailFamilyGroup.Charged, SamusBeamCombination.SpazerPlasma)] = null,
            [new(ProjectileTrailFamilyGroup.Charged, SamusBeamCombination.SpazerPlasmaWave)] = null,
            [new(ProjectileTrailFamilyGroup.Charged, SamusBeamCombination.SpazerPlasmaIce)] = null,
            [new(ProjectileTrailFamilyGroup.Charged, SamusBeamCombination.SpazerPlasmaIceWave)] = null,
            [new(ProjectileTrailFamilyGroup.SpecialBeamAttack, SamusBeamCombination.Power)] = null,
            [new(ProjectileTrailFamilyGroup.SpecialBeamAttack, SamusBeamCombination.Wave)] = null,
            [new(ProjectileTrailFamilyGroup.SpecialBeamAttack, SamusBeamCombination.Ice)] = null,
            [new(ProjectileTrailFamilyGroup.SpecialBeamAttack, SamusBeamCombination.IceWave)] = null,
            [new(ProjectileTrailFamilyGroup.SpecialBeamAttack, SamusBeamCombination.Spazer)] = ProjectileTrailFamily.SbaSpazer,
            [new(ProjectileTrailFamilyGroup.SpecialBeamAttack, SamusBeamCombination.SpazerWave)] = ProjectileTrailFamily.SbaWaveSpazer,
            [new(ProjectileTrailFamilyGroup.SpecialBeamAttack, SamusBeamCombination.SpazerIce)] = ProjectileTrailFamily.SbaSpazer,
            [new(ProjectileTrailFamilyGroup.SpecialBeamAttack, SamusBeamCombination.SpazerIceWave)] = null,
            [new(ProjectileTrailFamilyGroup.SpecialBeamAttack, SamusBeamCombination.Plasma)] = null,
            [new(ProjectileTrailFamilyGroup.SpecialBeamAttack, SamusBeamCombination.PlasmaWave)] = null,
            [new(ProjectileTrailFamilyGroup.SpecialBeamAttack, SamusBeamCombination.PlasmaIce)] = null,
            [new(ProjectileTrailFamilyGroup.SpecialBeamAttack, SamusBeamCombination.PlasmaIceWave)] = null,
            [new(ProjectileTrailFamilyGroup.SpecialBeamAttack, SamusBeamCombination.SpazerPlasma)] = null,
            [new(ProjectileTrailFamilyGroup.SpecialBeamAttack, SamusBeamCombination.SpazerPlasmaWave)] = null,
            [new(ProjectileTrailFamilyGroup.SpecialBeamAttack, SamusBeamCombination.SpazerPlasmaIce)] = null,
            [new(ProjectileTrailFamilyGroup.SpecialBeamAttack, SamusBeamCombination.SpazerPlasmaIceWave)] = null,
        }.ToFrozenDictionary();

    /// <summary>
    /// The list each family gives the sixteen direction-table indexes. Indexes ten to fifteen read the following table;
    /// null is an entry that names no trail list.
    /// </summary>
    internal static readonly FrozenDictionary<ProjectileTrailFamily, ProjectileTrailPlacementList?[]> Lists =
        new Dictionary<ProjectileTrailFamily, ProjectileTrailPlacementList?[]>
        {
            [ProjectileTrailFamily.UnchargedDefault] = [L.UnchargedDefault, L.UnchargedDefault, L.UnchargedDefault, L.UnchargedDefault, L.UnchargedDefault, L.UnchargedDefault, L.UnchargedDefault, L.UnchargedDefault, L.UnchargedDefault, L.UnchargedDefault, L.UnchargedIceSpazerUp, L.UnchargedIceSpazerUpRight, L.UnchargedIceSpazerRight, L.UnchargedIceSpazerDownRight, L.UnchargedIceSpazerDown, L.UnchargedIceSpazerDown],
            [ProjectileTrailFamily.UnchargedWave] = [L.UnchargedWaveVertical, L.UnchargedWaveRisingDiagonal, L.UnchargedWaveHorizontal, L.UnchargedWaveFallingDiagonal, L.UnchargedWaveVertical, L.UnchargedWaveVertical, L.UnchargedWaveRisingDiagonal, L.UnchargedWaveHorizontal, L.UnchargedWaveFallingDiagonal, L.UnchargedWaveVertical, L.UnchargedDefault, L.UnchargedDefault, L.UnchargedDefault, L.UnchargedDefault, L.UnchargedDefault, L.UnchargedDefault],
            [ProjectileTrailFamily.UnchargedIceSpazer] = [L.UnchargedIceSpazerUp, L.UnchargedIceSpazerUpRight, L.UnchargedIceSpazerRight, L.UnchargedIceSpazerDownRight, L.UnchargedIceSpazerDown, L.UnchargedIceSpazerDown, L.UnchargedIceSpazerDownLeft, L.UnchargedIceSpazerLeft, L.UnchargedIceSpazerUpLeft, L.UnchargedIceSpazerUp, L.UnchargedWaveIceSpazerUp, L.UnchargedWaveIceSpazerUpRight, L.UnchargedWaveIceSpazerRight, L.UnchargedWaveIceSpazerDownRight, L.UnchargedWaveIceSpazerDown, L.UnchargedWaveIceSpazerDown],
            [ProjectileTrailFamily.UnchargedWaveIceSpazer] = [L.UnchargedWaveIceSpazerUp, L.UnchargedWaveIceSpazerUpRight, L.UnchargedWaveIceSpazerRight, L.UnchargedWaveIceSpazerDownRight, L.UnchargedWaveIceSpazerDown, L.UnchargedWaveIceSpazerDown, L.UnchargedWaveIceSpazerDownLeft, L.UnchargedWaveIceSpazerLeft, L.UnchargedWaveIceSpazerUpLeft, L.UnchargedWaveIceSpazerUp, L.UnchargedIcePlasmaUp, L.UnchargedIcePlasmaUpRight, L.UnchargedIcePlasmaRight, L.UnchargedIcePlasmaDownRight, L.UnchargedIcePlasmaDown, L.UnchargedIcePlasmaDown],
            [ProjectileTrailFamily.UnchargedIcePlasma] = [L.UnchargedIcePlasmaUp, L.UnchargedIcePlasmaUpRight, L.UnchargedIcePlasmaRight, L.UnchargedIcePlasmaDownRight, L.UnchargedIcePlasmaDown, L.UnchargedIcePlasmaDown, L.UnchargedIcePlasmaDownLeft, L.UnchargedIcePlasmaLeft, L.UnchargedIcePlasmaUpLeft, L.UnchargedIcePlasmaUp, L.UnchargedWaveIcePlasmaUp, L.UnchargedWaveIcePlasmaUpRight, L.UnchargedWaveIcePlasmaRight, L.UnchargedWaveIcePlasmaDownRight, L.UnchargedWaveIcePlasmaDown, L.UnchargedWaveIcePlasmaDown],
            [ProjectileTrailFamily.UnchargedWaveIcePlasma] = [L.UnchargedWaveIcePlasmaUp, L.UnchargedWaveIcePlasmaUpRight, L.UnchargedWaveIcePlasmaRight, L.UnchargedWaveIcePlasmaDownRight, L.UnchargedWaveIcePlasmaDown, L.UnchargedWaveIcePlasmaDown, L.UnchargedWaveIcePlasmaDownLeft, L.UnchargedWaveIcePlasmaLeft, L.UnchargedWaveIcePlasmaUpLeft, L.UnchargedWaveIcePlasmaUp, null, null, null, null, null, null],
            [ProjectileTrailFamily.ChargedDefault] = [L.ChargedDefault, L.ChargedDefault, L.ChargedDefault, L.ChargedDefault, L.ChargedDefault, L.ChargedDefault, L.ChargedDefault, L.ChargedDefault, L.ChargedDefault, L.ChargedDefault, L.ChargedWaveVertical, L.ChargedWaveRisingDiagonal, L.ChargedWaveHorizontal, L.ChargedWaveFallingDiagonal, L.ChargedWaveVertical, L.ChargedWaveVertical],
            [ProjectileTrailFamily.ChargedWave] = [L.ChargedWaveVertical, L.ChargedWaveRisingDiagonal, L.ChargedWaveHorizontal, L.ChargedWaveFallingDiagonal, L.ChargedWaveVertical, L.ChargedWaveVertical, L.ChargedWaveRisingDiagonal, L.ChargedWaveHorizontal, L.ChargedWaveFallingDiagonal, L.ChargedWaveVertical, L.ChargedIceSpazerUp, L.ChargedIceSpazerUpRight, L.ChargedIceSpazerRight, L.ChargedIceSpazerDownRight, L.ChargedIceSpazerDown, L.ChargedIceSpazerDown],
            [ProjectileTrailFamily.ChargedIceSpazer] = [L.ChargedIceSpazerUp, L.ChargedIceSpazerUpRight, L.ChargedIceSpazerRight, L.ChargedIceSpazerDownRight, L.ChargedIceSpazerDown, L.ChargedIceSpazerDown, L.ChargedIceSpazerDownLeft, L.ChargedIceSpazerLeft, L.ChargedIceSpazerUpLeft, L.ChargedIceSpazerUp, L.ChargedWaveIceSpazerUp, L.ChargedWaveIceSpazerUpRight, L.ChargedWaveIceSpazerRight, L.ChargedWaveIceSpazerDownRight, L.ChargedWaveIceSpazerDown, L.ChargedWaveIceSpazerDown],
            [ProjectileTrailFamily.ChargedWaveIceSpazer] = [L.ChargedWaveIceSpazerUp, L.ChargedWaveIceSpazerUpRight, L.ChargedWaveIceSpazerRight, L.ChargedWaveIceSpazerDownRight, L.ChargedWaveIceSpazerDown, L.ChargedWaveIceSpazerDown, L.ChargedWaveIceSpazerDownLeft, L.ChargedWaveIceSpazerLeft, L.ChargedWaveIceSpazerUpLeft, L.ChargedWaveIceSpazerUp, L.ChargedIcePlasmaUp, L.ChargedIcePlasmaUpRight, L.ChargedIcePlasmaRight, L.ChargedIcePlasmaDownRight, L.ChargedIcePlasmaDown, L.ChargedIcePlasmaDown],
            [ProjectileTrailFamily.ChargedIcePlasma] = [L.ChargedIcePlasmaUp, L.ChargedIcePlasmaUpRight, L.ChargedIcePlasmaRight, L.ChargedIcePlasmaDownRight, L.ChargedIcePlasmaDown, L.ChargedIcePlasmaDown, L.ChargedIcePlasmaDownLeft, L.ChargedIcePlasmaLeft, L.ChargedIcePlasmaUpLeft, L.ChargedIcePlasmaUp, L.ChargedWaveIcePlasmaUp, L.ChargedWaveIcePlasmaUpRight, L.ChargedWaveIcePlasmaRight, L.ChargedWaveIcePlasmaDownRight, L.ChargedWaveIcePlasmaDown, L.ChargedWaveIcePlasmaDown],
            [ProjectileTrailFamily.ChargedWaveIcePlasma] = [L.ChargedWaveIcePlasmaUp, L.ChargedWaveIcePlasmaUpRight, L.ChargedWaveIcePlasmaRight, L.ChargedWaveIcePlasmaDownRight, L.ChargedWaveIcePlasmaDown, L.ChargedWaveIcePlasmaDown, L.ChargedWaveIcePlasmaDownLeft, L.ChargedWaveIcePlasmaLeft, L.ChargedWaveIcePlasmaUpLeft, L.ChargedWaveIcePlasmaUp, null, null, null, null, null, null],
            [ProjectileTrailFamily.SbaWaveSpazer] = [L.SbaWaveSpazerVertical, L.SbaWaveSpazerRisingDiagonal, L.SbaWaveSpazerHorizontal, L.SbaWaveSpazerFallingDiagonal, L.SbaWaveSpazerVertical, L.SbaWaveSpazerVertical, L.SbaWaveSpazerRisingDiagonal, L.SbaWaveSpazerHorizontal, L.SbaWaveSpazerFallingDiagonal, L.SbaWaveSpazerVertical, null, null, null, null, null, null],
            [ProjectileTrailFamily.SbaSpazer] = [L.SbaSpazer, L.SbaSpazer, L.SbaSpazer, L.SbaSpazer, L.SbaSpazerDown, L.SbaSpazerDown, L.SbaSpazer, L.SbaSpazer, L.SbaSpazer, L.SbaSpazer, null, null, null, null, null, null],
        }.ToFrozenDictionary();

    /// <summary>
    /// The placements each list gives an animation frame: its own records, then the records stored after it, up to the
    /// next pointer table or the end of the trail data.
    /// </summary>
    internal static readonly FrozenDictionary<ProjectileTrailPlacementList, ProjectileTrailPlacement[]> Records =
        new Dictionary<ProjectileTrailPlacementList, ProjectileTrailPlacement[]>
        {
            [L.UnchargedDefault] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)),
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)),
                new(new(0, 0), new(0, 0)), new(new(8, 0), new(0, 0)), new(new(12, 0), new(0, 0)), new(new(16, 0), new(0, 0)),
                new(new(20, 0), new(0, 0)), new(new(16, 0), new(0, 0)), new(new(12, 0), new(0, 0)), new(new(8, 0), new(0, 0)),
                new(new(0, 0), new(0, 0)), new(new(-8, 0), new(0, 0)), new(new(-12, 0), new(0, 0)), new(new(-16, 0), new(0, 0)),
                new(new(-20, 0), new(0, 0)), new(new(-16, 0), new(0, 0)),
            ],
            [L.UnchargedWaveVertical] =
            [
                new(new(0, 0), new(0, 0)), new(new(8, 0), new(0, 0)), new(new(12, 0), new(0, 0)), new(new(16, 0), new(0, 0)),
                new(new(20, 0), new(0, 0)), new(new(16, 0), new(0, 0)), new(new(12, 0), new(0, 0)), new(new(8, 0), new(0, 0)),
                new(new(0, 0), new(0, 0)), new(new(-8, 0), new(0, 0)), new(new(-12, 0), new(0, 0)), new(new(-16, 0), new(0, 0)),
                new(new(-20, 0), new(0, 0)), new(new(-16, 0), new(0, 0)), new(new(-12, 0), new(0, 0)), new(new(-8, 0), new(0, 0)),
                new(new(0, 0), new(0, 0)), new(new(0, -8), new(0, 0)), new(new(0, -12), new(0, 0)), new(new(0, -16), new(0, 0)),
                new(new(0, -20), new(0, 0)), new(new(0, -16), new(0, 0)),
            ],
            [L.UnchargedWaveHorizontal] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, -8), new(0, 0)), new(new(0, -12), new(0, 0)), new(new(0, -16), new(0, 0)),
                new(new(0, -20), new(0, 0)), new(new(0, -16), new(0, 0)), new(new(0, -12), new(0, 0)), new(new(0, -8), new(0, 0)),
                new(new(0, 0), new(0, 0)), new(new(0, 8), new(0, 0)), new(new(0, 12), new(0, 0)), new(new(0, 16), new(0, 0)),
                new(new(0, 20), new(0, 0)), new(new(0, 16), new(0, 0)), new(new(0, 12), new(0, 0)), new(new(0, 8), new(0, 0)),
                new(new(0, 0), new(0, 0)), new(new(-4, -4), new(0, 0)), new(new(-8, -8), new(0, 0)), new(new(-10, -10), new(0, 0)),
                new(new(-12, -12), new(0, 0)), new(new(-10, -10), new(0, 0)),
            ],
            [L.UnchargedWaveRisingDiagonal] =
            [
                new(new(0, 0), new(0, 0)), new(new(-4, -4), new(0, 0)), new(new(-8, -8), new(0, 0)), new(new(-10, -10), new(0, 0)),
                new(new(-12, -12), new(0, 0)), new(new(-10, -10), new(0, 0)), new(new(-8, -8), new(0, 0)), new(new(-4, -4), new(0, 0)),
                new(new(0, 0), new(0, 0)), new(new(4, 4), new(0, 0)), new(new(8, 8), new(0, 0)), new(new(10, 10), new(0, 0)),
                new(new(12, 12), new(0, 0)), new(new(10, 10), new(0, 0)), new(new(8, 8), new(0, 0)), new(new(4, 4), new(0, 0)),
                new(new(0, 0), new(0, 0)), new(new(4, -4), new(0, 0)), new(new(8, -8), new(0, 0)), new(new(10, -10), new(0, 0)),
                new(new(12, -12), new(0, 0)), new(new(10, -10), new(0, 0)),
            ],
            [L.UnchargedWaveFallingDiagonal] =
            [
                new(new(0, 0), new(0, 0)), new(new(4, -4), new(0, 0)), new(new(8, -8), new(0, 0)), new(new(10, -10), new(0, 0)),
                new(new(12, -12), new(0, 0)), new(new(10, -10), new(0, 0)), new(new(8, -8), new(0, 0)), new(new(4, -4), new(0, 0)),
                new(new(0, 0), new(0, 0)), new(new(-4, 4), new(0, 0)), new(new(-8, 8), new(0, 0)), new(new(-10, 10), new(0, 0)),
                new(new(-12, 12), new(0, 0)), new(new(-10, 10), new(0, 0)), new(new(-8, 8), new(0, 0)), new(new(-4, 4), new(0, 0)),
                new(new(0, 0), new(0, 0)), new(new(-8, 8), new(8, 8)), new(new(-16, 8), new(16, 8)), new(new(0, 0), new(0, 0)),
                new(new(-8, -8), new(8, -8)), new(new(-16, -8), new(16, -8)),
            ],
            [L.UnchargedIceSpazerUp] =
            [
                new(new(0, 0), new(0, 0)), new(new(-8, 8), new(8, 8)), new(new(-16, 8), new(16, 8)), new(new(0, 0), new(0, 0)),
                new(new(-8, -8), new(8, -8)), new(new(-16, -8), new(16, -8)), new(new(-8, 8), new(-8, 8)), new(new(-14, 2), new(-2, 14)),
                new(new(-20, -4), new(2, 20)), new(new(-8, 0), new(-8, 0)), new(new(-8, -8), new(-8, 8)), new(new(-8, -16), new(-8, 16)),
                new(new(-8, -8), new(-8, -8)), new(new(-2, -16), new(-16, -2)), new(new(4, -20), new(-20, 4)), new(new(8, -8), new(8, -8)),
                new(new(14, -2), new(2, -14)), new(new(20, 4), new(-2, -20)), new(new(8, 0), new(8, 0)), new(new(8, 8), new(8, -8)),
                new(new(8, 16), new(8, -16)), new(new(8, 8), new(8, 8)),
            ],
            [L.UnchargedIceSpazerDown] =
            [
                new(new(0, 0), new(0, 0)), new(new(-8, -8), new(8, -8)), new(new(-16, -8), new(16, -8)), new(new(-8, 8), new(-8, 8)),
                new(new(-14, 2), new(-2, 14)), new(new(-20, -4), new(2, 20)), new(new(-8, 0), new(-8, 0)), new(new(-8, -8), new(-8, 8)),
                new(new(-8, -16), new(-8, 16)), new(new(-8, -8), new(-8, -8)), new(new(-2, -16), new(-16, -2)), new(new(4, -20), new(-20, 4)),
                new(new(8, -8), new(8, -8)), new(new(14, -2), new(2, -14)), new(new(20, 4), new(-2, -20)), new(new(8, 0), new(8, 0)),
                new(new(8, 8), new(8, -8)), new(new(8, 16), new(8, -16)), new(new(8, 8), new(8, 8)), new(new(2, 16), new(16, 2)),
                new(new(-4, 20), new(20, -4)), new(new(0, 0), new(0, 0)),
            ],
            [L.UnchargedIceSpazerUpRight] =
            [
                new(new(-8, 8), new(-8, 8)), new(new(-14, 2), new(-2, 14)), new(new(-20, -4), new(2, 20)), new(new(-8, 0), new(-8, 0)),
                new(new(-8, -8), new(-8, 8)), new(new(-8, -16), new(-8, 16)), new(new(-8, -8), new(-8, -8)), new(new(-2, -16), new(-16, -2)),
                new(new(4, -20), new(-20, 4)), new(new(8, -8), new(8, -8)), new(new(14, -2), new(2, -14)), new(new(20, 4), new(-2, -20)),
                new(new(8, 0), new(8, 0)), new(new(8, 8), new(8, -8)), new(new(8, 16), new(8, -16)), new(new(8, 8), new(8, 8)),
                new(new(2, 16), new(16, 2)), new(new(-4, 20), new(20, -4)), new(new(0, 0), new(0, 0)), new(new(-4, 8), new(4, 8)),
                new(new(-8, 8), new(8, 8)), new(new(-12, 8), new(12, 8)),
            ],
            [L.UnchargedIceSpazerRight] =
            [
                new(new(-8, 0), new(-8, 0)), new(new(-8, -8), new(-8, 8)), new(new(-8, -16), new(-8, 16)), new(new(-8, -8), new(-8, -8)),
                new(new(-2, -16), new(-16, -2)), new(new(4, -20), new(-20, 4)), new(new(8, -8), new(8, -8)), new(new(14, -2), new(2, -14)),
                new(new(20, 4), new(-2, -20)), new(new(8, 0), new(8, 0)), new(new(8, 8), new(8, -8)), new(new(8, 16), new(8, -16)),
                new(new(8, 8), new(8, 8)), new(new(2, 16), new(16, 2)), new(new(-4, 20), new(20, -4)), new(new(0, 0), new(0, 0)),
                new(new(-4, 8), new(4, 8)), new(new(-8, 8), new(8, 8)), new(new(-12, 8), new(12, 8)), new(new(-16, 8), new(16, 8)),
                new(new(-16, 8), new(16, 8)), new(new(-16, 8), new(16, 8)),
            ],
            [L.UnchargedIceSpazerDownRight] =
            [
                new(new(-8, -8), new(-8, -8)), new(new(-2, -16), new(-16, -2)), new(new(4, -20), new(-20, 4)), new(new(8, -8), new(8, -8)),
                new(new(14, -2), new(2, -14)), new(new(20, 4), new(-2, -20)), new(new(8, 0), new(8, 0)), new(new(8, 8), new(8, -8)),
                new(new(8, 16), new(8, -16)), new(new(8, 8), new(8, 8)), new(new(2, 16), new(16, 2)), new(new(-4, 20), new(20, -4)),
                new(new(0, 0), new(0, 0)), new(new(-4, 8), new(4, 8)), new(new(-8, 8), new(8, 8)), new(new(-12, 8), new(12, 8)),
                new(new(-16, 8), new(16, 8)), new(new(-16, 8), new(16, 8)), new(new(-16, 8), new(16, 8)), new(new(-12, 8), new(12, 8)),
                new(new(-8, 8), new(8, 8)), new(new(-4, 8), new(4, 8)),
            ],
            [L.UnchargedIceSpazerDownLeft] =
            [
                new(new(8, -8), new(8, -8)), new(new(14, -2), new(2, -14)), new(new(20, 4), new(-2, -20)), new(new(8, 0), new(8, 0)),
                new(new(8, 8), new(8, -8)), new(new(8, 16), new(8, -16)), new(new(8, 8), new(8, 8)), new(new(2, 16), new(16, 2)),
                new(new(-4, 20), new(20, -4)), new(new(0, 0), new(0, 0)), new(new(-4, 8), new(4, 8)), new(new(-8, 8), new(8, 8)),
                new(new(-12, 8), new(12, 8)), new(new(-16, 8), new(16, 8)), new(new(-16, 8), new(16, 8)), new(new(-16, 8), new(16, 8)),
                new(new(-12, 8), new(12, 8)), new(new(-8, 8), new(8, 8)), new(new(-4, 8), new(4, 8)), new(new(0, 0), new(0, 0)),
                new(new(-12, 6), new(-6, 12)), new(new(-14, 2), new(-2, 14)),
            ],
            [L.UnchargedIceSpazerLeft] =
            [
                new(new(8, 0), new(8, 0)), new(new(8, 8), new(8, -8)), new(new(8, 16), new(8, -16)), new(new(8, 8), new(8, 8)),
                new(new(2, 16), new(16, 2)), new(new(-4, 20), new(20, -4)), new(new(0, 0), new(0, 0)), new(new(-4, 8), new(4, 8)),
                new(new(-8, 8), new(8, 8)), new(new(-12, 8), new(12, 8)), new(new(-16, 8), new(16, 8)), new(new(-16, 8), new(16, 8)),
                new(new(-16, 8), new(16, 8)), new(new(-12, 8), new(12, 8)), new(new(-8, 8), new(8, 8)), new(new(-4, 8), new(4, 8)),
                new(new(0, 0), new(0, 0)), new(new(-12, 6), new(-6, 12)), new(new(-14, 2), new(-2, 14)), new(new(-16, 0), new(0, 16)),
                new(new(-18, -2), new(2, 18)), new(new(-20, -4), new(2, 20)),
            ],
            [L.UnchargedIceSpazerUpLeft] =
            [
                new(new(8, 8), new(8, 8)), new(new(2, 16), new(16, 2)), new(new(-4, 20), new(20, -4)), new(new(0, 0), new(0, 0)),
                new(new(-4, 8), new(4, 8)), new(new(-8, 8), new(8, 8)), new(new(-12, 8), new(12, 8)), new(new(-16, 8), new(16, 8)),
                new(new(-16, 8), new(16, 8)), new(new(-16, 8), new(16, 8)), new(new(-12, 8), new(12, 8)), new(new(-8, 8), new(8, 8)),
                new(new(-4, 8), new(4, 8)), new(new(0, 0), new(0, 0)), new(new(-12, 6), new(-6, 12)), new(new(-14, 2), new(-2, 14)),
                new(new(-16, 0), new(0, 16)), new(new(-18, -2), new(2, 18)), new(new(-20, -4), new(2, 20)), new(new(-18, -2), new(2, 18)),
                new(new(-16, 0), new(0, 16)), new(new(-14, 2), new(-2, 14)),
            ],
            [L.UnchargedWaveIceSpazerUp] =
            [
                new(new(0, 0), new(0, 0)), new(new(-4, 8), new(4, 8)), new(new(-8, 8), new(8, 8)), new(new(-12, 8), new(12, 8)),
                new(new(-16, 8), new(16, 8)), new(new(-16, 8), new(16, 8)), new(new(-16, 8), new(16, 8)), new(new(-12, 8), new(12, 8)),
                new(new(-8, 8), new(8, 8)), new(new(-4, 8), new(4, 8)), new(new(0, 0), new(0, 0)), new(new(-12, 6), new(-6, 12)),
                new(new(-14, 2), new(-2, 14)), new(new(-16, 0), new(0, 16)), new(new(-18, -2), new(2, 18)), new(new(-20, -4), new(2, 20)),
                new(new(-18, -2), new(2, 18)), new(new(-16, 0), new(0, 16)), new(new(-14, 2), new(-2, 14)), new(new(-12, 6), new(-6, 12)),
                new(new(0, 0), new(0, 0)), new(new(-8, -4), new(-8, 4)),
            ],
            [L.UnchargedWaveIceSpazerUpRight] =
            [
                new(new(0, 0), new(0, 0)), new(new(-12, 6), new(-6, 12)), new(new(-14, 2), new(-2, 14)), new(new(-16, 0), new(0, 16)),
                new(new(-18, -2), new(2, 18)), new(new(-20, -4), new(2, 20)), new(new(-18, -2), new(2, 18)), new(new(-16, 0), new(0, 16)),
                new(new(-14, 2), new(-2, 14)), new(new(-12, 6), new(-6, 12)), new(new(0, 0), new(0, 0)), new(new(-8, -4), new(-8, 4)),
                new(new(-8, -8), new(-8, 8)), new(new(-8, -12), new(-8, 12)), new(new(-8, -16), new(-8, 16)), new(new(-8, -16), new(-8, 16)),
                new(new(-8, -16), new(-8, 16)), new(new(-8, -12), new(-8, 12)), new(new(-8, -8), new(-8, 8)), new(new(-8, -4), new(-8, 4)),
                new(new(0, 0), new(0, 0)), new(new(-12, -6), new(-6, -12)),
            ],
            [L.UnchargedWaveIceSpazerRight] =
            [
                new(new(0, 0), new(0, 0)), new(new(-8, -4), new(-8, 4)), new(new(-8, -8), new(-8, 8)), new(new(-8, -12), new(-8, 12)),
                new(new(-8, -16), new(-8, 16)), new(new(-8, -16), new(-8, 16)), new(new(-8, -16), new(-8, 16)), new(new(-8, -12), new(-8, 12)),
                new(new(-8, -8), new(-8, 8)), new(new(-8, -4), new(-8, 4)), new(new(0, 0), new(0, 0)), new(new(-12, -6), new(-6, -12)),
                new(new(-2, -16), new(-16, -2)), new(new(-16, 0), new(0, -16)), new(new(-18, 2), new(2, -18)), new(new(4, -20), new(-20, 4)),
                new(new(-18, 2), new(2, -18)), new(new(-16, 0), new(0, -16)), new(new(-2, -16), new(-16, -2)), new(new(-12, -6), new(-6, -12)),
                new(new(0, 0), new(0, 0)), new(new(-4, -8), new(4, -8)),
            ],
            [L.UnchargedWaveIceSpazerDownRight] =
            [
                new(new(0, 0), new(0, 0)), new(new(-12, -6), new(-6, -12)), new(new(-2, -16), new(-16, -2)), new(new(-16, 0), new(0, -16)),
                new(new(-18, 2), new(2, -18)), new(new(4, -20), new(-20, 4)), new(new(-18, 2), new(2, -18)), new(new(-16, 0), new(0, -16)),
                new(new(-2, -16), new(-16, -2)), new(new(-12, -6), new(-6, -12)), new(new(0, 0), new(0, 0)), new(new(-4, -8), new(4, -8)),
                new(new(-8, -8), new(8, -8)), new(new(-12, -8), new(12, -8)), new(new(-16, -8), new(16, -8)), new(new(-16, -8), new(16, -8)),
                new(new(-16, -8), new(16, -8)), new(new(-12, -8), new(12, -8)), new(new(-8, -8), new(8, -8)), new(new(-4, -8), new(4, -8)),
                new(new(0, 0), new(0, 0)), new(new(2, -14), new(14, -2)),
            ],
            [L.UnchargedWaveIceSpazerDown] =
            [
                new(new(0, 0), new(0, 0)), new(new(-4, -8), new(4, -8)), new(new(-8, -8), new(8, -8)), new(new(-12, -8), new(12, -8)),
                new(new(-16, -8), new(16, -8)), new(new(-16, -8), new(16, -8)), new(new(-16, -8), new(16, -8)), new(new(-12, -8), new(12, -8)),
                new(new(-8, -8), new(8, -8)), new(new(-4, -8), new(4, -8)), new(new(0, 0), new(0, 0)), new(new(2, -14), new(14, -2)),
                new(new(0, -16), new(16, 0)), new(new(-2, -18), new(18, 2)), new(new(-2, -20), new(20, 4)), new(new(-2, -20), new(20, 4)),
                new(new(-2, -20), new(20, 4)), new(new(-2, -18), new(18, 2)), new(new(0, -16), new(16, 0)), new(new(2, -14), new(14, -2)),
                new(new(0, 0), new(0, 0)), new(new(8, -4), new(8, 4)),
            ],
            [L.UnchargedWaveIceSpazerDownLeft] =
            [
                new(new(0, 0), new(0, 0)), new(new(2, -14), new(14, -2)), new(new(0, -16), new(16, 0)), new(new(-2, -18), new(18, 2)),
                new(new(-2, -20), new(20, 4)), new(new(-2, -20), new(20, 4)), new(new(-2, -20), new(20, 4)), new(new(-2, -18), new(18, 2)),
                new(new(0, -16), new(16, 0)), new(new(2, -14), new(14, -2)), new(new(0, 0), new(0, 0)), new(new(8, -4), new(8, 4)),
                new(new(8, -8), new(8, 8)), new(new(8, -12), new(8, 12)), new(new(8, -16), new(8, 16)), new(new(8, -16), new(8, 16)),
                new(new(8, -16), new(8, 16)), new(new(8, -12), new(8, 12)), new(new(8, -8), new(8, 8)), new(new(8, -4), new(8, 4)),
                new(new(0, 0), new(0, 0)), new(new(6, 10), new(10, 6)),
            ],
            [L.UnchargedWaveIceSpazerLeft] =
            [
                new(new(0, 0), new(0, 0)), new(new(8, -4), new(8, 4)), new(new(8, -8), new(8, 8)), new(new(8, -12), new(8, 12)),
                new(new(8, -16), new(8, 16)), new(new(8, -16), new(8, 16)), new(new(8, -16), new(8, 16)), new(new(8, -12), new(8, 12)),
                new(new(8, -8), new(8, 8)), new(new(8, -4), new(8, 4)), new(new(0, 0), new(0, 0)), new(new(6, 10), new(10, 6)),
                new(new(2, 16), new(16, 2)), new(new(0, 16), new(16, 0)), new(new(-2, 18), new(18, -2)), new(new(-4, 20), new(20, -4)),
                new(new(-2, 18), new(18, -2)), new(new(0, 16), new(16, 0)), new(new(2, 16), new(16, 2)), new(new(6, 10), new(10, 6)),
                new(new(0, 0), new(0, 0)), new(new(0, 16), new(0, 16)),
            ],
            [L.UnchargedWaveIceSpazerUpLeft] =
            [
                new(new(0, 0), new(0, 0)), new(new(6, 10), new(10, 6)), new(new(2, 16), new(16, 2)), new(new(0, 16), new(16, 0)),
                new(new(-2, 18), new(18, -2)), new(new(-4, 20), new(20, -4)), new(new(-2, 18), new(18, -2)), new(new(0, 16), new(16, 0)),
                new(new(2, 16), new(16, 2)), new(new(6, 10), new(10, 6)), new(new(0, 0), new(0, 0)), new(new(0, 16), new(0, 16)),
                new(new(0, 0), new(0, 0)), new(new(-12, 12), new(-12, 12)), new(new(0, 0), new(0, 0)), new(new(-16, 0), new(-16, 0)),
                new(new(0, 0), new(0, 0)), new(new(-12, -12), new(-12, -12)), new(new(0, 0), new(0, 0)), new(new(0, -16), new(0, -16)),
                new(new(0, 0), new(0, 0)), new(new(12, -12), new(12, -12)),
            ],
            [L.UnchargedIcePlasmaUp] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, 16), new(0, 16)), new(new(0, 0), new(0, 0)), new(new(-12, 12), new(-12, 12)),
                new(new(0, 0), new(0, 0)), new(new(-16, 0), new(-16, 0)), new(new(0, 0), new(0, 0)), new(new(-12, -12), new(-12, -12)),
                new(new(0, 0), new(0, 0)), new(new(0, -16), new(0, -16)), new(new(0, 0), new(0, 0)), new(new(12, -12), new(12, -12)),
                new(new(0, 0), new(0, 0)), new(new(16, 0), new(16, 0)), new(new(0, 0), new(0, 0)), new(new(12, 12), new(12, 12)),
                new(new(0, 0), new(0, 0)), new(new(0, 16), new(0, 16)), new(new(-8, 16), new(8, 16)), new(new(-16, 16), new(16, 16)),
                new(new(-16, 16), new(16, 16)), new(new(-16, 16), new(16, 16)),
            ],
            [L.UnchargedIcePlasmaUpRight] =
            [
                new(new(0, 0), new(0, 0)), new(new(-12, 12), new(-12, 12)), new(new(0, 0), new(0, 0)), new(new(-16, 0), new(-16, 0)),
                new(new(0, 0), new(0, 0)), new(new(-12, -12), new(-12, -12)), new(new(0, 0), new(0, 0)), new(new(0, -16), new(0, -16)),
                new(new(0, 0), new(0, 0)), new(new(12, -12), new(12, -12)), new(new(0, 0), new(0, 0)), new(new(16, 0), new(16, 0)),
                new(new(0, 0), new(0, 0)), new(new(12, 12), new(12, 12)), new(new(0, 0), new(0, 0)), new(new(0, 16), new(0, 16)),
                new(new(-8, 16), new(8, 16)), new(new(-16, 16), new(16, 16)), new(new(-16, 16), new(16, 16)), new(new(-16, 16), new(16, 16)),
                new(new(-16, 16), new(16, 16)), new(new(-16, 16), new(16, 16)),
            ],
            [L.UnchargedIcePlasmaRight] =
            [
                new(new(0, 0), new(0, 0)), new(new(-16, 0), new(-16, 0)), new(new(0, 0), new(0, 0)), new(new(-12, -12), new(-12, -12)),
                new(new(0, 0), new(0, 0)), new(new(0, -16), new(0, -16)), new(new(0, 0), new(0, 0)), new(new(12, -12), new(12, -12)),
                new(new(0, 0), new(0, 0)), new(new(16, 0), new(16, 0)), new(new(0, 0), new(0, 0)), new(new(12, 12), new(12, 12)),
                new(new(0, 0), new(0, 0)), new(new(0, 16), new(0, 16)), new(new(-8, 16), new(8, 16)), new(new(-16, 16), new(16, 16)),
                new(new(-16, 16), new(16, 16)), new(new(-16, 16), new(16, 16)), new(new(-16, 16), new(16, 16)), new(new(-16, 16), new(16, 16)),
                new(new(-8, 16), new(8, 16)), new(new(0, 0), new(0, 0)),
            ],
            [L.UnchargedIcePlasmaDownRight] =
            [
                new(new(0, 0), new(0, 0)), new(new(-12, -12), new(-12, -12)), new(new(0, 0), new(0, 0)), new(new(0, -16), new(0, -16)),
                new(new(0, 0), new(0, 0)), new(new(12, -12), new(12, -12)), new(new(0, 0), new(0, 0)), new(new(16, 0), new(16, 0)),
                new(new(0, 0), new(0, 0)), new(new(12, 12), new(12, 12)), new(new(0, 0), new(0, 0)), new(new(0, 16), new(0, 16)),
                new(new(-8, 16), new(8, 16)), new(new(-16, 16), new(16, 16)), new(new(-16, 16), new(16, 16)), new(new(-16, 16), new(16, 16)),
                new(new(-16, 16), new(16, 16)), new(new(-16, 16), new(16, 16)), new(new(-8, 16), new(8, 16)), new(new(0, 0), new(0, 0)),
                new(new(-12, 12), new(-12, 12)), new(new(-20, 8), new(-8, 18)),
            ],
            [L.UnchargedIcePlasmaDown] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, -16), new(0, -16)), new(new(0, 0), new(0, 0)), new(new(12, -12), new(12, -12)),
                new(new(0, 0), new(0, 0)), new(new(16, 0), new(16, 0)), new(new(0, 0), new(0, 0)), new(new(12, 12), new(12, 12)),
                new(new(0, 0), new(0, 0)), new(new(0, 16), new(0, 16)), new(new(-8, 16), new(8, 16)), new(new(-16, 16), new(16, 16)),
                new(new(-16, 16), new(16, 16)), new(new(-16, 16), new(16, 16)), new(new(-16, 16), new(16, 16)), new(new(-16, 16), new(16, 16)),
                new(new(-8, 16), new(8, 16)), new(new(0, 0), new(0, 0)), new(new(-12, 12), new(-12, 12)), new(new(-20, 8), new(-8, 18)),
                new(new(-24, 2), new(-2, 20)), new(new(-24, 0), new(0, 24)),
            ],
            [L.UnchargedIcePlasmaDownLeft] =
            [
                new(new(0, 0), new(0, 0)), new(new(12, -12), new(12, -12)), new(new(0, 0), new(0, 0)), new(new(16, 0), new(16, 0)),
                new(new(0, 0), new(0, 0)), new(new(12, 12), new(12, 12)), new(new(0, 0), new(0, 0)), new(new(0, 16), new(0, 16)),
                new(new(-8, 16), new(8, 16)), new(new(-16, 16), new(16, 16)), new(new(-16, 16), new(16, 16)), new(new(-16, 16), new(16, 16)),
                new(new(-16, 16), new(16, 16)), new(new(-16, 16), new(16, 16)), new(new(-8, 16), new(8, 16)), new(new(0, 0), new(0, 0)),
                new(new(-12, 12), new(-12, 12)), new(new(-20, 8), new(-8, 18)), new(new(-24, 2), new(-2, 20)), new(new(-24, 0), new(0, 24)),
                new(new(-24, 0), new(0, 24)), new(new(-24, 0), new(0, 24)),
            ],
            [L.UnchargedIcePlasmaLeft] =
            [
                new(new(0, 0), new(0, 0)), new(new(16, 0), new(16, 0)), new(new(0, 0), new(0, 0)), new(new(12, 12), new(12, 12)),
                new(new(0, 0), new(0, 0)), new(new(0, 16), new(0, 16)), new(new(-8, 16), new(8, 16)), new(new(-16, 16), new(16, 16)),
                new(new(-16, 16), new(16, 16)), new(new(-16, 16), new(16, 16)), new(new(-16, 16), new(16, 16)), new(new(-16, 16), new(16, 16)),
                new(new(-8, 16), new(8, 16)), new(new(0, 0), new(0, 0)), new(new(-12, 12), new(-12, 12)), new(new(-20, 8), new(-8, 18)),
                new(new(-24, 2), new(-2, 20)), new(new(-24, 0), new(0, 24)), new(new(-24, 0), new(0, 24)), new(new(-24, 0), new(0, 24)),
                new(new(-24, 2), new(-2, 20)), new(new(-20, 8), new(-8, 18)),
            ],
            [L.UnchargedIcePlasmaUpLeft] =
            [
                new(new(0, 0), new(0, 0)), new(new(12, 12), new(12, 12)), new(new(0, 0), new(0, 0)), new(new(0, 16), new(0, 16)),
                new(new(-8, 16), new(8, 16)), new(new(-16, 16), new(16, 16)), new(new(-16, 16), new(16, 16)), new(new(-16, 16), new(16, 16)),
                new(new(-16, 16), new(16, 16)), new(new(-16, 16), new(16, 16)), new(new(-8, 16), new(8, 16)), new(new(0, 0), new(0, 0)),
                new(new(-12, 12), new(-12, 12)), new(new(-20, 8), new(-8, 18)), new(new(-24, 2), new(-2, 20)), new(new(-24, 0), new(0, 24)),
                new(new(-24, 0), new(0, 24)), new(new(-24, 0), new(0, 24)), new(new(-24, 2), new(-2, 20)), new(new(-20, 8), new(-8, 18)),
                new(new(0, 0), new(0, 0)), new(new(-16, 0), new(-16, 0)),
            ],
            [L.UnchargedWaveIcePlasmaUp] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, 16), new(0, 16)), new(new(-8, 16), new(8, 16)), new(new(-16, 16), new(16, 16)),
                new(new(-16, 16), new(16, 16)), new(new(-16, 16), new(16, 16)), new(new(-16, 16), new(16, 16)), new(new(-16, 16), new(16, 16)),
                new(new(-8, 16), new(8, 16)), new(new(0, 0), new(0, 0)), new(new(-12, 12), new(-12, 12)), new(new(-20, 8), new(-8, 18)),
                new(new(-24, 2), new(-2, 20)), new(new(-24, 0), new(0, 24)), new(new(-24, 0), new(0, 24)), new(new(-24, 0), new(0, 24)),
                new(new(-24, 2), new(-2, 20)), new(new(-20, 8), new(-8, 18)), new(new(0, 0), new(0, 0)), new(new(-16, 0), new(-16, 0)),
                new(new(-16, -8), new(-16, 8)), new(new(-16, -12), new(-16, 12)),
            ],
            [L.UnchargedWaveIcePlasmaUpRight] =
            [
                new(new(0, 0), new(0, 0)), new(new(-12, 12), new(-12, 12)), new(new(-20, 8), new(-8, 18)), new(new(-24, 2), new(-2, 20)),
                new(new(-24, 0), new(0, 24)), new(new(-24, 0), new(0, 24)), new(new(-24, 0), new(0, 24)), new(new(-24, 2), new(-2, 20)),
                new(new(-20, 8), new(-8, 18)), new(new(0, 0), new(0, 0)), new(new(-16, 0), new(-16, 0)), new(new(-16, -8), new(-16, 8)),
                new(new(-16, -12), new(-16, 12)), new(new(-16, -16), new(-16, 16)), new(new(-16, -16), new(-16, 16)), new(new(-16, -16), new(-16, 16)),
                new(new(-16, -12), new(-16, 12)), new(new(-16, -8), new(-16, 8)), new(new(0, 0), new(0, 0)), new(new(-12, -12), new(-12, -12)),
                new(new(-18, -6), new(-6, -18)), new(new(-20, -2), new(-2, -20)),
            ],
            [L.UnchargedWaveIcePlasmaRight] =
            [
                new(new(0, 0), new(0, 0)), new(new(-16, 0), new(-16, 0)), new(new(-16, -8), new(-16, 8)), new(new(-16, -12), new(-16, 12)),
                new(new(-16, -16), new(-16, 16)), new(new(-16, -16), new(-16, 16)), new(new(-16, -16), new(-16, 16)), new(new(-16, -12), new(-16, 12)),
                new(new(-16, -8), new(-16, 8)), new(new(0, 0), new(0, 0)), new(new(-12, -12), new(-12, -12)), new(new(-18, -6), new(-6, -18)),
                new(new(-20, -2), new(-2, -20)), new(new(-24, 0), new(0, -24)), new(new(-24, 0), new(0, -24)), new(new(-24, 0), new(0, -24)),
                new(new(-20, -2), new(-2, -20)), new(new(-18, -6), new(-6, -18)), new(new(0, 0), new(0, 0)), new(new(0, -16), new(0, -16)),
                new(new(-8, -16), new(8, -16)), new(new(-16, -16), new(16, -16)),
            ],
            [L.UnchargedWaveIcePlasmaDownRight] =
            [
                new(new(0, 0), new(0, 0)), new(new(-12, -12), new(-12, -12)), new(new(-18, -6), new(-6, -18)), new(new(-20, -2), new(-2, -20)),
                new(new(-24, 0), new(0, -24)), new(new(-24, 0), new(0, -24)), new(new(-24, 0), new(0, -24)), new(new(-20, -2), new(-2, -20)),
                new(new(-18, -6), new(-6, -18)), new(new(0, 0), new(0, 0)), new(new(0, -16), new(0, -16)), new(new(-8, -16), new(8, -16)),
                new(new(-16, -16), new(16, -16)), new(new(-16, -16), new(16, -16)), new(new(-16, -16), new(16, -16)), new(new(-16, -16), new(16, -16)),
                new(new(-16, -16), new(16, -16)), new(new(-8, -16), new(8, -16)), new(new(0, 0), new(0, 0)), new(new(12, -12), new(12, -12)),
                new(new(20, -8), new(8, -18)), new(new(24, -2), new(2, -20)),
            ],
            [L.UnchargedWaveIcePlasmaDown] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, -16), new(0, -16)), new(new(-8, -16), new(8, -16)), new(new(-16, -16), new(16, -16)),
                new(new(-16, -16), new(16, -16)), new(new(-16, -16), new(16, -16)), new(new(-16, -16), new(16, -16)), new(new(-16, -16), new(16, -16)),
                new(new(-8, -16), new(8, -16)), new(new(0, 0), new(0, 0)), new(new(12, -12), new(12, -12)), new(new(20, -8), new(8, -18)),
                new(new(24, -2), new(2, -20)), new(new(24, 0), new(0, -24)), new(new(24, 0), new(0, -24)), new(new(24, 0), new(0, -24)),
                new(new(24, -2), new(2, -20)), new(new(20, -8), new(8, -18)), new(new(0, 0), new(0, 0)), new(new(16, 0), new(16, 0)),
                new(new(16, -8), new(16, 8)), new(new(16, -12), new(16, 12)),
            ],
            [L.UnchargedWaveIcePlasmaDownLeft] =
            [
                new(new(0, 0), new(0, 0)), new(new(12, -12), new(12, -12)), new(new(20, -8), new(8, -18)), new(new(24, -2), new(2, -20)),
                new(new(24, 0), new(0, -24)), new(new(24, 0), new(0, -24)), new(new(24, 0), new(0, -24)), new(new(24, -2), new(2, -20)),
                new(new(20, -8), new(8, -18)), new(new(0, 0), new(0, 0)), new(new(16, 0), new(16, 0)), new(new(16, -8), new(16, 8)),
                new(new(16, -12), new(16, 12)), new(new(16, -16), new(16, 16)), new(new(16, -16), new(16, 16)), new(new(16, -16), new(16, 16)),
                new(new(16, -12), new(16, 12)), new(new(16, -8), new(16, 8)), new(new(0, 0), new(0, 0)), new(new(12, 12), new(12, 12)),
                new(new(18, 6), new(6, 18)), new(new(20, 2), new(2, 20)),
            ],
            [L.UnchargedWaveIcePlasmaLeft] =
            [
                new(new(0, 0), new(0, 0)), new(new(16, 0), new(16, 0)), new(new(16, -8), new(16, 8)), new(new(16, -12), new(16, 12)),
                new(new(16, -16), new(16, 16)), new(new(16, -16), new(16, 16)), new(new(16, -16), new(16, 16)), new(new(16, -12), new(16, 12)),
                new(new(16, -8), new(16, 8)), new(new(0, 0), new(0, 0)), new(new(12, 12), new(12, 12)), new(new(18, 6), new(6, 18)),
                new(new(20, 2), new(2, 20)), new(new(24, 0), new(0, 24)), new(new(24, 0), new(0, 24)), new(new(24, 0), new(0, 24)),
                new(new(20, 2), new(2, 20)), new(new(18, 6), new(6, 18)),
            ],
            [L.UnchargedWaveIcePlasmaUpLeft] =
            [
                new(new(0, 0), new(0, 0)), new(new(12, 12), new(12, 12)), new(new(18, 6), new(6, 18)), new(new(20, 2), new(2, 20)),
                new(new(24, 0), new(0, 24)), new(new(24, 0), new(0, 24)), new(new(24, 0), new(0, 24)), new(new(20, 2), new(2, 20)),
                new(new(18, 6), new(6, 18)),
            ],
            [L.ChargedDefault] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)),
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)),
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(-8, 0), new(8, 0)), new(new(-8, 0), new(8, 0)),
                new(new(-12, 0), new(12, 0)), new(new(-12, 0), new(12, 0)), new(new(-16, 0), new(16, 0)), new(new(-16, 0), new(16, 0)),
                new(new(-16, 0), new(16, 0)), new(new(-16, 0), new(16, 0)), new(new(-16, 0), new(16, 0)), new(new(-16, 0), new(16, 0)),
                new(new(-12, 0), new(12, 0)), new(new(-12, 0), new(12, 0)),
            ],
            [L.ChargedWaveVertical] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(-8, 0), new(8, 0)), new(new(-8, 0), new(8, 0)),
                new(new(-12, 0), new(12, 0)), new(new(-12, 0), new(12, 0)), new(new(-16, 0), new(16, 0)), new(new(-16, 0), new(16, 0)),
                new(new(-16, 0), new(16, 0)), new(new(-16, 0), new(16, 0)), new(new(-16, 0), new(16, 0)), new(new(-16, 0), new(16, 0)),
                new(new(-12, 0), new(12, 0)), new(new(-12, 0), new(12, 0)), new(new(-8, 0), new(8, 0)), new(new(-8, 0), new(8, 0)),
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(0, -8), new(0, 8)), new(new(0, -8), new(0, 8)),
                new(new(0, -12), new(0, 12)), new(new(0, -12), new(0, 12)),
            ],
            [L.ChargedWaveHorizontal] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(0, -8), new(0, 8)), new(new(0, -8), new(0, 8)),
                new(new(0, -12), new(0, 12)), new(new(0, -12), new(0, 12)), new(new(0, -16), new(0, 16)), new(new(0, -16), new(0, 16)),
                new(new(0, -16), new(0, 16)), new(new(0, -16), new(0, 16)), new(new(0, -16), new(0, 16)), new(new(0, -16), new(0, 16)),
                new(new(0, -12), new(0, 12)), new(new(0, -12), new(0, 12)), new(new(0, -8), new(0, 8)), new(new(0, -8), new(0, 8)),
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(-4, -4), new(4, 4)), new(new(-4, -4), new(4, 4)),
                new(new(-8, -8), new(8, 8)), new(new(-8, -8), new(8, 8)),
            ],
            [L.ChargedWaveRisingDiagonal] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(-4, -4), new(4, 4)), new(new(-4, -4), new(4, 4)),
                new(new(-8, -8), new(8, 8)), new(new(-8, -8), new(8, 8)), new(new(-8, -8), new(8, 8)), new(new(-8, -8), new(8, 8)),
                new(new(-10, -10), new(10, 10)), new(new(-10, -10), new(10, 10)), new(new(-8, -8), new(8, 8)), new(new(-8, -8), new(8, 8)),
                new(new(-8, -8), new(8, 8)), new(new(-8, -8), new(8, 8)), new(new(-4, -4), new(4, 4)), new(new(-4, -4), new(4, 4)),
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(-4, 4), new(4, -4)), new(new(-4, 4), new(4, -4)),
                new(new(-8, 8), new(8, -8)), new(new(-8, 8), new(8, -8)),
            ],
            [L.ChargedWaveFallingDiagonal] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(-4, 4), new(4, -4)), new(new(-4, 4), new(4, -4)),
                new(new(-8, 8), new(8, -8)), new(new(-8, 8), new(8, -8)), new(new(-8, 8), new(8, -8)), new(new(-8, 8), new(8, -8)),
                new(new(-10, 10), new(10, -10)), new(new(-10, 10), new(10, -10)), new(new(-8, 8), new(8, -8)), new(new(-8, 8), new(8, -8)),
                new(new(-8, 8), new(8, -8)), new(new(-8, 8), new(8, -8)), new(new(-4, 4), new(4, -4)), new(new(-4, 4), new(4, -4)),
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(0, 8), new(0, 8)), new(new(0, 8), new(0, 8)),
                new(new(0, 16), new(0, 16)), new(new(0, 16), new(0, 16)),
            ],
            [L.ChargedIceSpazerUp] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(0, 8), new(0, 8)), new(new(0, 8), new(0, 8)),
                new(new(0, 16), new(0, 16)), new(new(0, 16), new(0, 16)), new(new(-8, 16), new(8, 16)), new(new(-8, 16), new(8, 16)),
                new(new(-16, 16), new(16, 16)), new(new(-16, 16), new(16, 16)), new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)),
                new(new(-8, 8), new(-8, 8)), new(new(-8, 8), new(-8, 8)), new(new(-12, 12), new(-12, 12)), new(new(-12, 12), new(-12, 12)),
                new(new(-16, 8), new(-8, 16)), new(new(-16, 8), new(-8, 16)), new(new(-24, 0), new(0, 24)), new(new(-24, 0), new(0, 24)),
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)),
            ],
            [L.ChargedIceSpazerUpRight] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(-8, 8), new(-8, 8)), new(new(-8, 8), new(-8, 8)),
                new(new(-12, 12), new(-12, 12)), new(new(-12, 12), new(-12, 12)), new(new(-16, 8), new(-8, 16)), new(new(-16, 8), new(-8, 16)),
                new(new(-24, 0), new(0, 24)), new(new(-24, 0), new(0, 24)), new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)),
                new(new(-8, 8), new(-8, 8)), new(new(-8, 8), new(-8, 8)), new(new(-16, 0), new(-16, 0)), new(new(-16, 0), new(-16, 0)),
                new(new(-16, -8), new(-16, 8)), new(new(-16, -8), new(-16, 8)), new(new(-16, -16), new(-16, 16)), new(new(-16, -16), new(-16, 16)),
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)),
            ],
            [L.ChargedIceSpazerRight] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(-8, 8), new(-8, 8)), new(new(-8, 8), new(-8, 8)),
                new(new(-16, 0), new(-16, 0)), new(new(-16, 0), new(-16, 0)), new(new(-16, -8), new(-16, 8)), new(new(-16, -8), new(-16, 8)),
                new(new(-16, -16), new(-16, 16)), new(new(-16, -16), new(-16, 16)), new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)),
                new(new(-8, -8), new(-8, -8)), new(new(-8, -8), new(-8, -8)), new(new(-12, -12), new(-12, -12)), new(new(-12, -12), new(-12, -12)),
                new(new(-16, -8), new(-8, -16)), new(new(-16, -8), new(-8, -16)), new(new(-24, 0), new(0, -24)), new(new(-24, 0), new(0, -24)),
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)),
            ],
            [L.ChargedIceSpazerDownRight] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(-8, -8), new(-8, -8)), new(new(-8, -8), new(-8, -8)),
                new(new(-12, -12), new(-12, -12)), new(new(-12, -12), new(-12, -12)), new(new(-16, -8), new(-8, -16)), new(new(-16, -8), new(-8, -16)),
                new(new(-24, 0), new(0, -24)), new(new(-24, 0), new(0, -24)), new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)),
                new(new(0, -8), new(0, -8)), new(new(0, -8), new(0, -8)), new(new(0, -16), new(0, -16)), new(new(0, -16), new(0, -16)),
                new(new(-8, -16), new(8, -16)), new(new(-8, -16), new(8, -16)), new(new(-16, -16), new(16, -16)), new(new(-16, -16), new(16, -16)),
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)),
            ],
            [L.ChargedIceSpazerDown] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(0, -8), new(0, -8)), new(new(0, -8), new(0, -8)),
                new(new(0, -16), new(0, -16)), new(new(0, -16), new(0, -16)), new(new(-8, -16), new(8, -16)), new(new(-8, -16), new(8, -16)),
                new(new(-16, -16), new(16, -16)), new(new(-16, -16), new(16, -16)), new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)),
                new(new(8, -8), new(8, -8)), new(new(8, -8), new(8, -8)), new(new(12, -12), new(12, -12)), new(new(12, -12), new(12, -12)),
                new(new(16, -8), new(8, -16)), new(new(16, -8), new(8, -16)), new(new(24, 0), new(0, -24)), new(new(24, 0), new(0, -24)),
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)),
            ],
            [L.ChargedIceSpazerDownLeft] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(8, -8), new(8, -8)), new(new(8, -8), new(8, -8)),
                new(new(12, -12), new(12, -12)), new(new(12, -12), new(12, -12)), new(new(16, -8), new(8, -16)), new(new(16, -8), new(8, -16)),
                new(new(24, 0), new(0, -24)), new(new(24, 0), new(0, -24)), new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)),
                new(new(8, 0), new(8, 0)), new(new(8, 0), new(8, 0)), new(new(16, 0), new(16, 0)), new(new(16, 0), new(16, 0)),
                new(new(16, -8), new(16, 8)), new(new(16, -8), new(16, 8)), new(new(16, -16), new(16, 16)), new(new(16, -16), new(16, 16)),
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)),
            ],
            [L.ChargedIceSpazerLeft] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(8, 0), new(8, 0)), new(new(8, 0), new(8, 0)),
                new(new(16, 0), new(16, 0)), new(new(16, 0), new(16, 0)), new(new(16, -8), new(16, 8)), new(new(16, -8), new(16, 8)),
                new(new(16, -16), new(16, 16)), new(new(16, -16), new(16, 16)), new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)),
                new(new(8, 8), new(8, 8)), new(new(8, 8), new(8, 8)), new(new(12, 12), new(12, 12)), new(new(12, 12), new(12, 12)),
                new(new(16, 8), new(8, 16)), new(new(16, 8), new(8, 16)), new(new(24, 0), new(0, 24)), new(new(24, 0), new(0, 24)),
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)),
            ],
            [L.ChargedIceSpazerUpLeft] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(8, 8), new(8, 8)), new(new(8, 8), new(8, 8)),
                new(new(12, 12), new(12, 12)), new(new(12, 12), new(12, 12)), new(new(16, 8), new(8, 16)), new(new(16, 8), new(8, 16)),
                new(new(24, 0), new(0, 24)), new(new(24, 0), new(0, 24)), new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)),
                new(new(0, 8), new(0, 8)), new(new(0, 8), new(0, 8)), new(new(0, 16), new(0, 16)), new(new(0, 16), new(0, 16)),
                new(new(-4, 16), new(4, 16)), new(new(-4, 16), new(4, 16)), new(new(-8, 16), new(8, 16)), new(new(-8, 16), new(8, 16)),
                new(new(-12, 16), new(12, 16)), new(new(-12, 16), new(12, 16)),
            ],
            [L.ChargedWaveIceSpazerUp] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(0, 8), new(0, 8)), new(new(0, 8), new(0, 8)),
                new(new(0, 16), new(0, 16)), new(new(0, 16), new(0, 16)), new(new(-4, 16), new(4, 16)), new(new(-4, 16), new(4, 16)),
                new(new(-8, 16), new(8, 16)), new(new(-8, 16), new(8, 16)), new(new(-12, 16), new(12, 16)), new(new(-12, 16), new(12, 16)),
                new(new(-16, 16), new(16, 16)), new(new(-16, 16), new(16, 16)), new(new(-16, 16), new(16, 16)), new(new(-16, 16), new(16, 16)),
                new(new(-16, 16), new(16, 16)), new(new(-16, 16), new(16, 16)), new(new(-12, 16), new(12, 16)), new(new(-12, 16), new(12, 16)),
                new(new(-8, 16), new(8, 16)), new(new(-8, 16), new(8, 16)),
            ],
            [L.ChargedWaveIceSpazerUpRight] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(-8, 8), new(-8, 8)), new(new(-8, 8), new(-8, 8)),
                new(new(-12, 12), new(-12, 12)), new(new(-12, 12), new(-12, 12)), new(new(-16, 8), new(-8, 16)), new(new(-16, 8), new(-8, 16)),
                new(new(-16, 8), new(-8, 16)), new(new(-16, 8), new(-8, 16)), new(new(-16, 8), new(-8, 16)), new(new(-16, 8), new(-8, 16)),
                new(new(-24, 0), new(0, 24)), new(new(-24, 0), new(0, 24)), new(new(-24, 0), new(0, 24)), new(new(-24, 0), new(0, 24)),
                new(new(-24, 0), new(0, 24)), new(new(-24, 0), new(0, 24)), new(new(-16, 8), new(-8, 16)), new(new(-16, 8), new(-8, 16)),
                new(new(-16, 8), new(-8, 16)), new(new(-16, 8), new(-8, 16)),
            ],
            [L.ChargedWaveIceSpazerRight] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(-8, 0), new(-8, 0)), new(new(-8, 0), new(-8, 0)),
                new(new(-16, 0), new(-16, 0)), new(new(-16, 0), new(-16, 0)), new(new(-16, -4), new(-16, 4)), new(new(-16, -4), new(-16, 4)),
                new(new(-16, -8), new(-16, 8)), new(new(-16, -8), new(-16, 8)), new(new(-16, -12), new(-16, 12)), new(new(-16, -12), new(-16, 12)),
                new(new(-16, -16), new(-16, 16)), new(new(-16, -16), new(-16, 16)), new(new(-16, -16), new(-16, 16)), new(new(-16, -16), new(-16, 16)),
                new(new(-16, -16), new(-16, 16)), new(new(-16, -16), new(-16, 16)), new(new(-16, -12), new(-16, 12)), new(new(-16, -12), new(-16, 12)),
                new(new(-16, -8), new(-16, 8)), new(new(-16, -8), new(-16, 8)),
            ],
            [L.ChargedWaveIceSpazerDownRight] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(-8, -8), new(-8, -8)), new(new(-8, -8), new(-8, -8)),
                new(new(-12, -12), new(-12, -12)), new(new(-12, -12), new(-12, -12)), new(new(-16, -8), new(-8, -16)), new(new(-16, -8), new(-8, -16)),
                new(new(-16, -8), new(-8, -16)), new(new(-16, -8), new(-8, -16)), new(new(-20, -4), new(-4, -20)), new(new(-20, -4), new(-4, -20)),
                new(new(-24, 0), new(0, -24)), new(new(-24, 0), new(0, -24)), new(new(-24, 0), new(0, -24)), new(new(-24, 0), new(0, -24)),
                new(new(-24, 0), new(0, -24)), new(new(-24, 0), new(0, -24)), new(new(-20, -4), new(-4, -20)), new(new(-20, -4), new(-4, -20)),
                new(new(-16, -8), new(-8, -16)), new(new(-16, -8), new(-8, -16)),
            ],
            [L.ChargedWaveIceSpazerDown] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(0, -8), new(0, -8)), new(new(0, -8), new(0, -8)),
                new(new(0, -16), new(0, -16)), new(new(0, -16), new(0, -16)), new(new(-4, -16), new(4, -16)), new(new(-4, -16), new(4, -16)),
                new(new(-8, -16), new(8, -16)), new(new(-8, -16), new(8, -16)), new(new(-12, -16), new(12, -16)), new(new(-12, -16), new(12, -16)),
                new(new(-16, -16), new(16, -16)), new(new(-16, -16), new(16, -16)), new(new(-16, -16), new(16, -16)), new(new(-16, -16), new(16, -16)),
                new(new(-16, -16), new(16, -16)), new(new(-16, -16), new(16, -16)), new(new(-12, -16), new(12, -16)), new(new(-12, -16), new(12, -16)),
                new(new(-8, -16), new(8, -16)), new(new(-8, -16), new(8, -16)),
            ],
            [L.ChargedWaveIceSpazerDownLeft] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(8, -8), new(8, -8)), new(new(8, -8), new(8, -8)),
                new(new(12, -12), new(12, -12)), new(new(12, -12), new(12, -12)), new(new(8, -16), new(16, -8)), new(new(8, -16), new(16, -8)),
                new(new(8, -16), new(16, -8)), new(new(8, -16), new(16, -8)), new(new(4, -20), new(20, -4)), new(new(4, -20), new(20, -4)),
                new(new(0, -24), new(24, 0)), new(new(0, -24), new(24, 0)), new(new(0, -24), new(24, 0)), new(new(0, -24), new(24, 0)),
                new(new(0, -24), new(24, 0)), new(new(0, -24), new(24, 0)), new(new(4, -20), new(20, -4)), new(new(4, -20), new(20, -4)),
                new(new(8, -16), new(16, -8)), new(new(8, -16), new(16, -8)),
            ],
            [L.ChargedWaveIceSpazerLeft] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(8, 0), new(8, 0)), new(new(8, 0), new(8, 0)),
                new(new(16, 0), new(16, 0)), new(new(16, 0), new(16, 0)), new(new(16, -4), new(16, 4)), new(new(16, -4), new(16, 4)),
                new(new(16, -8), new(16, 8)), new(new(16, -8), new(16, 8)), new(new(16, -12), new(16, 12)), new(new(16, -12), new(16, 12)),
                new(new(16, -16), new(16, 16)), new(new(16, -16), new(16, 16)), new(new(16, -16), new(16, 16)), new(new(16, -16), new(16, 16)),
                new(new(16, -16), new(16, 16)), new(new(16, -16), new(16, 16)), new(new(16, -12), new(16, 12)), new(new(16, -12), new(16, 12)),
                new(new(16, -8), new(16, 8)), new(new(16, -8), new(16, 8)),
            ],
            [L.ChargedWaveIceSpazerUpLeft] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(8, 8), new(8, 8)), new(new(8, 8), new(8, 8)),
                new(new(12, 12), new(12, 12)), new(new(12, 12), new(12, 12)), new(new(8, 16), new(16, 8)), new(new(8, 16), new(16, 8)),
                new(new(8, 16), new(16, 8)), new(new(8, 16), new(16, 8)), new(new(4, 20), new(20, 4)), new(new(4, 20), new(20, 4)),
                new(new(0, 24), new(24, 0)), new(new(0, 24), new(24, 0)), new(new(0, 24), new(24, 0)), new(new(0, 24), new(24, 0)),
                new(new(0, 24), new(24, 0)), new(new(0, 24), new(24, 0)), new(new(4, 20), new(20, 4)), new(new(4, 20), new(20, 4)),
                new(new(8, 16), new(16, 8)), new(new(8, 16), new(16, 8)),
            ],
            [L.ChargedIcePlasmaUp] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(0, 12), new(0, 12)), new(new(0, 12), new(0, 12)),
                new(new(0, 24), new(0, 24)), new(new(0, 24), new(0, 24)), new(new(0, 28), new(0, 28)), new(new(0, 28), new(0, 28)),
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(-8, 8), new(-8, 8)), new(new(-8, 8), new(-8, 8)),
                new(new(-16, 16), new(-16, 16)), new(new(-16, 16), new(-16, 16)), new(new(-24, 24), new(-24, 24)), new(new(-24, 24), new(-24, 24)),
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(-12, 0), new(-12, 0)), new(new(-12, 0), new(-12, 0)),
                new(new(-24, 0), new(-24, 0)), new(new(-24, 0), new(-24, 0)),
            ],
            [L.ChargedIcePlasmaUpRight] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(-8, 8), new(-8, 8)), new(new(-8, 8), new(-8, 8)),
                new(new(-16, 16), new(-16, 16)), new(new(-16, 16), new(-16, 16)), new(new(-24, 24), new(-24, 24)), new(new(-24, 24), new(-24, 24)),
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(-12, 0), new(-12, 0)), new(new(-12, 0), new(-12, 0)),
                new(new(-24, 0), new(-24, 0)), new(new(-24, 0), new(-24, 0)), new(new(-28, 0), new(-28, 0)), new(new(-28, 0), new(-28, 0)),
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(-8, -8), new(-8, -8)), new(new(-8, -8), new(-8, -8)),
                new(new(-16, -16), new(-16, -16)), new(new(-16, -16), new(-16, -16)),
            ],
            [L.ChargedIcePlasmaRight] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(-12, 0), new(-12, 0)), new(new(-12, 0), new(-12, 0)),
                new(new(-24, 0), new(-24, 0)), new(new(-24, 0), new(-24, 0)), new(new(-28, 0), new(-28, 0)), new(new(-28, 0), new(-28, 0)),
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(-8, -8), new(-8, -8)), new(new(-8, -8), new(-8, -8)),
                new(new(-16, -16), new(-16, -16)), new(new(-16, -16), new(-16, -16)), new(new(-24, -24), new(-24, -24)), new(new(-24, -24), new(-24, -24)),
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(0, -12), new(0, -12)), new(new(0, -12), new(0, -12)),
                new(new(0, -24), new(0, -24)), new(new(0, -24), new(0, -24)),
            ],
            [L.ChargedIcePlasmaDownRight] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(-8, -8), new(-8, -8)), new(new(-8, -8), new(-8, -8)),
                new(new(-16, -16), new(-16, -16)), new(new(-16, -16), new(-16, -16)), new(new(-24, -24), new(-24, -24)), new(new(-24, -24), new(-24, -24)),
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(0, -12), new(0, -12)), new(new(0, -12), new(0, -12)),
                new(new(0, -24), new(0, -24)), new(new(0, -24), new(0, -24)), new(new(0, -28), new(0, -28)), new(new(0, -28), new(0, -28)),
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(8, -8), new(8, -8)), new(new(8, -8), new(8, -8)),
                new(new(16, -16), new(16, -16)), new(new(16, -16), new(16, -16)),
            ],
            [L.ChargedIcePlasmaDown] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(0, -12), new(0, -12)), new(new(0, -12), new(0, -12)),
                new(new(0, -24), new(0, -24)), new(new(0, -24), new(0, -24)), new(new(0, -28), new(0, -28)), new(new(0, -28), new(0, -28)),
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(8, -8), new(8, -8)), new(new(8, -8), new(8, -8)),
                new(new(16, -16), new(16, -16)), new(new(16, -16), new(16, -16)), new(new(24, -24), new(24, -24)), new(new(24, -24), new(24, -24)),
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(12, 0), new(12, 0)), new(new(12, 0), new(12, 0)),
                new(new(24, 0), new(24, 0)), new(new(24, 0), new(24, 0)),
            ],
            [L.ChargedIcePlasmaDownLeft] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(8, -8), new(8, -8)), new(new(8, -8), new(8, -8)),
                new(new(16, -16), new(16, -16)), new(new(16, -16), new(16, -16)), new(new(24, -24), new(24, -24)), new(new(24, -24), new(24, -24)),
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(12, 0), new(12, 0)), new(new(12, 0), new(12, 0)),
                new(new(24, 0), new(24, 0)), new(new(24, 0), new(24, 0)), new(new(28, 0), new(28, 0)), new(new(28, 0), new(28, 0)),
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(8, 8), new(8, 8)), new(new(8, 8), new(8, 8)),
                new(new(16, 16), new(16, 16)), new(new(16, 16), new(16, 16)),
            ],
            [L.ChargedIcePlasmaLeft] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(12, 0), new(12, 0)), new(new(12, 0), new(12, 0)),
                new(new(24, 0), new(24, 0)), new(new(24, 0), new(24, 0)), new(new(28, 0), new(28, 0)), new(new(28, 0), new(28, 0)),
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(8, 8), new(8, 8)), new(new(8, 8), new(8, 8)),
                new(new(16, 16), new(16, 16)), new(new(16, 16), new(16, 16)), new(new(24, 24), new(24, 24)), new(new(24, 24), new(24, 24)),
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(0, 12), new(0, 12)), new(new(0, 12), new(0, 12)),
                new(new(0, 24), new(0, 24)), new(new(0, 24), new(0, 24)),
            ],
            [L.ChargedIcePlasmaUpLeft] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(8, 8), new(8, 8)), new(new(8, 8), new(8, 8)),
                new(new(16, 16), new(16, 16)), new(new(16, 16), new(16, 16)), new(new(24, 24), new(24, 24)), new(new(24, 24), new(24, 24)),
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(0, 12), new(0, 12)), new(new(0, 12), new(0, 12)),
                new(new(0, 24), new(0, 24)), new(new(0, 24), new(0, 24)), new(new(0, 28), new(0, 28)), new(new(0, 28), new(0, 28)),
                new(new(-8, 28), new(8, 28)), new(new(-8, 28), new(8, 28)), new(new(-12, 28), new(12, 28)), new(new(-12, 28), new(12, 28)),
                new(new(-16, 28), new(16, 28)), new(new(-16, 28), new(16, 28)),
            ],
            [L.ChargedWaveIcePlasmaUp] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(0, 12), new(0, 12)), new(new(0, 12), new(0, 12)),
                new(new(0, 24), new(0, 24)), new(new(0, 24), new(0, 24)), new(new(0, 28), new(0, 28)), new(new(0, 28), new(0, 28)),
                new(new(-8, 28), new(8, 28)), new(new(-8, 28), new(8, 28)), new(new(-12, 28), new(12, 28)), new(new(-12, 28), new(12, 28)),
                new(new(-16, 28), new(16, 28)), new(new(-16, 28), new(16, 28)), new(new(-16, 28), new(16, 28)), new(new(-16, 28), new(16, 28)),
                new(new(-16, 28), new(16, 28)), new(new(-16, 28), new(16, 28)), new(new(-12, 28), new(12, 28)), new(new(-12, 28), new(12, 28)),
                new(new(-8, 28), new(8, 28)), new(new(-8, 28), new(8, 28)),
            ],
            [L.ChargedWaveIcePlasmaUpRight] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(-8, 8), new(-8, 8)), new(new(-8, 8), new(-8, 8)),
                new(new(-16, 16), new(-16, 16)), new(new(-16, 16), new(-16, 16)), new(new(-20, 20), new(-20, 20)), new(new(-20, 20), new(-20, 20)),
                new(new(-28, 12), new(-16, 24)), new(new(-28, 12), new(-16, 24)), new(new(-32, 12), new(-12, 28)), new(new(-32, 12), new(-12, 28)),
                new(new(-32, 8), new(-8, 32)), new(new(-32, 8), new(-8, 32)), new(new(-32, 8), new(-8, 32)), new(new(-32, 8), new(-8, 32)),
                new(new(-32, 8), new(-8, 32)), new(new(-32, 8), new(-8, 32)), new(new(-32, 12), new(-12, 28)), new(new(-32, 12), new(-12, 28)),
                new(new(-28, 12), new(-16, 24)), new(new(-28, 12), new(-16, 24)),
            ],
            [L.ChargedWaveIcePlasmaRight] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(-12, 0), new(-12, 0)), new(new(-12, 0), new(-12, 0)),
                new(new(-24, 0), new(-24, 0)), new(new(-24, 0), new(-24, 0)), new(new(-28, 0), new(-28, 0)), new(new(-28, 0), new(-28, 0)),
                new(new(-28, -8), new(-28, 8)), new(new(-28, -8), new(-28, 8)), new(new(-28, -12), new(-28, 12)), new(new(-28, -12), new(-28, 12)),
                new(new(-28, -16), new(-28, 16)), new(new(-28, -16), new(-28, 16)), new(new(-28, -16), new(-28, 16)), new(new(-28, -16), new(-28, 16)),
                new(new(-28, -16), new(-28, 16)), new(new(-28, -16), new(-28, 16)), new(new(-28, -12), new(-28, 12)), new(new(-28, -12), new(-28, 12)),
                new(new(-28, -8), new(-28, 8)), new(new(-28, -8), new(-28, 8)),
            ],
            [L.ChargedWaveIcePlasmaDownRight] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(-8, -8), new(-8, -8)), new(new(-8, -8), new(-8, -8)),
                new(new(-16, -16), new(-16, -16)), new(new(-16, -16), new(-16, -16)), new(new(-20, -20), new(-20, -20)), new(new(-20, -20), new(-20, -20)),
                new(new(-24, -16), new(-16, -24)), new(new(-24, -16), new(-16, -24)), new(new(-32, -12), new(-12, -32)), new(new(-32, -12), new(-12, -32)),
                new(new(-32, -8), new(-8, -32)), new(new(-32, -8), new(-8, -32)), new(new(-32, -8), new(-8, -32)), new(new(-32, -8), new(-8, -32)),
                new(new(-32, -8), new(-8, -32)), new(new(-32, -8), new(-8, -32)), new(new(-32, -12), new(-12, -32)), new(new(-32, -12), new(-12, -32)),
                new(new(-24, -16), new(-16, -24)), new(new(-24, -16), new(-16, -24)),
            ],
            [L.ChargedWaveIcePlasmaDown] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(0, -12), new(0, -12)), new(new(0, -12), new(0, -12)),
                new(new(0, -24), new(0, -24)), new(new(0, -24), new(0, -24)), new(new(0, -28), new(0, -28)), new(new(0, -28), new(0, -28)),
                new(new(-8, -28), new(8, -28)), new(new(-8, -28), new(8, -28)), new(new(-12, -28), new(12, -28)), new(new(-12, -28), new(12, -28)),
                new(new(-16, -28), new(16, -28)), new(new(-16, -28), new(16, -28)), new(new(-16, -28), new(16, -28)), new(new(-16, -28), new(16, -28)),
                new(new(-16, -28), new(16, -28)), new(new(-16, -28), new(16, -28)), new(new(-12, -28), new(12, -28)), new(new(-12, -28), new(12, -28)),
                new(new(-8, -28), new(8, -28)), new(new(-8, -28), new(8, -28)),
            ],
            [L.ChargedWaveIcePlasmaDownLeft] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(8, -8), new(8, -8)), new(new(8, -8), new(8, -8)),
                new(new(16, -16), new(16, -16)), new(new(16, -16), new(16, -16)), new(new(20, -20), new(20, -20)), new(new(20, -20), new(20, -20)),
                new(new(28, -12), new(16, -24)), new(new(28, -12), new(16, -24)), new(new(32, -12), new(12, -28)), new(new(32, -12), new(12, -28)),
                new(new(32, -8), new(8, -32)), new(new(32, -8), new(8, -32)), new(new(32, -8), new(8, -32)), new(new(32, -8), new(8, -32)),
                new(new(32, -8), new(8, -32)), new(new(32, -8), new(8, -32)), new(new(32, -12), new(12, -28)), new(new(32, -12), new(12, -28)),
                new(new(28, -12), new(16, -24)), new(new(28, -12), new(16, -24)),
            ],
            [L.ChargedWaveIcePlasmaLeft] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(12, 0), new(12, 0)), new(new(12, 0), new(12, 0)),
                new(new(24, 0), new(24, 0)), new(new(24, 0), new(24, 0)), new(new(28, 0), new(28, 0)), new(new(28, 0), new(28, 0)),
                new(new(28, -8), new(28, 8)), new(new(28, -8), new(28, 8)), new(new(28, -12), new(28, 12)), new(new(28, -12), new(28, 12)),
                new(new(28, -16), new(28, 16)), new(new(28, -16), new(28, 16)), new(new(28, -16), new(28, 16)), new(new(28, -16), new(28, 16)),
                new(new(28, -16), new(28, 16)), new(new(28, -16), new(28, 16)), new(new(28, -12), new(28, 12)), new(new(28, -12), new(28, 12)),
                new(new(28, -8), new(28, 8)), new(new(28, -8), new(28, 8)),
            ],
            [L.ChargedWaveIcePlasmaUpLeft] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, 0), new(0, 0)), new(new(8, 8), new(8, 8)), new(new(8, 8), new(8, 8)),
                new(new(16, 16), new(16, 16)), new(new(16, 16), new(16, 16)), new(new(20, 20), new(20, 20)), new(new(20, 20), new(20, 20)),
                new(new(24, 16), new(16, 24)), new(new(24, 16), new(16, 24)), new(new(32, 12), new(12, 32)), new(new(32, 12), new(12, 32)),
                new(new(32, 8), new(8, 32)), new(new(32, 8), new(8, 32)), new(new(32, 8), new(8, 32)), new(new(32, 8), new(8, 32)),
                new(new(32, 8), new(8, 32)), new(new(32, 8), new(8, 32)), new(new(32, 12), new(12, 32)), new(new(32, 12), new(12, 32)),
                new(new(24, 16), new(16, 24)), new(new(24, 16), new(16, 24)),
            ],
            [L.SbaWaveSpazerVertical] =
            [
                new(new(0, 0), new(0, 0)), new(new(16, 0), new(-16, 0)), new(new(0, 0), new(0, 0)), new(new(-16, 0), new(16, 0)),
                new(new(0, 0), new(0, 0)), new(new(-10, -10), new(10, 10)), new(new(0, 0), new(0, 0)), new(new(10, 10), new(-10, -10)),
                new(new(0, 0), new(0, 0)), new(new(0, -16), new(0, 16)), new(new(0, 0), new(0, 0)), new(new(0, 16), new(0, -16)),
                new(new(0, 0), new(0, 0)), new(new(10, -10), new(-10, 10)), new(new(0, 0), new(0, 0)), new(new(-10, 10), new(10, -10)),
            ],
            [L.SbaWaveSpazerRisingDiagonal] =
            [
                new(new(0, 0), new(0, 0)), new(new(-10, -10), new(10, 10)), new(new(0, 0), new(0, 0)), new(new(10, 10), new(-10, -10)),
                new(new(0, 0), new(0, 0)), new(new(0, -16), new(0, 16)), new(new(0, 0), new(0, 0)), new(new(0, 16), new(0, -16)),
                new(new(0, 0), new(0, 0)), new(new(10, -10), new(-10, 10)), new(new(0, 0), new(0, 0)), new(new(-10, 10), new(10, -10)),
            ],
            [L.SbaWaveSpazerHorizontal] =
            [
                new(new(0, 0), new(0, 0)), new(new(0, -16), new(0, 16)), new(new(0, 0), new(0, 0)), new(new(0, 16), new(0, -16)),
                new(new(0, 0), new(0, 0)), new(new(10, -10), new(-10, 10)), new(new(0, 0), new(0, 0)), new(new(-10, 10), new(10, -10)),
            ],
            [L.SbaWaveSpazerFallingDiagonal] =
            [
                new(new(0, 0), new(0, 0)), new(new(10, -10), new(-10, 10)), new(new(0, 0), new(0, 0)), new(new(-10, 10), new(10, -10)),
            ],
            [L.SbaSpazer] =
            [
                new(new(0, 0), new(0, 0)), new(new(-8, 8), new(8, 8)), new(new(-16, 8), new(16, 8)), new(new(0, 0), new(0, 0)),
                new(new(-8, -8), new(8, -8)), new(new(-16, -8), new(16, -8)),
            ],
            [L.SbaSpazerDown] =
            [
                new(new(0, 0), new(0, 0)), new(new(-8, -8), new(8, -8)), new(new(-16, -8), new(16, -8)),
            ],
        }.ToFrozenDictionary();
}
