/* #431: original-CPU Crystal Flash beta / Draygon gamma shared-counter probe.
   Isolates owner entry and active handlers, not boss movement, drops or rendering. */
#define CAPTURE_DMA_CHANNEL_REGISTERS
#include "../Common/CartridgeCpuFixture.h"
#include "../Common/MovementEntryPoints.h"

enum {
  GrabEntry = 0x90e23b, GrabGamma = 0xe2a1, FlashEntry = 0x90d5a2,
  FlashRaising = 0xd678, FlashMain = 0xd6ce, FlashFinish = 0xd75b,
  Palette = 0x91d6f7,
  /* X-Ray admission and the ordinary HDMA phase needed to finish Flash's bubble. */
  XrayAdmission = 0x91e16d, HdmaPhase = 0x8884b9,
  /* Samus body OAM emission, including the invincibility/shine gate. */
  DrawBody = 0x9085e2,
  /* Bank-$94 body-overlap dispatcher, including area-specific sand reactions. */
  InsideBlockPhase = 0x949b60,
  BeamHud = 0x90b80d, ProjectilePhase = 0x90aece, ProjectileDraw = 0x938254,
  BeamCooldown = 0x90ac1c,
  OamLowTable = 0x370
};
static void grab(int right) {
  Cpu *cpu = cpu_init(NULL, 0);
  cpu->pc = GrabEntry; cpu->k = cpu->db = GrabEntry >> 16;
  cpu->sp = cpu->spBreakpoint = 0x1ff0; cpu->a = right;
  returned = false;
  for (int n = 0; n < 1000000 && !returned; n++) cpu_runOpcode(cpu);
  cpu_free(cpu);
  if (!returned) Die("Grab entry exceeded instruction budget");
}
#include "Edges.h"
#include "Refill.h"
static uint16 input_at(int frame, int mode) {
  if (mode == 0) return 0;
  if (mode == 1) return frame == 16 ? 0x100 : 0;
  if (mode == 2) return frame < 16 ? 0 : frame & 1 ? 0x100 : 0x200;
  return frame >= 16 ? 0x100 : 0;
}
int main(int argc, char **argv) {
  bool xray = argc == 3 && strcmp(argv[2], "xray") == 0;
  bool recharge = argc == 3 && strcmp(argv[2], "recharge") == 0;
  bool lifetime = argc == 3 && strcmp(argv[2], "lifetime") == 0;
  bool repeat = argc == 3 && strcmp(argv[2], "repeat") == 0;
  bool sand = argc == 3 && strcmp(argv[2], "sand") == 0;
  bool beam = argc == 3 && strcmp(argv[2], "beam") == 0;
  bool full = argc == 3 && (strcmp(argv[2], "runtime") == 0 || xray || recharge || lifetime || repeat || sand || beam);
  bool edges = argc == 3 && strcmp(argv[2], "edges") == 0;
  bool refill = argc == 3 && strcmp(argv[2], "refill") == 0;
  if (argc != 2 && !full && !edges && !refill) return 2;
  FILE *file = fopen(argv[1], "rb");
  if (!file || fread(rom, 1, sizeof(rom), file) != sizeof(rom) || fgetc(file) != EOF)
    Die("Expected unheadered 3 MiB ROM");
  fclose(file);
  if (edges) { edge_matrix(); return 0; }
  if (refill) { refill_matrix(); return 0; }
  printf("order,right,mode,frame,input,pose,y,handler,gamma,counter,previous,index,health,missiles,supers,pbs,shine,palette%s%s\n",
    full ? ",x,xsub,ysub,yspeed,ysubspeed,ydir,anim,animtimer,inputhandler" : "", lifetime ? ",body,zero_timer_body" : sand ? ",boost,extra_y,extra_ysub" : beam ? ",beam_palette,beam_oam" : "");
  for (int order = 0; order < 2; order++)
  for (int right = 0; right < 2; right++)
  for (int mode = 0; mode < (sand ? 8 : 4); mode++) {
    if ((xray || lifetime || repeat || beam) && mode != 2 || recharge && mode < 2) continue;
    memset(g_ram, 0, sizeof(g_ram));
    memset(dma_channel_registers, 0, sizeof(dma_channel_registers));
    samus_pose = samus_prev_pose = right ? 1 : 2;
    samus_pose_x_dir = samus_prev_pose_x_dir = right ? 8 : 4;
    samus_x_pos = recharge ? 1152 : 256; samus_y_pos = 400;
    samus_health = 49; samus_max_health = 99;
    samus_missiles = samus_super_missiles = samus_power_bombs = 10;
    samus_max_missiles = samus_max_super_missiles = samus_max_power_bombs = 10;
    button_config_shoot_x = 0x40; game_state = 8;
    grapple_beam_function = 0xc4f0;
    if (recharge) equipped_items = collected_items = 0x2000;
    if (full) {
      room_width_in_blocks = 144; room_height_in_blocks = 80;
      room_width_in_scrolls = 9; room_height_in_scrolls = 5; room_size_in_blocks = 144 * 80 * 2;
      interactive_enemy_indexes[0] = 0xffff;
      for (int x = 0; x < 144; x++) level_data[32 * 144 + x] = level_data[16 * 144 + x] = 0x8000;
      fx_y_pos = lava_acid_y_pos = 0xffff;
      samus_prev_x_pos = samus_x_pos; samus_prev_y_pos = samus_y_pos;
      samus_input_handler = 0xe913; samus_movement_handler = 0xa337;
      samus_x_speed_table_pointer = 0x9f55;
      button_config_run_b = 0x8000; button_config_jump_a = 0x80;
      button_config_aim_up_R = 0x10; button_config_aim_down_L = 0x20;
      button_config_itemcancel_y = 0x4000; button_config_itemswitch = 0x2000;
    }
    run(RefreshRadius);
    if (order == 0) grab(right);
    joypad1_lastkeys = 0x470;
    run(FlashEntry);
    if (samus_movement_handler != FlashRaising) Die("Flash admission failed");
    uint16 previous = 0x470;
    for (int frame = 0; frame < (sand || beam ? 351 : repeat ? 800 : lifetime ? 1000 : recharge ? 700 : xray ? 351 : full ? 430 : 120); frame++) {
      if (xray || recharge || lifetime || repeat || sand || beam) run(HdmaPhase);
      if (order == 1 && frame == 12) grab(right);
      uint16 input = input_at(frame, recharge || sand ? 2 : mode);
      if (full && frame >= 300) input = frame < 360 ? 0 : frame == 360 ? 0x80 : 0x880;
      if (lifetime && frame >= 300) input = 0;
      if (repeat && frame >= 300) input = 0;
      if (sand && frame >= 300) input = 0;
      if (beam && frame >= 300) input = 0;
      if (recharge && frame >= 300) {
        int crouch = mode == 2 ? 490 : 390;
        input = frame < 350 ? 0 : frame < crouch ? 0x8000 | (right ? 0x100 : 0x200) : frame == crouch ? 0x410 : 0;
      }
      joypad1_lastkeys = input; joypad1_newkeys = input & ~previous; previous = input;
      nmi_frame_counter_word = frame + (full ? 2 : 0);
      if (full) { run(InputPhase); run(InteractionPhase); samus_contact_damage_index = 0; }
      if (full || samus_movement_handler == FlashRaising || samus_movement_handler == FlashMain || samus_movement_handler == FlashFinish)
        run(0x900000 | samus_movement_handler);
      if (frame_handler_gamma == GrabGamma) run(0x900000 | GrabGamma);
      run(AnimationPhase);
      if (full) {
        unsigned stages[] = {TransitionPhase,CollisionPosePhase,ApplyPosePhase,PoseHistoryPhase,HurtPhase,CollisionPhase};
        for (int i = 0; i < 6; i++) run(stages[i]);
      }
      run(Palette);
      if (beam) run(BeamCooldown);
      if (sand && frame == 350) {
        area_index = 4;
        int row = (mode == 2 ? samus_y_pos - samus_y_radius : samus_y_pos + samus_y_radius - 1) >> 4;
        int block = row * room_width_in_blocks + (samus_x_pos >> 4);
        if (mode != 0) {
          level_data[block] = mode == 7 ? 0x8000 : 0x3000;
          BTS[block] = mode >= 3 && mode <= 5 ? 0x80 + mode : 0x82;
          if (mode == 6) {
            level_data[block + 1] = 0x3000; BTS[block + 1] = 0x82;
            level_data[block] = 0x5000; BTS[block] = 1;
          }
        }
        run(InsideBlockPhase);
        if (samus_shine_timer != 5 || timer_for_shine_timer != 7)
          Die("Sand must preserve the retained Flash palette and timer");
      }
      if (repeat && frame == 350) {
        joypad1_lastkeys = 0x470;
        run(FlashEntry);
        joypad1_lastkeys = input;
        if ((samus_movement_handler == FlashRaising) != (order == 1))
          Die("Repeat Flash must admit only the still-low-health/full-ammo order");
      }
      if (repeat && frame == 799 &&
          (order == 1 ? samus_shine_timer != 0 || samus_missiles || samus_super_missiles || samus_power_bombs
                      : samus_shine_timer == 0 || timer_for_shine_timer != 7))
        Die("Successful Flash must consume ammo and clear retention; failed Flash must preserve it");
      if (recharge && frame == (mode == 2 ? 490 : 390) &&
          (timer_for_shine_timer != (mode == 2 ? 1 : 7) || samus_shine_timer != (mode == 2 ? 179 : 5)))
        Die("Only a stage-four recharge may replace the retained Flash timer");
      if (recharge && frame == 699 && (samus_shine_timer != (mode == 2 ? 0 : 1)))
        Die("Recharge must expire while insufficient charge retains Flash");
      if (xray && frame == 350) {
        run(XrayAdmission); run(ApplyPosePhase);
        if (!time_is_frozen_flag || timer_for_shine_timer != 8 || samus_shine_timer != 0) {
          fprintf(stderr,"Xray frozen=%04X palette=%04X shine=%04X pose=%04X pending=%04X special=%04X status=%04X\n",time_is_frozen_flag,timer_for_shine_timer,samus_shine_timer,samus_pose,samus_new_pose_interrupted,samus_special_transgfx_index,power_bomb_explosion_status);
          Die("X-Ray must replace the retained Flash palette and clear its timer");
        }
      }
      if (full && !recharge && !lifetime && !repeat && mode == 2 && frame == 363 &&
          (samus_movement_handler != 0xd0ab || samus_contact_damage_index != 2 || samus_health != (order == 0 ? 98 : 48)))
        Die("Interrupted Flash must permit a damaging, energy-consuming spark without a charge");
      if (!full && frame == 96) {
        if (order == 0 && (mode == 1 || mode == 3) && samus_missiles != 0xffff)
          Die("One extra D-pad edge must underflow ten missiles");
        if (mode == 2 && (frame_handler_gamma == GrabGamma || substate != 60))
          Die("Alternating D-pad edges must reach the shared release counter");
        if (order == 1 && (samus_missiles != 10 || samus_super_missiles != 10 || samus_power_bombs != 10))
          Die("Grab during Flash must stop its ammo-drain movement handler");
      }
      printf("%d,%d,%d,%d,%04X,%02X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X",
        order,right,mode,frame,input,samus_pose,samus_y_pos,samus_movement_handler,frame_handler_gamma,
        substate,suit_pickup_light_beam_pos,which_item_to_pickup,samus_health,samus_missiles,samus_super_missiles,samus_power_bombs,samus_shine_timer,timer_for_shine_timer);
      if (full) printf(",%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X",samus_x_pos,samus_x_subpos,samus_y_subpos,samus_y_speed,samus_y_subspeed,samus_y_dir,samus_anim_frame,samus_anim_frame_timer,samus_input_handler);
      if (sand) printf(",%04X,%04X,%04X",speed_boost_counter,extra_samus_y_displacement,extra_samus_y_subdisplacement);
      if (beam) {
        printf(",");
        if (frame < 300) printf("-");
        else for (int i = 224; i < 240; i++) printf("%04X",palette_buffer[i] & 0x7fff);
        printf(",");
        if (frame != 350) printf("-");
        else {
          joypad1_lastkeys = joypad1_newkeys = 0x40;
          layer1_x_pos = 128; layer1_y_pos = 400; nmi_frame_counter_word = 351;
          run(BeamHud); run(ProjectilePhase);
          oam_next_ptr = 0; run(ProjectileDraw);
          if (!oam_next_ptr) {
            fprintf(stderr,"Beam count=%04X damage=%04X map=%04X instr=%04X type=%04X x=%04X y=%04X cooldown=%04X\n",projectile_counter,projectile_damage[0],projectile_spritemap_pointers[0],projectile_bomb_instruction_ptr[0],projectile_type[0],projectile_x_pos[0],projectile_y_pos[0],cooldown_timer);
            Die("Retained Flash beam must emit actual projectile OAM");
          }
          for (int i = 0; i < oam_next_ptr; i++) printf("%02X",g_ram[OamLowTable + i]);
        }
      }
      if (lifetime) {
        int visible = -1, control = -1;
        if (frame >= 300) {
          uint16 timer = samus_shine_timer, inv = samus_invincibility_timer;
          layer1_x_pos = 128; layer1_y_pos = 400;
          samus_invincibility_timer = 100; oam_next_ptr = 0;
          run(DrawBody); visible = oam_next_ptr;
          samus_shine_timer = 0; oam_next_ptr = 0;
          run(DrawBody); control = oam_next_ptr;
          samus_shine_timer = timer; samus_invincibility_timer = inv;
          if (!visible || ((frame & 1) ? control != 0 : control != visible))
            Die("Retained timer must prevent invincibility flicker; zero control must flicker");
        }
        printf(",%d,%d",visible,control);
      }
      printf("\n");
    }
  }
  return 0;
}
