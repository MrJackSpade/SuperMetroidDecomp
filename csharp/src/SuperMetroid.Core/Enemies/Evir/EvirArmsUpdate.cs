namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>Which update positions Evir's arms.</summary>
internal enum EvirArmsUpdate
{
    /// <summary>An ordinary main-AI update.</summary>
    Main,

    /// <summary>Initialization, before the body is fully linked.</summary>
    Initialization,
}
