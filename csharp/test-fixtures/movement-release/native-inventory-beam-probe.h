// #395: execute retail CPU instructions, not upstream C's explicit VAR-bug fix.
// Include after native-release-probe.h in sm_rtl.c; invoke before SDL startup.
int DiagnosticInventoryBeams(const char *rom) {
  int status = ProbeLoadRetailMovementRom(rom);
  if (status) return status;
  for (int scenario = 0; scenario < 3; scenario++) {
    cpu_reset(g_snes->cpu);
    memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false;
    g_snes->cpu->sp = 0x1ff0;
    g_snes->cpu->dp = 0;
    equipped_beams = 4;
    collected_beams = 0x100f;
    collected_items = equipped_items = 0x3300;
    pausemenu_equipment_category_item = 3;
    // Distinguish a nine-word erroneous boot-category copy from a five-word beam copy.
    memset(g_ram + 0x3800, 0x55, 0x800);
    joypad1_newkeys = scenario == 0 ? 0x280 : 0x200;
    RunAsmCode(0x82b150, 0, 0, 0, 0);
    if (scenario == 1) {
      joypad1_newkeys = 0x80;
      RunAsmCode(0x82afbe, 0, 0, 0, 0);
    }
    printf("INVENTORY scenario=%d selector=%04X beams=%04X items=%04X\n",
      scenario, pausemenu_equipment_category_item, equipped_beams, equipped_items);
    for (int offset = 0; offset < 0x800; offset += 2) {
      unsigned int word = g_ram[0x3800 + offset] | g_ram[0x3801 + offset] << 8;
      if (word != 0x5555) printf(" tile=%04X:%04X", 0x3800 + offset, word);
    }
    puts("");
  }
  return 0;
}
