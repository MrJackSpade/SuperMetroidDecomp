// #443: original full enemy dispatcher with controlled camera exposure after a hit.
int DiagnosticZebetiteRegen(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "w"); if (!f) return 4;
  fprintf(f, "visible,frame,health,flash,handler,secondaryHealth,secondaryFlash\n");
  for (int visible = 1; visible <= 20; visible++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0;
    events_that_happened[0] = 8;
    RunAsmCode(0xa6fcd9, 0, 0, 0, 0);
    cur_enemy_index = 0; RunAsmCode(0xa6fc33, 0, 0, 0, 0);
    cur_enemy_index = 64; RunAsmCode(0xa6fc33, 0, 64, 0, 0);
    first_free_enemy_index = 128; enemy_index_to_shake = 0xffff;
    samus_x_pos = samus_y_pos = 3000;
    projectile_type[0] = 0x100; projectile_damage[0] = 100;
    cur_enemy_index = 0; RunAsmCode(0xa6fdac, 0, 0, 0, 0);
    for (int frame = 0; frame < 20; frame++) {
      layer1_x_pos = frame < visible ? gEnemyData(0)->x_pos - 128 : 0;
      memset(enemy_drawing_queue_sizes, 0, 16);
      RunAsmCode(0xa08eb6, 0, 0, 0, 0); RunAsmCode(0xa08fd4, 0, 0, 0, 0);
      fprintf(f, "%d,%d,%u,%u,%u,%u,%u\n", visible, frame,
        gEnemyData(0)->health, gEnemyData(0)->flash_timer, gEnemyData(0)->ai_handler_bits,
        gEnemyData(64)->health, gEnemyData(64)->flash_timer);
    }
  }
  fclose(f); return 0;
}
