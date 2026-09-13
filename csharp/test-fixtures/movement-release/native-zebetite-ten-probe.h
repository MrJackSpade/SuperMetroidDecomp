// #443: ten missiles under controlled camera exposure, original enemy frames.
int DiagnosticZebetiteTen(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "w"); if (!f) return 4;
  fprintf(f, "generation,visible,frame,id,health,flash,events\n");
  for (int generation = 1; generation <= 3; generation += 2)
  for (int visible = 1; visible <= 7; visible++) {
    int exposure = visible == 7 ? 20 : visible;
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0;
    events_that_happened[0] = generation << 3;
    RunAsmCode(0xa6fcd9, 0, 0, 0, 0);
    cur_enemy_index = 0; RunAsmCode(0xa6fc33, 0, 0, 0, 0);
    cur_enemy_index = 64; RunAsmCode(0xa6fc33, 0, 64, 0, 0);
    first_free_enemy_index = 128; enemy_index_to_shake = 0xffff;
    samus_x_pos = samus_y_pos = 3000;
    uint16 camera = gEnemyData(0)->x_pos - 128;
    uint16 off_camera = gEnemyData(0)->x_pos + gEnemyData(0)->x_width + 1;
    for (int frame = 0; frame < 212; frame++) {
      if (frame < 200 && frame % 20 == 0) {
        projectile_type[0] = 0x100; projectile_damage[0] = 100;
        cur_enemy_index = 0; RunAsmCode(0xa6fdac, 0, 0, 0, 0);
      }
      layer1_x_pos = frame >= 200 || frame % 20 < exposure ? camera : off_camera;
      memset(enemy_drawing_queue_sizes, 0, 16);
      RunAsmCode(0xa08eb6, 0, 0, 0, 0); RunAsmCode(0xa08fd4, 0, 0, 0, 0);
      fprintf(f, "%d,%d,%d,%u,%u,%u,%u\n", generation, exposure, frame,
        gEnemyData(0)->enemy_ptr, gEnemyData(0)->health, gEnemyData(0)->flash_timer, events_that_happened[0]);
    }
  }
  fclose(f); return 0;
}
