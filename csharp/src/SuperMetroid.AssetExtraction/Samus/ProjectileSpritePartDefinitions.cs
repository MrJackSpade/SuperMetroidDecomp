using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>How each beam, missile and effect sprite is composed: its animation phase or pose and the parts that phase draws.</summary>
public static class ProjectileSpritePartDefinitions
{
    /// <summary>Native order of the eight Wave-beam displacement groups.</summary>
    internal enum WaveDirection
    {
        /// <summary>Upward axial offset.</summary>
        Up,
        /// <summary>Downward axial offset.</summary>
        Down,
        /// <summary>Up-right diagonal offset.</summary>
        UpRight,
        /// <summary>Down-left diagonal offset.</summary>
        DownLeft,
        /// <summary>Rightward axial offset.</summary>
        Right,
        /// <summary>Leftward axial offset.</summary>
        Left,
        /// <summary>Up-left diagonal offset.</summary>
        UpLeft,
        /// <summary>Down-right diagonal offset.</summary>
        DownRight,
    }
    /// <summary>$93:A252: Power-beam poses select OBJ tile $30..32 with priority2; those artwork choices remain required, while palette selects the installed CGRAM row.</summary>
    internal const int PowerTile = 0x30;

    /// <summary>$93:A252: Power-beam poses select OBJ tile $30..32 with priority2; those artwork choices remain required, while palette selects the installed CGRAM row.</summary>
    internal const int PowerPriority = 2;

    /// <summary>$90:ACDE-ACE8 loads the active beam colors at CGRAM224. OBJ attributes select a sixteen-color row in CGRAM's upper half, so its palette field derives from that destination.</summary>
    internal const int PowerPalette = (SamusColourDefinitions.BeamFirstColour - GameplayColours.ColorCount / 2) /
        GameplayBasePaletteFormat.SpriteColorCount;

    /// <summary>$82:E13E-E148 restores9A:81A0 at SpriteP5/CGRAM208. Fixed projectile artwork selects that installed sixteen-color row; its color payload remains required.</summary>
    internal const int FixedProjectilePalette = (GameplayBasePaletteFormat.EnemyProjectileInitialColor - GameplayColours.ColorCount / 2) /
        GameplayBasePaletteFormat.SpriteColorCount;

    /// <summary>$93:ADDC/ADF9: independently selected horizontal/vertical SuperMissile glyph roots and priority. These artwork inputs remain required under frames; palette selects the installed fixed-projectile CGRAM row.</summary>
    internal const int SuperMissileHorizontalGlyph = 0x65;

    /// <summary>$93:ADDC/ADF9: independently selected horizontal/vertical SuperMissile glyph roots and priority. These artwork inputs remain required under frames; palette selects the installed fixed-projectile CGRAM row.</summary>
    internal const int SuperMissileVerticalGlyph = 0x69;

    /// <summary>$93:ADDC/ADF9: independently selected horizontal/vertical SuperMissile glyph roots and priority. These artwork inputs remain required under frames; palette selects the installed fixed-projectile CGRAM row.</summary>
    internal const int SuperMissilePriority = 2;

    /// <summary>$93:EC43/EDA3: charged Power/Ice use independently selected OBJ glyphs $33/$34, sharing palette6/priority2 with the single-tile beam.</summary>
    internal const int ChargedBeamTile = 0x33;

    /// <summary>$93:AEA4..AEC0 and other diagonal groups: component magnitudes6/9/11/12 equal floor(3*axial/4). This selected ratio remains required, not a trigonometric assertion.</summary>
    internal const int WaveDiagonalNumerator = 3;

    /// <summary>$93:AEA4..AEC0 and other diagonal groups: component magnitudes6/9/11/12 equal floor(3*axial/4). This selected ratio remains required, not a trigonometric assertion.</summary>
    internal const int WaveDiagonalDenominator = 4;

    /// <summary>Native AD6F andADE3 L-footprint origins. These four independently selected pivot coordinates remain REQUIRED artwork inputs.</summary>
    internal const int MissileDiagonalX = -8;

    /// <summary>Native AD6F andADE3 L-footprint origins. These four independently selected pivot coordinates remain REQUIRED artwork inputs.</summary>
    internal const int MissileDiagonalY = -11;

    /// <summary>Native AD6F andADE3 L-footprint origins. These four independently selected pivot coordinates remain REQUIRED artwork inputs.</summary>
    internal const int SuperMissileDiagonalX = -6;

    /// <summary>Native AD6F andADE3 L-footprint origins. These four independently selected pivot coordinates remain REQUIRED artwork inputs.</summary>
    internal const int SuperMissileDiagonalY = -10;

    /// <summary>Native AD72/ADF0 glyph roots and AD6F/ADE3 first corner in bottom-right,bottom-left,top-left order; both the chosen three-corner footprint and these artwork/order seeds remain REQUIRED.</summary>
    internal const int MissileDiagonalGlyph = 0x56;

    /// <summary>Native AD72/ADF0 glyph roots and AD6F/ADE3 first corner in bottom-right,bottom-left,top-left order; both the chosen three-corner footprint and these artwork/order seeds remain REQUIRED.</summary>
    internal const int SuperMissileDiagonalGlyph = 0x66;

    /// <summary>Native AD72/ADF0 glyph roots and AD6F/ADE3 first corner in bottom-right,bottom-left,top-left order; both the chosen three-corner footprint and these artwork/order seeds remain REQUIRED.</summary>
    internal const int MissileFirstCorner = 2;

    /// <summary>Native AD72/ADF0 glyph roots and AD6F/ADE3 first corner in bottom-right,bottom-left,top-left order; both the chosen three-corner footprint and these artwork/order seeds remain REQUIRED.</summary>
    internal const int SuperMissileFirstCorner = 0;

    /// <summary>$93:AD45-AD60: four centered single-cell Bomb poses use consecutive glyphs from4C. Glyph origin/priority remain REQUIRED artwork inputs; palette selects the installed fixed-projectile row.</summary>
    internal const int BombGlyphStart = 0x4c;

    /// <summary>$93:AD45-AD60: four centered single-cell Bomb poses use consecutive glyphs from4C. Glyph origin/priority remain REQUIRED artwork inputs; palette selects the installed fixed-projectile row.</summary>
    internal const int BombPalette = FixedProjectilePalette;

    /// <summary>$93:AD45-AD60: four centered single-cell Bomb poses use consecutive glyphs from4C. Glyph origin/priority remain REQUIRED artwork inputs; palette selects the installed fixed-projectile row.</summary>
    internal const int EffectPriority = 3;

    /// <summary>$93:ABC1-AC18: four mirrored quadrant beam-explosion stages use consecutive glyphs60-63 and the installed beam-color row. Glyph selection/priority/pixels remain REQUIRED.</summary>
    internal const int BeamExplosionQuadGlyphStart = 0x60;

    /// <summary>$93:ABC1-AC18: four mirrored quadrant beam-explosion stages use consecutive glyphs60-63 and the installed beam-color row. Glyph selection/priority/pixels remain REQUIRED.</summary>
    internal const int BeamExplosionPalette = PowerPalette;

    /// <summary>Where each Spazer orientation's seed composition sits; the third diagonal orientation lacks its last spread.</summary>
    internal static readonly int[] SpazerSeeds = [0, 6, 12, 17, 23, 29, 35, 41];

    /// <summary>$93:D113/D118 and D643/D6EF: selected diagonal32/31,vertical33,horizontal30 atlas cells remain REQUIRED artwork identities.</summary>
    internal const int SpazerDiagonalGlyph = 0x32;

    /// <summary>$93:D113/D118 and D643/D6EF: selected diagonal32/31,vertical33,horizontal30 atlas cells remain REQUIRED artwork identities.</summary>
    internal const int SpazerVerticalGlyph = 0x33;

    /// <summary>$93:D113/D118 and D643/D6EF: selected diagonal32/31,vertical33,horizontal30 atlas cells remain REQUIRED artwork identities.</summary>
    internal const int SpazerHorizontalGlyph = 0x30;

    /// <summary>$93:D8EE: the chosen four-cell length and tile34 artwork remain REQUIRED inputs; cell adjacency and centering calculate.</summary>
    internal const int ChargedSpazerHorizontalCells = 4;

    /// <summary>$93:D8EE: the chosen four-cell length and tile34 artwork remain REQUIRED inputs; cell adjacency and centering calculate.</summary>
    internal const int ChargedSpazerHorizontalGlyph = 0x34;

    /// <summary>$93:DA8E: the selected four-cell column and glyph37 remain REQUIRED artwork inputs.</summary>
    internal const int ChargedSpazerVerticalCells = 4;

    /// <summary>$93:DA8E: the selected four-cell column and glyph37 remain REQUIRED artwork inputs.</summary>
    internal const int ChargedSpazerVerticalGlyph = 0x37;

    /// <summary>Where each axial Spazer startup orientation's one-strip composition sits; its two-strip one follows.</summary>
    internal static readonly int[] SpazerAxialStartups = [0, 4, 8, 12, 14, 18];

    /// <summary>Where each diagonal Spazer startup orientation's one-pair composition sits; its two-pair one follows.</summary>
    internal static readonly int[] SpazerDiagonalStartups = [2, 6, 10];

    /// <summary>Where the falling and rising alternate diagonal startups begin; each has two compositions.</summary>
    internal static readonly int[] AlternateDiagonalStartups = [16, 20];

    /// <summary>$93:F048: chosen inner/endcap glyphs35/36 remain REQUIRED artwork inputs.</summary>
    internal const int SpazerDiagonalInnerGlyph = 0x35;

    /// <summary>$93:F048: chosen inner/endcap glyphs35/36 remain REQUIRED artwork inputs.</summary>
    internal const int SpazerDiagonalEndcapGlyph = 0x36;

    /// <summary>$93:BC0A: the chosen four-cell short Plasma footprint remains REQUIRED artwork input.</summary>
    internal const int PlasmaWaveShortCells = 4;
}
