namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Which way a glass shard projectile flies: its bank-$86 spawn parameter (#1296).</summary>
internal enum MotherBrainGlassShard : ushort
{
    /// <summary>Flies right.</summary>
    Right,
    /// <summary>Flies left.</summary>
    Left,
    /// <summary>Falls from the centre.</summary>
    Centre,
}
