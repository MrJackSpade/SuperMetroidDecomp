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
  cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
  g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0;
  events_that_happened[0] = 8; // Generation one: second Zebetite.
  RunAsmCode(0xa6fcd9, 0, 0, 0, 0);
  cur_enemy_index = 0; RunAsmCode(0xa6fc33, 0, 0, 0, 0);
  printf("LINK before primary=%04X secondary=%04X link=%04X\n", gEnemyData(0)->enemy_ptr, gEnemyData(64)->enemy_ptr, Get_Zebetites(64)->zebet_parameter_2);
  gEnemyData(0)->health = gEnemyData(64)->health = 0;
  cur_enemy_index = 0; RunAsmCode(0xa6fc33, 0, 0, 0, 0);
  printf("LINK respawn health=%u secondary=%u generation=%u\n", gEnemyData(0)->health, gEnemyData(64)->health, Get_Zebetites(0)->zebet_var_D);
  projectile_type[0] = 0; projectile_damage[0] = 20;
  cur_enemy_index = 64; RunAsmCode(0xa6fdac, 0, 64, 0, 0);
  printf("LINK shot health=%u secondary=%u flash=%u/%u\n", gEnemyData(0)->health, gEnemyData(64)->health, gEnemyData(0)->flash_timer, gEnemyData(64)->flash_timer);
  for (int frozen = 0; frozen < 2; frozen++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0;
    gEnemyData(0)->enemy_ptr = 0xe27f;
    gEnemyData(0)->x_pos = 1024; gEnemyData(0)->y_pos = 128;
    gEnemyData(0)->ai_handler_bits = frozen ? 4 : 0;
    RunAsmCode(0xa08eb6, 0, 0, 0, 0);
    printf("FROZEN active=%04X interactive=%04X frozen=%d\n", active_enemy_indexes[0], interactive_enemy_indexes[0], frozen);
  }
  return 0;
}
