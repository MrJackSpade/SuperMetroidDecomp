/* #439: unpatched cartridge jump/fade/equipment routines, without a GUI. */
#include "../Common/CartridgeCpuFixture.h"
#include "../Common/MovementEntryPoints.h"
#include "fixture.h"

int main(int argc, char **argv) {
  if (argc != 2) return 2;
  FILE *file = fopen(argv[1], "rb");
  if (!file || fread(rom, 1, sizeof(rom), file) != sizeof(rom) || fgetc(file) != EOF)
    Die("Expected unheadered 3 MiB ROM");
  fclose(file);
  printf("mode,jump,frame,input,x,y,pose,anim,timer,yspeed,ydir,items\n");
  for (int mode = 0; mode < 3; mode++) for (int jump = 27; jump <= 31; jump++) {
    memset(g_ram, 0, sizeof(g_ram));
    room_width_in_blocks = 144; room_height_in_blocks = 80;
    room_width_in_scrolls = 9; room_height_in_scrolls = 5; room_size_in_blocks = 144 * 80 * 2;
    interactive_enemy_indexes[0] = 0xffff;
    for (int x = 0; x < 144; x++) level_data[32 * 144 + x] = 0x8000;
    fx_y_pos = 8; lava_acid_y_pos = 0xffff; fx_type = 6; liquid_physics_type = 1;
    collected_items = 0x20; equipped_items = mode == 2 ? 0 : 0x20;
    game_state = 8; samus_health = samus_max_health = 99;
    samus_x_pos = samus_prev_x_pos = 1000; samus_y_pos = samus_prev_y_pos = 491;
    samus_pose = samus_prev_pose = 1; samus_pose_x_dir = samus_prev_pose_x_dir = 8;
    run(RefreshRadius);
    samus_anim_frame_timer = 1; samus_x_speed_table_pointer = 0x9f55;
    samus_input_handler = 0xe913; samus_movement_handler = 0xa337; grapple_beam_function = 0xc4f0;
    button_config_run_b = 0x8000; button_config_jump_a = 0x80; button_config_shoot_x = 0x40;
    button_config_aim_up_R = 0x10; button_config_aim_down_L = 0x20;
    button_config_itemcancel_y = 0x4000; button_config_itemswitch = 0x2000;
    reg_INIDISP = 15;
    uint16 previous = 0;
    for (int frame = 0; frame < 220; frame++) {
      if (frame == 31) {
        if (reg_INIDISP != 0x80 && reg_INIDISP != 0) Die("Fade did not freeze after 30 frames");
        /* The selector and toggle are native menu routines; no direct equipment
           write fakes the exploit. Rendering-only menu frames are not emulated. */
        run(SelectInitialEquipment);
        if (pausemenu_equipment_category_item != 0x102) Die("Gravity was not selected by inventory");
        joypad1_newkeys = mode == 1 ? 0x80 : 0;
        run(SuitEquipmentInput);
        if (equipped_items != (mode == 0 ? 0x20 : 0)) Die("Native menu Gravity toggle failed");
        run(ReconcileSamusEquipment);
        previous = jump <= 30 ? 0x80 : 0;
        game_state = 0x12;
      }
      nmi_frame_counter_word = nmi_frame_counter_byte = frame + 2;
      uint16 input = (frame == 0 ? 0x1000 : 0) | (frame >= jump ? 0x80 : 0);
      joypad1_lastkeys = input; joypad1_newkeys = input & ~previous; previous = input;
      run(InputPhase); run(InteractionPhase); samus_contact_damage_index = 0;
      run(0x900000 | samus_movement_handler);
      unsigned stages[] = {AnimationPhase,TransitionPhase,CollisionPosePhase,ApplyPosePhase,
        PoseHistoryPhase,HurtPhase,CollisionPhase};
      for (int i = 0; i < 7; i++) run(stages[i]);
      if (frame == 0) {
        run(PauseAdmission);
        if (game_state != 12 || screen_fade_delay != 1 || screen_fade_counter != 1)
          Die("Native Start did not admit pause");
      } else if (frame <= 30) run(FadeOut);
      printf("%d,%d,%d,%04X,%04X%04X,%04X%04X,%02X,%04X,%04X,%04X%04X,%04X,%04X\n",
        mode,jump,frame,input,samus_x_pos,samus_x_subpos,samus_y_pos,samus_y_subpos,
        samus_pose,samus_anim_frame,samus_anim_frame_timer,samus_y_speed,samus_y_subspeed,samus_y_dir,equipped_items);
    }
  }
  return 0;
}
