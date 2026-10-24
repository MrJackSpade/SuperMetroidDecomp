using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>How each beam, missile and effect sprite is composed: its animation phase or pose and the parts that phase draws.</summary>
public static class ProjectileSpritePartDefinitions
{
    /// <summary>Native order of the eight Wave-beam displacement groups.</summary>
    private enum WaveDirection
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
    private const int PowerTile = 0x30;

    /// <summary>$93:A252: Power-beam poses select OBJ tile $30..32 with priority2; those artwork choices remain required, while palette selects the installed CGRAM row.</summary>
    internal const int PowerPriority = 2;

    /// <summary>$90:ACDE-ACE8 loads the active beam colors at CGRAM224. OBJ attributes select a sixteen-color row in CGRAM's upper half, so its palette field derives from that destination.</summary>
    internal const int PowerPalette = (SamusProjectileRomData.Palettes.BeamFirstColour - CgramPalettes.ColorCount / 2) /
        GameplayBasePaletteFormat.SpriteColorCount;

    /// <summary>$82:E13E-E148 restores9A:81A0 at SpriteP5/CGRAM208. Fixed projectile artwork selects that installed sixteen-color row; its color payload remains required.</summary>
    internal const int FixedProjectilePalette = (GameplayBasePaletteFormat.EnemyProjectileInitialColor - CgramPalettes.ColorCount / 2) /
        GameplayBasePaletteFormat.SpriteColorCount;

    /// <summary>Recognizes one-cell Power or Ice compositions and their cycle phase.</summary>
    internal static bool TrySingleBeamPhase(ProjectileSpriteId id, out int phase)
    {
        phase = id.Index;
        return id.Group is ProjectileSpriteGroup.Power or ProjectileSpriteGroup.Ice;
    }

    /// <summary>Recognizes charged Power and Ice quad compositions while reserving Power's independently selected first record.</summary>
    internal static bool TryChargedBeamPhase(ProjectileSpriteId id, out int phase, out bool ice)
    {
        phase = id.Index;
        ice = id.Group == ProjectileSpriteGroup.ChargedIce;
        // Charged Power's first composition keeps its independently selected ordering.
        return ice || (id.Group == ProjectileSpriteGroup.ChargedPower && id.Index != 0);
    }

    /// <summary>Axial SuperMissile poses: every other composition, each followed by a three-part diagonal one.</summary>
    internal static bool TryAxialSuperMissilePose(ProjectileSpriteId id, out int pose)
    {
        pose = id.Index / 2;
        return id.Group == ProjectileSpriteGroup.SuperMissile && id.Index % 2 == 0;
    }

    /// <summary>$93:ADDC/ADF9: independently selected horizontal/vertical SuperMissile glyph roots and priority. These artwork inputs remain required under frames; palette selects the installed fixed-projectile CGRAM row.</summary>
    internal const int SuperMissileHorizontalGlyph = 0x65;

    /// <summary>$93:ADDC/ADF9: independently selected horizontal/vertical SuperMissile glyph roots and priority. These artwork inputs remain required under frames; palette selects the installed fixed-projectile CGRAM row.</summary>
    internal const int SuperMissileVerticalGlyph = 0x69;

    /// <summary>$93:ADDC/ADF9: independently selected horizontal/vertical SuperMissile glyph roots and priority. These artwork inputs remain required under frames; palette selects the installed fixed-projectile CGRAM row.</summary>
    internal const int SuperMissilePriority = 2;

    /// <summary>$93:A24D..A27E: eight centered one-tile Power poses (also the first four Ice poses at $93:EDF6) traverse a triangular three-glyph cycle and rotate its horizontal/vertical reflection phases.</summary>
    internal static readonly IReadOnlyList<IReadOnlyList<CompiledSpritePart>> SingleBeamPoses =
        [.. Enumerable.Range(0, 8).Select(phase => (IReadOnlyList<CompiledSpritePart>)[SingleBeamPart(phase)])];

    /// <summary>Selects the cyclic glyph and reflection flags for a pose's one centered beam cell.</summary>
    private static CompiledSpritePart SingleBeamPart(int phase)
    {
        int tile = PowerTile + 2 - Math.Abs(2 - (phase & 3));
        var flips = (phase is >= 3 and <= 5 ? SnesTileFlipFlags.Horizontal : 0) |
            (phase >= 4 ? SnesTileFlipFlags.Vertical : 0);
        return new(SnesSpritemapXWord.Create(-8 / 2, false), SnesSpritemapYByte.Create(-8 / 2),
            SnesObjAttributeWord.Create(tile, PowerPalette, PowerPriority, flips), false);
    }

    /// <summary>$93:EC43/EDA3: charged Power/Ice use independently selected OBJ glyphs $33/$34, sharing palette6/priority2 with the single-tile beam.</summary>
    internal const int ChargedBeamTile = 0x33;

    /// <summary>Recognizes one of the centered or displaced single-cell Wave compositions.</summary>
    internal static bool TryWavePhase(ProjectileSpriteId id, out int phase)
    {
        phase = id.Index;
        return id.Group == ProjectileSpriteGroup.Wave;
    }

    /// <summary>$93:AEA4..AEC0 and other diagonal groups: component magnitudes6/9/11/12 equal floor(3*axial/4). This selected ratio remains required, not a trigonometric assertion.</summary>
    internal const int WaveDiagonalNumerator = 3;

    /// <summary>$93:AEA4..AEC0 and other diagonal groups: component magnitudes6/9/11/12 equal floor(3*axial/4). This selected ratio remains required, not a trigonometric assertion.</summary>
    internal const int WaveDiagonalDenominator = 4;

    /// <summary>$93:AE65..AF4B: one centered pose then four stages in each of eight signed directional displacement groups.</summary>
    internal static readonly IReadOnlyList<IReadOnlyList<CompiledSpritePart>> WavePoses =
        [.. Enumerable.Range(0, 33).Select(phase => (IReadOnlyList<CompiledSpritePart>)[WavePart(phase)])];

    /// <summary>Places a pose's one Wave glyph at the required axial or diagonal travel distance.</summary>
    private static CompiledSpritePart WavePart(int phase)
    {
        int x = -8 / 2, y = -8 / 2, tile = PowerTile;
        if (phase != 0)
        {
            int step = (phase - 1) % 4;
            WaveDirection direction = ((phase - 1) / 4) switch
            {
                0 => WaveDirection.Up,
                1 => WaveDirection.Down,
                2 => WaveDirection.UpRight,
                3 => WaveDirection.DownLeft,
                4 => WaveDirection.Right,
                5 => WaveDirection.Left,
                6 => WaveDirection.UpLeft,
                7 => WaveDirection.DownRight,
                _ => throw new ArgumentOutOfRangeException(nameof(phase)),
            };
            var (dx, dy) = direction switch
            {
                WaveDirection.Up => new Offset16<RoomPixels>(0, -1),
                WaveDirection.Down => new Offset16<RoomPixels>(0, 1),
                WaveDirection.UpRight => new Offset16<RoomPixels>(1, -1),
                WaveDirection.DownLeft => new Offset16<RoomPixels>(-1, 1),
                WaveDirection.Right => new Offset16<RoomPixels>(1, 0),
                WaveDirection.Left => new Offset16<RoomPixels>(-1, 0),
                WaveDirection.UpLeft => new Offset16<RoomPixels>(-1, -1),
                WaveDirection.DownRight => new Offset16<RoomPixels>(1, 1),
                _ => throw new ArgumentOutOfRangeException(nameof(phase)),
            };
            int distance = SuperMetroid.Core.Samus.Projectiles.ProjectileWaveEnvelopeDefinitions.AxialLobeDistance(step);
            if (dx != 0 && dy != 0) distance = distance * WaveDiagonalNumerator / WaveDiagonalDenominator;
            x += dx * distance;
            y += dy * distance;
            tile += (step + 1) / 2;
        }
        return new(SnesSpritemapXWord.Create(x, false), SnesSpritemapYByte.Create(y),
            SnesObjAttributeWord.Create(tile, PowerPalette, PowerPriority, 0), false);
    }

    /// <summary>Recognizes charged Wave or IceWave vertical compositions: two cores, then eight paired-lobe phases.</summary>
    internal static bool TryVerticalChargedWavePhase(ProjectileSpriteId id, out int phase)
    {
        phase = id.Index;
        return id.Group is ProjectileSpriteGroup.ChargedWave or ProjectileSpriteGroup.ChargedIceWave && id.Index < 10;
    }

    /// <summary>Diagonal missile and SuperMissile poses: the three-part composition after each two-part axial one.</summary>
    internal static bool TryDiagonalMissilePose(ProjectileSpriteId id, out int pose, out bool super)
    {
        pose = id.Index / 2;
        super = id.Group == ProjectileSpriteGroup.SuperMissile;
        return id.Group is ProjectileSpriteGroup.Missile or ProjectileSpriteGroup.SuperMissile && id.Index % 2 == 1;
    }

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

    /// <summary>Bomb poses, or the four four-quadrant beam-explosion stages after its two single-tile cores.</summary>
    internal static bool TrySimpleEffectPose(ProjectileSpriteId id, out int phase, out bool quad)
    {
        quad = id.Group == ProjectileSpriteGroup.BeamExplosion;
        phase = quad ? id.Index - 2 : id.Index;
        return id.Group == ProjectileSpriteGroup.Bomb || (quad && id.Index >= 2);
    }

    /// <summary>Horizontal charged Wave or IceWave paired lobes: the eight compositions after the twenty-six vertical and diagonal ones.</summary>
    internal static bool TryHorizontalChargedWavePhase(ProjectileSpriteId id, out int phase)
    {
        phase = id.Index - 26;
        return id.Group is ProjectileSpriteGroup.ChargedWave or ProjectileSpriteGroup.ChargedIceWave && phase >= 0;
    }

    /// <summary>Each Spazer orientation's seed composition: four large diagonal ones, then four small axial ones.</summary>
    internal static bool TrySpazerSeedPose(ProjectileSpriteId id, out int pose)
    {
        pose = Array.IndexOf(SpazerSeeds, id.Index);
        return id.Group == ProjectileSpriteGroup.Spazer && pose >= 0;
    }

    /// <summary>Where each Spazer orientation's seed composition sits; the third diagonal orientation lacks its last spread.</summary>
    private static readonly int[] SpazerSeeds = [0, 6, 12, 17, 23, 29, 35, 41];

    /// <summary>$93:D113/D118 and D643/D6EF: selected diagonal32/31,vertical33,horizontal30 atlas cells remain REQUIRED artwork identities.</summary>
    internal const int SpazerDiagonalGlyph = 0x32;

    /// <summary>$93:D113/D118 and D643/D6EF: selected diagonal32/31,vertical33,horizontal30 atlas cells remain REQUIRED artwork identities.</summary>
    internal const int SpazerVerticalGlyph = 0x33;

    /// <summary>$93:D113/D118 and D643/D6EF: selected diagonal32/31,vertical33,horizontal30 atlas cells remain REQUIRED artwork identities.</summary>
    internal const int SpazerHorizontalGlyph = 0x30;

    /// <summary>The first four three-lane spreads after each diagonal Spazer seed.</summary>
    internal static bool TrySpazerDiagonalSpread(ProjectileSpriteId id, out int pose, out int phase)
    {
        for (pose = 0; pose < 4; pose++)
        {
            phase = id.Index - SpazerSeeds[pose] - 1;
            if (id.Group == ProjectileSpriteGroup.Spazer && phase is >= 0 and < 4) return true;
        }
        phase = 0;
        return false;
    }

    /// <summary>The five three-lane spreads after each axial Spazer seed, except the last orientation's first.</summary>
    internal static bool TrySpazerAxialSpread(ProjectileSpriteId id, out int pose, out int phase)
    {
        for (pose = 4; pose < SpazerSeeds.Length; pose++)
        {
            phase = id.Index - SpazerSeeds[pose] - 1;
            if (id.Group == ProjectileSpriteGroup.Spazer && phase is >= 0 and < 5 && !(pose == 7 && phase == 0)) return true;
        }
        phase = 0;
        return false;
    }

    /// <summary>Horizontal charged Spazer: its core and five spreads.</summary>
    internal static bool TryHorizontalChargedSpazer(ProjectileSpriteId id, out int phase)
    {
        phase = id.Index;
        return id.Group == ProjectileSpriteGroup.ChargedSpazer && id.Index < 6;
    }

    /// <summary>$93:D8EE: the chosen four-cell length and tile34 artwork remain REQUIRED inputs; cell adjacency and centering calculate.</summary>
    internal const int ChargedSpazerHorizontalCells = 4;

    /// <summary>$93:D8EE: the chosen four-cell length and tile34 artwork remain REQUIRED inputs; cell adjacency and centering calculate.</summary>
    internal const int ChargedSpazerHorizontalGlyph = 0x34;

    /// <summary>Vertical charged Spazer spreads: the four compositions after the horizontal group and the vertical seed and first spread.</summary>
    internal static bool TryVerticalChargedSpazerSpread(ProjectileSpriteId id, out int phase)
    {
        phase = id.Index - 8;
        return id.Group == ProjectileSpriteGroup.ChargedSpazer && phase is >= 0 and < 4;
    }

    /// <summary>$93:DA8E: the selected four-cell column and glyph37 remain REQUIRED artwork inputs.</summary>
    internal const int ChargedSpazerVerticalCells = 4;

    /// <summary>$93:DA8E: the selected four-cell column and glyph37 remain REQUIRED artwork inputs.</summary>
    internal const int ChargedSpazerVerticalGlyph = 0x37;

    /// <summary>Where each axial Spazer startup orientation's one-strip composition sits; its two-strip one follows.</summary>
    private static readonly int[] SpazerAxialStartups = [0, 4, 8, 12, 14, 18];

    /// <summary>Where each diagonal Spazer startup orientation's one-pair composition sits; its two-pair one follows.</summary>
    private static readonly int[] SpazerDiagonalStartups = [2, 6, 10];

    /// <summary>Where the falling and rising alternate diagonal startups begin; each has two compositions.</summary>
    private static readonly int[] AlternateDiagonalStartups = [16, 20];

    /// <summary>The one- and two-strip startup compositions of each axial Spazer orientation.</summary>
    internal static bool TrySpazerAxialStartup(ProjectileSpriteId id, out int group, out int length)
    {
        for (group = 0; group < SpazerAxialStartups.Length; group++)
        {
            length = id.Index - SpazerAxialStartups[group] + 1;
            if (id.Group == ProjectileSpriteGroup.SpazerStartup && length is 1 or 2) return true;
        }
        length = 0;
        return false;
    }

    /// <summary>The one- and two-pair startup compositions of each diagonal Spazer orientation.</summary>
    internal static bool TrySpazerDiagonalStartup(ProjectileSpriteId id, out int group, out int pairs)
    {
        for (group = 0; group < SpazerDiagonalStartups.Length; group++)
        {
            pairs = id.Index - SpazerDiagonalStartups[group] + 1;
            if (id.Group == ProjectileSpriteGroup.SpazerStartup && pairs is 1 or 2) return true;
        }
        pairs = 0;
        return false;
    }

    /// <summary>The core composition that begins each of charged Plasma's eight startup orientation groups.</summary>
    internal static bool TryPlasmaStartupCore(ProjectileSpriteId id, out int group)
    {
        group = id.Index / 4;
        return id.Group == ProjectileSpriteGroup.PlasmaStartup && id.Index % 4 == 0;
    }

    /// <summary>The two compositions of the falling (unreflected) and rising (reflected) alternate diagonal startups.</summary>
    internal static bool TryAlternateDiagonalStartup(ProjectileSpriteId id, out bool reflected, out int phase)
    {
        for (int variant = 0; variant < AlternateDiagonalStartups.Length; variant++)
        {
            phase = id.Index - AlternateDiagonalStartups[variant];
            reflected = variant == 1;
            if (id.Group == ProjectileSpriteGroup.SpazerStartup && phase is 0 or 1) return true;
        }
        reflected = false;
        phase = 0;
        return false;
    }

    /// <summary>$93:F048: chosen inner/endcap glyphs35/36 remain REQUIRED artwork inputs.</summary>
    internal const int SpazerDiagonalInnerGlyph = 0x35;

    /// <summary>$93:F048: chosen inner/endcap glyphs35/36 remain REQUIRED artwork inputs.</summary>
    internal const int SpazerDiagonalEndcapGlyph = 0x36;

    /// <summary>The horizontal uncharged PlasmaWave core and its four lobes.</summary>
    internal static bool TryHorizontalPlasmaWaveShort(ProjectileSpriteId id, out int phase)
    {
        phase = id.Index;
        return id.Group == ProjectileSpriteGroup.PlasmaWave && id.Index < 5;
    }

    /// <summary>$93:BC0A: the chosen four-cell short Plasma footprint remains REQUIRED artwork input.</summary>
    internal const int PlasmaWaveShortCells = 4;
}
