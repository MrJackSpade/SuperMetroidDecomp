// #563: original CPU, constructed square ceiling/wall/floor; no translated AI.
// Include after native-release-probe.h. Output is diagnostic state only.
int DiagnosticCrawlerQuake(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f, "ceiling,control,frame,x,xsub,y,ysub,function,return,vx,vy,fall,fallsub\n");
  for (int ceiling = 0; ceiling < 2; ceiling++) for (int control = 0; control < 3; control++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    room_width_in_blocks = room_height_in_blocks = 16;
    for (int y = 0; y < 16; y++) for (int x = 0; x < 16; x++)
      level_data[y * 16 + x] = y == 6 || y == 12 || x == 6 ? 0x8000 : 0;
    Enemy_FireZoomer *e = (Enemy_FireZoomer *)(g_ram + 0x0f78);
    e->base.enemy_ptr = 0xdcff; e->base.x_width = e->base.y_height = 8;
    e->base.x_pos = ceiling ? 144 : 120; e->base.y_pos = ceiling ? 120 : 144;
    e->fzr_var_A = ceiling ? 0 : 0xff80; e->fzr_var_B = ceiling ? 0xff80 : 0;
    e->fzr_var_F = e->fzr_var_03 = ceiling ? 0xe7f2 : 0xe6c8;
    for (int frame = 0; frame < 40; frame++) {
      earthquake_timer = frame ? 0 : control == 1 ? 29 : 30;
      earthquake_type = control == 2 ? 18 : 20;
      RunAsmCode(0xa3e6c2, 0, 0, 0, 0);
      fprintf(f, "%d,%d,%d,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X\n",
        ceiling,control,frame,e->base.x_pos,e->base.x_subpos,e->base.y_pos,e->base.y_subpos,
        e->fzr_var_F,e->fzr_var_03,e->fzr_var_A,e->fzr_var_B,e->fzr_var_02,e->fzr_var_01);
    }
  }
  fclose(f); return 0;
}
