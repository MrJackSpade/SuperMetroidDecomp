// #443: execute the original Zebetite embedded-record spawn through the CPU.
// Include after native-release-probe.h; enter before SDL starts.
int DiagnosticZebetiteSlots(const char *rom) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  for (int hole = 0; hole < 4; hole++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0;
    for (int i = 0; i < 4; i++) if (i != hole) gEnemyData(i * 64)->enemy_ptr = 0xd47f;
    RunAsmCode(0xa6fcd9, 0, 0, 0, 0);
    for (int i = 0; i < 4; i++) {
      EnemyData *e = gEnemyData(i * 64);
      printf("ZEBETITE hole=%d slot=%d id=%04X health=%u x=%u y=%u\n", hole, i, e->enemy_ptr, e->health, e->x_pos, e->y_pos);
    }
  }
  return 0;
}
