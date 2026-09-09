// #474: cartridge CPU timing oracle. Include after native-release-probe.h.
// No cheats, SDL, SRAM or emulated input backend. Output is plain diagnostic CSV.
int DiagnosticSpinjump(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom);
  if (status) return status;
  FILE *f = fopen(output, "w");
  if (!f) return 4;
  fprintf(f, "water,left,scenario,delay,frame,input,x,y,pose,movement\n");
  for (int water = 0; water < 2; water++)
  for (int left = 0; left < 2; left++)
  for (int scenario = 0; scenario < 3; scenario++)
  for (int delay = 0; delay <= 8; delay++) {
    cpu_reset(g_snes->cpu);
    memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0;
    room_width_in_blocks = 16; room_height_in_blocks = 32;
    interactive_enemy_indexes[0] = 0xffff;
    for (int i = 256; i < 272; i++) level_data[i] = 0x8000;
    fx_y_pos = water ? 8 : 0xffff; lava_acid_y_pos = 0xffff;
    fx_liquid_options = 0x80; fx_type = water ? 6 : 0; liquid_physics_type = water;
    samus_x_pos = 200; samus_y_pos = 235;
    samus_x_radius = 5; samus_y_radius = 21;
    samus_pose = samus_prev_pose = left ? 1 : 2;
    samus_pose_x_dir = samus_prev_pose_x_dir = left ? 8 : 4;
    samus_x_speed_table_pointer = 0x9f55; samus_input_handler = 0xe913;
    samus_health = 99; button_config_run_b = 0x8000; button_config_jump_a = 0x80;
    samus_anim_frame_timer = 5;
    uint16 previous = 0;
    for (int frame = -24; frame < 40; frame++) {
      uint16 direction = left ? 0x200 : 0x100;
      uint16 input = frame < 0 ? (scenario == 2 && frame >= -1 ? 0x80 : 0) : scenario == 0 ?
        direction | (frame >= delay ? 0x80 : 0) :
        0x80 | (frame >= delay ? direction : 0);
      samus_new_pose = samus_new_pose_interrupted = samus_new_pose_transitional = 0xffff;
      samus_momentum_routine_index = samus_special_transgfx_index = samus_hurt_switch_index = 0;
      joypad1_lastkeys = input; joypad1_newkeys = input & ~previous; previous = input;
      RunAsmCode(0x90ec22, 0, 0, 0, 0); // Refresh pose collision radii.
      if (!(scenario == 2 && frame >= -1 && frame < 0))
        RunAsmCode(0x90e90f, 0, 0, 0, 0); // Dispatch the installed input handler, including auto-jump.
      RunAsmCode(0x909c5b, 0, 0, 0, 0); // Refresh native liquid-dependent gravity.
      RunAsmCode(0x90a337, 0, 0, 0, 0);
      RunAsmCode(0x908000, 0, 0, 0, 0);
      RunAsmCode(0x91e8b6, 0, 0, 0, 0);
      RunAsmCode(0x91eb88, 0, 0, 0, 0);
      RunAsmCode(0x90eab3, 0, 0, 0, 0); // Draw-time held-Jump history and timer.
      if (frame >= 0) fprintf(f, "%d,%d,%d,%d,%d,%04X,%04X%04X,%04X%04X,%02X,%02X\n",
        water,left,scenario,delay,frame,input,samus_x_pos,samus_x_subpos,
        samus_y_pos,samus_y_subpos,samus_pose,samus_movement_type);
    }
  }
  fclose(f); return 0;
}
