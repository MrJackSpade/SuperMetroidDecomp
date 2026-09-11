/// <summary>Pinned bank-$82 operands and tables used as the independent layout reference.</summary>
internal static class OptionsCursorFixtureData
{
    /// <summary>Bank of the options-menu actors and their spritemaps.</summary>
    public const int MenuBank = 0x820000;
    /// <summary>$82:F31B, controller row screen-space X/Y pairs used by the selector pre-instruction.</summary>
    public const int ControllerPositions = 0x82f31b;
    /// <summary>$82:F48E, controller heading's first duration/spritemap record.</summary>
    public const int ControllerBorderList = 0x82f48e;
    /// <summary>$82:F354, immediate X operand in controller-heading setup.</summary>
    public const int ControllerBorderX = 0x82f354;
    /// <summary>$82:F36A, immediate Y operand in shared heading setup.</summary>
    public const int BorderY = 0x82f36a;
    /// <summary>$82:F370, shared menu OBJ palette operand.</summary>
    public const int Palette = 0x82f370;
    /// <summary>$82:F2E1, off-screen X operand for null selector-table phases.</summary>
    public const int HiddenX = 0x82f2e1;
    /// <summary>$82:F2E7, Y operand paired with the hidden selector X.</summary>
    public const int HiddenY = 0x82f2e7;
    /// <summary>$82:F442, four duration/spritemap records for the missile selector.</summary>
    public const int MissileAnimation = 0x82f442;
}
