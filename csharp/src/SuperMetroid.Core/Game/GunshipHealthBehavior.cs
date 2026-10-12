using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>The gunship health species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class GunshipHealthBehavior
{
    private readonly EnemySlotTable _slotTable;

    internal GunshipHealthBehavior(EnemySlotTable slotTable)
    {
        _slotTable = slotTable;
    }

    /// <summary>
    /// Gunship command $1A installs $90:E902, which checks low health despite locked
    /// input. Derive its lifetime from the entry/exit actor rather than inventing a
    /// second saved handler flag. The initial post-Ceres landing does not use it.
    /// </summary>
    public bool HasGunshipHealthHandler
    {
        get
        {
            for (int i = 0; i < _slotTable.EnemyCount; i++)
            {
                var slot = _slotTable._slots[i];
                if ((((int)new SnesAddress(slot.Definition.Bank, slot.Definition.InitializationAiPointer))) != (int)EnemyAiRoutine.InitAI_ShipTop) continue;
                return (GunshipFunction)slot.VariableF is GunshipFunction.WaitForEntranceToOpen or GunshipFunction.LowerSamus
                    or GunshipFunction.WaitForEntranceToClose or GunshipFunction.BeginLiftoffOrRestoreSamus
                    or GunshipFunction.HandleSaveConfirmation or GunshipFunction.WaitForExitPadToOpen
                    or GunshipFunction.RaiseSamus or GunshipFunction.FinishSamusExit
                    or GunshipFunction.LoadLiftoffDustTiles or GunshipFunction.FireUpEngines
                    or GunshipFunction.SteadyLiftoff or GunshipFunction.AcceleratingLiftoff
                    or GunshipFunction.MoveAccelerating;
            }
            return false;
        }
    }
}
