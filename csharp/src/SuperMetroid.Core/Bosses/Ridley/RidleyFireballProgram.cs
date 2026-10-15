namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Ridley's typed fireball and afterburn programs (#641), in the authored order of their bank-$86 lists.</summary>
internal enum RidleyFireballProgram
{
    /// <summary>The launching fireball (<c>InstList_EnemyProjectile_RidleysFireball_0</c>, $86:9552).</summary>
    Fireball,
    /// <summary>The flying fireball's loop (<c>InstList_EnemyProjectile_RidleysFireball_1</c>, $86:9560).</summary>
    FireballLoop,
    /// <summary>A directional afterburn's dying puff after it hits a wall (<c>InstList_EnemyProjectile_Afterburn_Final</c>, $86:9574).</summary>
    AfterburnFinal,
    /// <summary>The impact center that emits a horizontal pair (<c>InstList_EnemyProjectile_HorizontalAfterburn_Center</c>, $86:95A0).</summary>
    HorizontalCenter,
    /// <summary>The impact center that emits a vertical pair (<c>InstList_EnemyProjectile_VerticalAfterburn_Center</c>, $86:95D3).</summary>
    VerticalCenter,
    /// <summary>A directional afterburn that extends its chain (<c>InstList_EnemyProjectile_Afterburn</c>, $86:9606).</summary>
    DirectionalAfterburn,
}
