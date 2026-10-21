using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// What a scripted room program calls on the families that run it (#1395): the pre-instructions its cues install and the
/// Lower Norfair Chozo statue's acid and terrain changes. The room composition implements it over those families.
/// </summary>
internal interface IScriptedRoomPlmHooks
{
    /// <summary>The pre-instruction routine a scripted cue installs.</summary>
    IPlmPreInstruction PreInstruction(ScriptedRoomPlmPreInstruction preInstruction);

    /// <summary>Whether the Lower Norfair Chozo statue has already lowered the acid.</summary>
    bool HasLoweredLowerNorfairAcid { get; }

    /// <summary>Lowers the Lower Norfair acid to the statue's resting height.</summary>
    void SetLoweredAcidHeight();

    /// <summary>Turns the statue room's spikes into slopes.</summary>
    void TransformSpikesToSlopes(RoomLevelData level);

    /// <summary>Turns the statue room's slopes back into spikes.</summary>
    void RevertSlopesToSpikes(RoomLevelData level);
}
