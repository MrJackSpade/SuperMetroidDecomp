// Original-CPU slopespark handoff probe for #436. Include after
// native-release-probe.h and dispatch before SDL initialization. The WRAM input is
// a private pre-controller checkpoint from the supplied retail movie; only the
// numeric trace written by this probe is suitable for committing.
static void DiagnosticSlopesparkWriteRow(
    FILE *output, int medium, int delayed, int frame, uint16 input) {
  fprintf(output,
      "%d,%d,%d,%04X,%04X%04X,%04X%04X,%02X,%02X,%04X%04X,%04X%04X,"
      "%04X%04X,%04X,%04X,%04X,%04X,%04X,%04X\n",
      medium, delayed, frame, input,
      samus_x_pos, samus_x_subpos, samus_y_pos, samus_y_subpos,
      samus_pose, samus_movement_type,
      samus_x_base_speed, samus_x_base_subspeed,
      samus_x_extra_run_speed, samus_x_extra_run_subspeed,
      samus_y_speed, samus_y_subspeed,
      samus_knockback_timer, knockback_dir, samus_shine_timer,
      timer_for_shine_timer, speed_boost_counter, samus_movement_handler);
}

static void DiagnosticSlopesparkStep(uint16 input, uint16 previous) {
  samus_new_pose = 0xffff;
  samus_new_pose_interrupted = 0xffff;
  samus_new_pose_transitional = 0xffff;
  samus_momentum_routine_index = 0;
  samus_special_transgfx_index = 0;
  samus_hurt_switch_index = 0;
  joypad1_lastkeys = input;
  joypad1_newkeys = input & ~previous;

  // This is the retail per-frame Samus ordering used by the established damage-
  // boost matrix. Every stage below executes original 65816 instructions.
  RunAsmCode(0x90ec22, 0, 0, 0, 0);
  RunAsmCode(0x90e90f, 0, 0, 0, 0);
  RunAsmCode(0x909c5b, 0, 0, 0, 0);
  samus_contact_damage_index = 0;
  RunAsmCode(0x900000 | samus_movement_handler, 0, 0, 0, 0);
  RunAsmCode(0x908000, 0, 0, 0, 0);
  RunAsmCode(0x90dde9, 0, 0, 0, 0);
  RunAsmCode(0x91e8b6, 0, 0, 0, 0);
  RunAsmCode(0x91eb88, 0, 0, 0, 0);
  RunAsmCode(0x90eab3, 0, 0, 0, 0);
  RunAsmCode(0x90e9ce, 0, 0, 0, 0);
  RunAsmCode(0xa09169, 0, 0, 0, 0);
  RunAsmCode(0x91d6f7, 0, 0, 0, 0);
}

int DiagnosticDamageBoostSlopespark(
    const char *rom, const char *wram_path, const char *output_path) {
  static const uint16 kAirSuccess[] = { 0x0280, 0x0280, 0, 0x0080, 0x0200, 0 };
  static const uint16 kAirDelayed[] = { 0x0280, 0x0280, 0, 0, 0x0080, 0x0200, 0 };
  static const uint16 kWaterSuccess[] = {
    0x0280, 0x0280, 0, 0x0080, 0x0200, 0x0200, 0x0200, 0x0200, 0,
  };
  static const uint16 kWaterDelayed[] = {
    0x0280, 0x0280, 0, 0, 0x0080, 0x0200, 0x0200, 0x0200, 0x0200, 0,
  };
  const uint16 *inputs[2][2] = {
    { kAirSuccess, kAirDelayed }, { kWaterSuccess, kWaterDelayed },
  };
  const int lengths[2][2] = {
    { sizeof(kAirSuccess) / sizeof(kAirSuccess[0]),
      sizeof(kAirDelayed) / sizeof(kAirDelayed[0]) },
    { sizeof(kWaterSuccess) / sizeof(kWaterSuccess[0]),
      sizeof(kWaterDelayed) / sizeof(kWaterDelayed[0]) },
  };

  int status = ProbeLoadRetailMovementRom(rom);
  if (status) return status;
  size_t wram_size = 0;
  uint8 *checkpoint = ReadWholeFile(wram_path, &wram_size);
  if (!checkpoint || wram_size != sizeof(g_ram)) {
    free(checkpoint);
    fprintf(stderr, "Expected one complete 128-KiB WRAM checkpoint.\n");
    return 4;
  }
  FILE *output = fopen(output_path, "wx");
  if (!output) {
    free(checkpoint);
    return 5;
  }
  fprintf(output,
      "medium,delayed,frame,input,x,y,pose,movement,baseSpeed,extraSpeed,"
      "ySpeed,hurtTimer,hurtDirection,shineTimer,paletteOwner,boostCounter,"
      "movementHandler\n");

  for (int medium = 0; medium < 2; medium++) {
    for (int delayed = 0; delayed < 2; delayed++) {
      cpu_reset(g_snes->cpu);
      memcpy(g_ram, checkpoint, sizeof(g_ram));
      g_snes->cpu->e = false;
      g_snes->cpu->sp = 0x1ff0;
      g_snes->cpu->dp = 0;

      if (medium) {
        // Preserve the movie's exact authored room, slope, subpixels, pose history,
        // and stored shine. Only substitute the cartridge's fully-submerged,
        // suitless-water knockback state for the captured dry contact state.
        fx_y_pos = 0;
        lava_acid_y_pos = 0xffff;
        fx_liquid_options = 0;
        fx_type = 6;
        liquid_physics_type = 1;
        samus_y_speed = 2;
        samus_y_subspeed = 0;
        samus_knockback_timer = 7;
      }

      uint16 previous = 0x0100;
      DiagnosticSlopesparkWriteRow(output, medium, delayed, -1, previous);
      for (int frame = 0; frame < lengths[medium][delayed]; frame++) {
        uint16 input = inputs[medium][delayed][frame];
        DiagnosticSlopesparkStep(input, previous);
        previous = input;
        DiagnosticSlopesparkWriteRow(output, medium, delayed, frame, input);
      }
    }
  }

  fclose(output);
  free(checkpoint);
  return 0;
}
