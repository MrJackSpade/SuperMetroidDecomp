namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>
/// The independently authored Samus loading-palette inputs: tint endpoints whose chosen channels are paint while the
/// rest follow a normal suit color.
/// </summary>
internal enum LoadingSuitColor
{
    /// <summary>Power dim row, slot 1.</summary>
    PowerDim1 = 1,
    /// <summary>Power dim row, slot 2.</summary>
    PowerDim2,
    /// <summary>Power bright row, slot 9.</summary>
    PowerBright9,
    /// <summary>Power dim row, slot 12.</summary>
    PowerDim12,
    /// <summary>Varia bright row, slot 10.</summary>
    VariaBright10,
    /// <summary>Varia bright row, slot 11.</summary>
    VariaBright11,
    /// <summary>Varia dim row, slot 12.</summary>
    VariaDim12,
    /// <summary>Gravity dim row, slot 2.</summary>
    GravityDim2,
}
