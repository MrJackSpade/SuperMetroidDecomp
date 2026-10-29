using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;
using static SuperMetroid.AssetExtraction.Samus.ProjectileSpritePartDefinitions;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Which parts each projectile sprite draws: the phase or pose an identity selects and the parts of each beam and wave pose.</summary>
public static class ProjectileSpriteParts
{
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

    /// <summary>$93:A24D..A27E: eight centered one-tile Power poses (also the first four Ice poses at $93:EDF6) traverse a triangular three-glyph cycle and rotate its horizontal/vertical reflection phases.</summary>
    internal static readonly IReadOnlyList<IReadOnlyList<CompiledSpritePart>> SingleBeamPoses =
        [.. Enumerable.Range(0, 8).Select(phase => (IReadOnlyList<CompiledSpritePart>)[SingleBeamPart(phase)])];

    /// <summary>Selects the cyclic glyph and reflection flags for a pose's one centered beam cell.</summary>
    private static CompiledSpritePart SingleBeamPart(int phase)
    {
        int tile = PowerTile + 2 - Math.Abs(2 - (phase & 3));
        var flips = (phase is >= 3 and <= 5 ? TileFlip.Horizontal : 0) |
            (phase >= 4 ? TileFlip.Vertical : 0);
        return new(SpritePart.Create(Offset16<ScreenPixels>.Of(-8 / 2, -8 / 2), false,
            tile, PowerPalette, PowerPriority, flips), false);
    }

    /// <summary>Recognizes one of the centered or displaced single-cell Wave compositions.</summary>
    internal static bool TryWavePhase(ProjectileSpriteId id, out int phase)
    {
        phase = id.Index;
        return id.Group == ProjectileSpriteGroup.Wave;
    }

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
            Offset16<RoomPixels> lobeDirection = direction switch
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
            int distance = SuperMetroid.Core.Samus.Projectiles.ProjectileWaveEnvelope.AxialLobeDistance(step);
            if (lobeDirection.X != 0 && lobeDirection.Y != 0) distance = distance * WaveDiagonalNumerator / WaveDiagonalDenominator;
            x += lobeDirection.X * distance;
            y += lobeDirection.Y * distance;
            tile += (step + 1) / 2;
        }
        return new(SpritePart.Create(Offset16<ScreenPixels>.Of(x, y), false,
            tile, PowerPalette, PowerPriority, 0), false);
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

    /// <summary>Vertical charged Spazer spreads: the four compositions after the horizontal group and the vertical seed and first spread.</summary>
    internal static bool TryVerticalChargedSpazerSpread(ProjectileSpriteId id, out int phase)
    {
        phase = id.Index - 8;
        return id.Group == ProjectileSpriteGroup.ChargedSpazer && phase is >= 0 and < 4;
    }

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

    /// <summary>The horizontal uncharged PlasmaWave core and its four lobes.</summary>
    internal static bool TryHorizontalPlasmaWaveShort(ProjectileSpriteId id, out int phase)
    {
        phase = id.Index;
        return id.Group == ProjectileSpriteGroup.PlasmaWave && id.Index < 5;
    }
}
