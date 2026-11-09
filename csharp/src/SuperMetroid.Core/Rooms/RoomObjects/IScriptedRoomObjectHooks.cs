namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// What a scripted room program calls on the families that run it (#1395): the pre-instructions its cues install. The
/// room composition implements it over those families.
/// </summary>
internal interface IScriptedRoomObjectHooks
{
    /// <summary>The pre-instruction routine a scripted cue installs.</summary>
    IRoomObjectFrameBehavior FrameBehavior(ScriptedRoomObjectFrameBehavior frameBehavior);
}
