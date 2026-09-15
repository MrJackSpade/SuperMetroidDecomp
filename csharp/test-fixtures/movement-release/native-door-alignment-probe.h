// #448: execute the cartridge's $82:E310 door-alignment coroutine directly.
// Include after native-release-probe.h. Time freeze suppresses the unrelated
// background streaming call while leaving the alignment branch untouched.
int DiagnosticDoorAlignment(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom);
  if (status) return status;
  FILE *f = fopen(output, "wx");
  if (!f) return 4;
  fprintf(f, "direction,start_x,start_y,call,before_x,before_y,after_x,after_y,next\n");
  const uint16 offsets[] = { 0x00, 0x01, 0x02, 0x7f, 0x80, 0xfe, 0xff };
  for (int direction = 0; direction < 4; direction++) {
    for (int sample = 0; sample < 7; sample++) {
      cpu_reset(g_snes->cpu);
      memset(g_ram, 0, sizeof(g_ram));
      g_snes->cpu->e = false;
      g_snes->cpu->sp = 0x1ff0;
      g_snes->cpu->dp = 0;
      door_direction = direction;
      door_transition_function = 0xe310;
      time_is_frozen_flag = 1;
      layer1_x_pos = 0x1200 | offsets[sample];
      layer1_y_pos = 0x3400 | offsets[sample];
      const uint16 start_x = layer1_x_pos;
      const uint16 start_y = layer1_y_pos;
      for (int call = 0; call < 130; call++) {
        const uint16 before_x = layer1_x_pos;
        const uint16 before_y = layer1_y_pos;
        RunAsmCode(0x82e310, 0, 0, 0, 0);
        fprintf(f, "%d,%04X,%04X,%d,%04X,%04X,%04X,%04X,%04X\n",
          direction, start_x, start_y, call, before_x, before_y,
          layer1_x_pos, layer1_y_pos, door_transition_function);
        if (door_transition_function != 0xe310) break;
      }
    }
  }
  fclose(f);
  return 0;
}
