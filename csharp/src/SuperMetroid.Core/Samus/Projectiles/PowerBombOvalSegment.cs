namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The Power Bomb blast's stages that show authored ovals, one oval each update (#2529).</summary>
internal enum PowerBombOvalSegment
{
    /// <summary>The yellow pre-explosion's ovals (<see cref="Abilities.PowerBombExplosionDefinitions.YellowOvalRadii"/>).</summary>
    PreExplosionYellow,

    /// <summary>The white explosion's ovals (<see cref="Abilities.PowerBombExplosionDefinitions.WhiteOvalRadii"/>).</summary>
    ExplosionWhite,
}
