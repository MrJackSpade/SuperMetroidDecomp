// #441: execute the unpatched cartridge RNG and SRAM load routines. No disk SRAM
// is loaded or written: the save is constructed in the emulated cartridge RAM.
// Include after native-release-probe.h and dispatch before host/SDL startup.
int DiagnosticSaveLoadRandom(const char *rom) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
  g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
  samus_health = samus_max_health = 399;
  samus_power_bombs = samus_max_power_bombs = 5;
  area_index = 5; load_station_index = 0;
  // Original SaveToSram does not call the translated C host's RtlWriteSram.
  RunAsmCode(0x818000, 0, 0, 0, 0);
  for (int waits = 0; waits < 3; waits++) {
    random_number = 97;
    // State zero also receives the main-loop call after the reset vector seeds.
    for (int frame = 0; frame < 426 + waits; frame++) RunAsmCode(0x808111, 0, 0, 0, 0);
    uint16 before = random_number;
    samus_health = 1; samus_power_bombs = 0;
    RunAsmCode(0x818085, 0, 0, 0, 0);
    printf("SAVE_RNG wait=%d before=%u after=%u health=%u ammo=%u\n",
      waits, before, random_number, samus_health, samus_power_bombs);
    if (random_number != before || samus_health != 399 || samus_power_bombs != 5) return 5;
    RunAsmCode(0x808111, 0, 0, 0, 0);
    printf("FIRST_GAMEPLAY_RNG wait=%d value=%u\n", waits, random_number);
  }
  return 0;
}
