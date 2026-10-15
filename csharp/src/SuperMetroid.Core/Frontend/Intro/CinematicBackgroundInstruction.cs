namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Instructions of the intro's bank-$8C cinematic background-object lists.</summary>
internal enum CinematicBackgroundInstruction : ushort
{
    /// <summary><c>CinematicBGObject_Instruction_Delete</c> at $8B:9698.</summary>
    Delete = 0x9698,

    /// <summary><c>CinematicBGObject_Instruction_GotoY</c> at $8B:971E.</summary>
    Goto = 0x971e,

    /// <summary><c>Instruction_SetCaretToBlink</c> at $8B:ADD4.</summary>
    SetCaretToBlink = 0xadd4,

    /// <summary><c>Instruction_HandleCreatingSubtitle_Page1</c> at $8B:AE43.</summary>
    BeginPage1 = 0xae43,

    /// <summary><c>Instruction_SpawnBlinkingMarkers_WaitForInput_Page1</c> at $8B:AE5B.</summary>
    FinishPage1 = 0xae5b,

    /// <summary><c>Instruction_HandleCreatingSubtitle_Page2</c> at $8B:AE79.</summary>
    BeginPage2 = 0xae79,

    /// <summary><c>Instruction_SpawnBlinkingMarkers_WaitForInput_Page2</c> at $8B:AE91.</summary>
    FinishPage2 = 0xae91,

    /// <summary><c>Instruction_HandleCreatingSubtitle_Page3</c> at $8B:B074.</summary>
    BeginPage3 = 0xb074,

    /// <summary><c>Instruction_SpawnBlinkingMarkers_WaitForInput_Page3</c> at $8B:B08C.</summary>
    FinishPage3 = 0xb08c,

    /// <summary><c>Instruction_HandleCreatingSubtitle_Page4</c> at $8B:B0B3.</summary>
    BeginPage4 = 0xb0b3,

    /// <summary><c>Instruction_SpawnBlinkingMarkers_WaitForInput_Page4</c> at $8B:B0CB.</summary>
    FinishPage4 = 0xb0cb,

    /// <summary><c>Instruction_HandleCreatingSubtitle_Page5</c> at $8B:B19B.</summary>
    BeginPage5 = 0xb19b,

    /// <summary><c>Instruction_SpawnBlinkingMarkers_WaitForInput_Page5</c> at $8B:B1B3.</summary>
    FinishPage5 = 0xb1b3,

    /// <summary><c>Instruction_HandleCreatingSubtitle_Page6</c> at $8B:B228.</summary>
    BeginPage6 = 0xb228,

    /// <summary><c>Instruction_FinishIntro</c> at $8B:B240.</summary>
    FinishIntro = 0xb240,
}
