namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    /// <summary>
    /// Gunship command $1A installs $90:E902, which checks low health despite locked
    /// input. Derive its lifetime from the entry/exit actor rather than inventing a
    /// second saved handler flag. The initial post-Ceres landing does not use it.
    /// </summary>
    public bool HasGunshipHealthHandler
    {
        get
        {
            for (int i = 0; i < EnemyCount; i++)
            {
                var slot = _slots[i];
                if (((slot.Definition.Bank << 16) | slot.Definition.InitializationAiPointer) != EnemyAiCodePointers.InitAI_ShipTop) continue;
                return slot.VariableF is GunshipCodePointers.WaitForEntranceToOpen or GunshipCodePointers.LowerSamus
                    or GunshipCodePointers.WaitForEntranceToClose or GunshipCodePointers.BeginLiftoffOrRestoreSamus
                    or GunshipCodePointers.HandleSaveConfirmation or GunshipCodePointers.WaitForExitPadToOpen
                    or GunshipCodePointers.RaiseSamus or GunshipCodePointers.FinishSamusExit
                    or GunshipCodePointers.LoadLiftoffDustTiles or GunshipCodePointers.FireUpEngines
                    or GunshipCodePointers.SteadyLiftoff or GunshipCodePointers.AcceleratingLiftoff
                    or GunshipCodePointers.MoveAccelerating;
            }
            return false;
        }
    }
}
