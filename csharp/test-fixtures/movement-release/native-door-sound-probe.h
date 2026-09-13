// #422: execute original 65816 queue/handshake code with deterministic APU replies.
// Include after native-release-probe.h. No SPC timing or controller-route claim.
#include "snes/apu.h"
bool g_diagnostic_door_sound_ports;
int DiagnosticDoorWaitHandoff(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  g_diagnostic_door_sound_ports = true;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f, "library,count,lag,frame,phase,read,write,state,port,x,y\n");
  const uint32 queue_max3[] = { 0x809035, 0x8090b7, 0x809139 };
  for (int library = 0; library < 3; library++)
  for (int count = 0; count <= 3; count++)
  for (int lag = 0; lag <= 2; lag++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram)); apu_reset(g_snes->apu);
    g_snes->apu->spc->stopped = true; g_snes->cpu->e = false;
    g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    game_state = 11; door_transition_function = 0xe29e;
    interactive_enemy_indexes[0] = 0xffff;
    // LoadEnemies ($A0:8A31-$8A3A) installs this even for an empty population.
    // The draw pass invokes it unconditionally; zero is not a valid empty hook.
    enemy_gfx_drawn_hook.bank = 0xa0; enemy_gfx_drawn_hook.addr = 0x804c;
    samus_pose = samus_prev_pose = 1; samus_pose_x_dir = samus_prev_pose_x_dir = 8;
    samus_draw_handler = 0xeb52; samus_health = 99;
    samus_x_pos = samus_prev_x_pos = 128; samus_y_pos = samus_prev_y_pos = 128;
    for (int i = 0; i < count; i++) RunAsmCode(queue_max3[library], i + 1, 0, 0, 0);
    uint8 history[32] = {0};
    for (int frame = 0; frame < 32; frame++) {
      g_snes->apu->outPorts[library + 1] = frame > lag ? history[frame - lag - 1] : 0;
      oam_next_ptr = 0;
      RunAsmCode(0x82e29e, 0, 0, 0, 0);
      memset(&g_apu_write, 0xff, sizeof(g_apu_write)); RunAsmCode(0x8289ef, 0, 0, 0, 0);
      uint8 port = g_apu_write.ports[library + 1];
      history[frame] = port != 255 ? port : frame ? history[frame - 1] : 0;
      fprintf(f, "%d,%d,%d,%d,%04X,%u,%u,%u,%u,%u,%u\n",library,count,lag,frame,
        door_transition_function,sfx_readpos[library],sfx_writepos[library],sfx_state[library],port,samus_x_pos,samus_y_pos);
      fflush(f);
      if (door_transition_function != 0xe29e) break;
    }
  }
  fclose(f); return 0;
}
int DiagnosticCombinedDoorSounds(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  g_diagnostic_door_sound_ports = true;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f, "counts,start,reverse,frame,library,read,write,state,current,delay,port,queued\n");
  const uint32 queue_max3[] = { 0x809035, 0x8090b7, 0x809139 };
  for (int counts = 0; counts < 64; counts++)
  for (int start = 0; start <= 14; start += 14)
  for (int reverse = 0; reverse < 2; reverse++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram)); apu_reset(g_snes->apu);
    g_snes->apu->spc->stopped = true;
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    game_state = 11;
    for (int library = 0; library < 3; library++) {
      sfx_readpos[library] = sfx_writepos[library] = start;
      for (int i = 0; i < ((counts >> (library * 2)) & 3); i++)
        RunAsmCode(queue_max3[library], i + 1, 0, 0, 0);
    }
    uint8 history[3][32] = {0};
    for (int frame = 0; frame < 32; frame++) {
      for (int library = 0; library < 3; library++) {
        int lag = reverse ? 2 - library : library;
        g_snes->apu->outPorts[library + 1] = frame > lag ? history[library][frame - lag - 1] : 0;
      }
      memset(&g_apu_write, 0xff, sizeof(g_apu_write)); RunAsmCode(0x8289ef, 0, 0, 0, 0);
      int queued = 0;
      for (int library = 0; library < 3; library++) queued |= sfx_readpos[library] != sfx_writepos[library];
      for (int library = 0; library < 3; library++) {
        uint8 port = g_apu_write.ports[library + 1];
        history[library][frame] = port != 255 ? port : frame ? history[library][frame - 1] : 0;
        fprintf(f, "%d,%d,%d,%d,%d,%u,%u,%u,%u,%u,%u,%d\n", counts,start,reverse,frame,library,
          sfx_readpos[library],sfx_writepos[library],sfx_state[library],sfx_cur[library],sfx_clear_delay[library],port,queued);
      }
    }
  }
  fclose(f); return 0;
}
int DiagnosticDoorSounds(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom);
  if (status) return status;
  g_diagnostic_door_sound_ports = true;
  FILE *f = fopen(output, "wx");
  if (!f) return 4;
  fprintf(f, "library,count,start,lag,frame,read,write,state,current,delay,port,queued\n");
  const uint32 queue_max3[] = { 0x809035, 0x8090b7, 0x809139 };
  for (int library = 0; library < 3; library++)
  for (int count = 0; count <= 3; count++)
  for (int start = 0; start <= 14; start += 14)
  for (int lag = 0; lag <= 2; lag++) {
    cpu_reset(g_snes->cpu);
    memset(g_ram, 0, sizeof(g_ram));
    apu_reset(g_snes->apu);
    g_snes->apu->spc->stopped = true;
    g_snes->cpu->e = false;
    g_snes->cpu->sp = 0x1ff0;
    g_snes->cpu->dp = 0;
    game_state = 11;
    sfx_readpos[library] = sfx_writepos[library] = start;
    for (int i = 0; i < count; i++) RunAsmCode(queue_max3[library], i + 1, 0, 0, 0);
    uint8 history[32] = {0};
    for (int frame = 0; frame < 32; frame++) {
      g_snes->apu->outPorts[library + 1] = frame > lag ? history[frame - lag - 1] : 0;
      memset(&g_apu_write, 0xff, sizeof(g_apu_write));
      RunAsmCode(0x8289ef, 0, 0, 0, 0);
      uint8 port = g_apu_write.ports[library + 1];
      history[frame] = port != 255 ? port : frame ? history[frame - 1] : 0;
      fprintf(f, "%d,%d,%d,%d,%d,%u,%u,%u,%u,%u,%u,%d\n", library, count, start, lag, frame,
        sfx_readpos[library], sfx_writepos[library], sfx_state[library], sfx_cur[library],
        sfx_clear_delay[library], port, sfx_readpos[library] != sfx_writepos[library]);
    }
  }
  fclose(f);
  return 0;
}
