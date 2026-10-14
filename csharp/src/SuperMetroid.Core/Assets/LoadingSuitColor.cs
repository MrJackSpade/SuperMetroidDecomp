namespace SuperMetroid.Core.Assets;

/// <summary>Native color-word identities for independently authored Samus loading-palette inputs.</summary>
internal enum LoadingSuitColor : ushort
{
    /// <summary>Native $8D:DC7E color word, PowerDim1 input to Samus loading palette reconstruction.</summary>
    PowerDim1 = 0xdc7e,
    /// <summary>Native $8D:DC80 color word, PowerDim2 input to Samus loading palette reconstruction.</summary>
    PowerDim2 = 0xdc80,
    /// <summary>Native $8D:DBA1 color word, PowerBright9 input to Samus loading palette reconstruction.</summary>
    PowerBright9 = 0xdba1,
    /// <summary>Native $8D:DC94 color word, PowerDim12 input to Samus loading palette reconstruction.</summary>
    PowerDim12 = 0xdc94,
    /// <summary>Native $8D:DD09 color word, VariaBright10 input to Samus loading palette reconstruction.</summary>
    VariaBright10 = 0xdd09,
    /// <summary>Native $8D:DD0B color word, VariaBright11 input to Samus loading palette reconstruction.</summary>
    VariaBright11 = 0xdd0b,
    /// <summary>Native $8D:DDFA color word, VariaDim12 input to Samus loading palette reconstruction.</summary>
    VariaDim12 = 0xddfa,
    /// <summary>Native $8D:DF4C color word, GravityDim2 input to Samus loading palette reconstruction.</summary>
    GravityDim2 = 0xdf4c,
}
