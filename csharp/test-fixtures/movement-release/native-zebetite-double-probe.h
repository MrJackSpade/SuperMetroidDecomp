// #443: complete enemy frames after a lethal missile into the lower half.
int DiagnosticZebetiteDouble(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "w"); if (!f) return 4;
  fprintf(f, "delay,frame,id,health,generation,secondaryId,secondaryHealth,secondaryFlash,events\n");
  for (int delay = 0; delay <= 8; delay++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0;
    events_that_happened[0] = 8;
    RunAsmCode(0xa6fcd9, 0, 0, 0, 0);
    cur_enemy_index = 0; RunAsmCode(0xa6fc33, 0, 0, 0, 0);
    cur_enemy_index = 64; RunAsmCode(0xa6fc33, 0, 64, 0, 0);
    first_free_enemy_index = 128; enemy_index_to_shake = 0xffff;
    samus_x_pos = samus_y_pos = 3000;
    layer1_x_pos = gEnemyData(0)->x_pos - 128;
    gEnemyData(0)->health = gEnemyData(64)->health = 100;
    projectile_type[0] = 0x100; projectile_damage[0] = 100;
    cur_enemy_index = 64; RunAsmCode(0xa6fdac, 0, 64, 0, 0);
    for (int frame = 0; frame < 12; frame++) {
      if (frame == 8) layer1_x_pos = gEnemyData(0)->x_pos - 128;
      if (frame == delay && gEnemyData(64)->enemy_ptr == 0xe27f) {
        projectile_type[0] = 0; projectile_damage[0] = 20;
        cur_enemy_index = 64; RunAsmCode(0xa6fdac, 0, 64, 0, 0);
      }
      memset(enemy_drawing_queue_sizes, 0, 16);
      RunAsmCode(0xa08eb6, 0, 0, 0, 0); RunAsmCode(0xa08fd4, 0, 0, 0, 0);
      fprintf(f, "%d,%d,%u,%u,%u,%u,%u,%u,%u\n", delay, frame,
        gEnemyData(0)->enemy_ptr, gEnemyData(0)->health, Get_Zebetites(0)->zebet_var_D,
        gEnemyData(64)->enemy_ptr, gEnemyData(64)->health, gEnemyData(64)->flash_timer, events_that_happened[0]);
    }
  }
  fclose(f); return 0;
}
