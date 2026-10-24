using SuperMetroid.Diagnostics.Configuration;

namespace SuperMetroid.Diagnostics.Android;

/// <summary>Supported Android controls for the shared host INI, not separate cheat implementations.</summary>
internal sealed record AndroidSettingDefinition(string Label, string Section, string Key,
    string[] Values, Func<HostOptions, string> Read);
