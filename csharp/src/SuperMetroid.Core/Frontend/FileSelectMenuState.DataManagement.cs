using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Frontend;

/// <summary>Selects which file-management flow receives the current file-selection input.</summary>
internal enum FileSelectDataMode
{
    /// <summary>Copies an occupied save slot into a chosen destination slot.</summary>
    Copy,
    /// <summary>Clears the chosen save slot after its confirmation flow.</summary>
    Clear,
}
