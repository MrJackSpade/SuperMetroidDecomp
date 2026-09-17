/* #428: original-CPU movement-owner replacement probe for forced Blue Suit.
   Each row starts from the same active horizontal shinespark. The invoked cartridge
   routine either replaces the physical $0A58 movement-handler word or deliberately
   leaves it alone. The boost counter and extra speed are observed, never injected after
   spark setup, so a replaced handler with retained $0400/8.0000 state is native Blue Suit. */

enum {
  ForcedBlueSparkSetup = 0x90cffa,
  ForcedBlueSparkPoseSetup = 0x91faca,
  ForcedBlueDrainedPose = 0x91e4f8,
  ForcedBlueDrainedFallingCommand = 0x908360,
  ForcedBlueCeresEjection = 0x90e119,
  ForcedBlueElevatorCommand = 0x90f1c8,
  ForcedBlueXrayTeardown = 0x91e2ad,
  ForcedBlueReserveUnlockControl = 0x90f2e0,
};

static void seed_forced_blue_spark(void) {
  memset(g_ram, 0, sizeof(g_ram));
  memset(dma_channel_registers, 0, sizeof(dma_channel_registers));
  samus_pose = samus_prev_pose = 1;
  samus_pose_x_dir = samus_prev_pose_x_dir = 8;
  samus_x_pos = samus_prev_x_pos = 256;
  samus_y_pos = samus_prev_y_pos = 400;
  samus_health = samus_max_health = 99;
  equipped_items = collected_items = 0x2000;
  samus_input_handler = 0xe913;
  samus_movement_handler = 0xa337;
  frame_handler_alfa = 0xe695;
  frame_handler_beta = 0xe725;
  samus_x_speed_table_pointer = 0x9f55;
  fx_y_pos = lava_acid_y_pos = 0xffff;
  run(RefreshRadius);

  run(ForcedBlueSparkSetup);
  samus_pose = samus_prev_pose = 0xc9;
  samus_pose_x_dir = samus_prev_pose_x_dir = 8;
  run(RefreshRadius);
  run(ForcedBlueSparkPoseSetup);
  if (samus_movement_handler != 0xd106 || speed_boost_counter != 0x0400 ||
      samus_x_extra_run_speed != 8 || samus_x_extra_run_subspeed != 0)
    Die("Failed to seed native active horizontal shinespark");
}

static void print_forced_blue_row(const char *owner) {
  printf("%s,%04X,%04X,%02X,%04X,%04X,%04X,%04X,%04X\n",
    owner, samus_movement_handler, samus_input_handler, samus_pose, speed_boost_counter,
    samus_x_extra_run_speed, samus_x_extra_run_subspeed,
    samus_shine_timer, timer_for_shine_timer);
}

static void forced_blue_matrix(void) {
  printf("owner,movement,input,pose,boost,extra_speed,extra_subspeed,shine,palette\n");

  /* Super Metroid terminal drain and Mother Brain's rainbow release converge on
     controller zero and animation command $F7. The latter is the physical write that
     replaces the still-live spark handler. */
  seed_forced_blue_spark();
  run(ForcedBlueDrainedPose);
  run(ForcedBlueDrainedFallingCommand);
  print_forced_blue_row("drained-f7");

  seed_forced_blue_spark();
  run(ForcedBlueCeresEjection);
  print_forced_blue_row("ceres-ridley");

  seed_forced_blue_spark();
  run(ForcedBlueElevatorCommand);
  print_forced_blue_row("elevator-command");

  /* X-Mode eventually executes the ordinary X-Ray teardown after the spark has taken
     movement ownership. $91:E2AD restores normal movement while retaining $0400. */
  seed_forced_blue_spark();
  run(ForcedBlueXrayTeardown);
  print_forced_blue_row("xray-teardown");

  /* Automatic Reserve completion alone restores alpha/beta handlers, not $0A58. This is
     the adjacent control: Reserve/X-Mode needs its later interruption owner to make Blue
     Suit; command $10 by itself resumes the spark. */
  seed_forced_blue_spark();
  frame_handler_alfa = 0xe8d9;
  frame_handler_beta = 0xe8d9;
  run(ForcedBlueReserveUnlockControl);
  print_forced_blue_row("reserve-unlock-control");
}
