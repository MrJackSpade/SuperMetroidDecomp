// #467: run after native-release-probe.h, dispatch before SDL. Disposable SRAM only.
int DiagnosticMoonwalkOptions(const char *rom) {
  int status = ProbeLoadRetailMovementRom(rom);
  if (status) return status;
  for (int initial = 0; initial < 2; initial++)
  for (int abandon = 0; abandon < 2; abandon++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    memset(g_sram, 0, 0x2000);
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    samus_health = samus_max_health = 99; moonwalk_flag = initial;
    loading_game_state = 5;
    RunAsmCode(0x818000, 0, 0, 0, 0); // SaveToSRAM: actual CPU, no host SRAM write.
    RunAsmCode(0x818085, 0, 0, 0, 0); // File selection reloads the saved option first.
    menu_option_index = 1; joypad1_newkeys = 0x100;
    RunAsmCode(0x82f024, 0, 0, 0, 0); // Right input on the Moonwalk row.
    if (moonwalk_flag != !initial) return 5;
    if (abandon) RunAsmCode(0x818085, 0, 0, 0, 0); // Reselecting the file discards edits.
    joypad1_lastkeys = joypad1_newkeys = 0;
    RunAsmCode(0x82eeb4, 0, 0, 0, 0); // Continue preserves live WRAM, not a second SRAM load.
    int expected = abandon ? initial : !initial;
    if (moonwalk_flag != expected || game_state != 5) return 6;
    RunAsmCode(0x818000, 0, 0, 0, 0);
    moonwalk_flag = 7;
    RunAsmCode(0x818085, 0, 0, 0, 0);
    if (moonwalk_flag != expected) return 7;
    printf("MOONWALK-OPTIONS initial=%d abandon=%d continued-and-saved=%d\n", initial, abandon, moonwalk_flag);
  }
  return 0;
}
