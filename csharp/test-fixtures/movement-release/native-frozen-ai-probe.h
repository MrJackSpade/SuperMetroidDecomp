#include "native-bounded-cpu.h"

// Execute the cartridge's common frozen callback, not its C translation. Timers
// straddle expiration; hurt+frozen records exercise the callback's retained AI bits.
int DiagnosticFrozenAi(const char *rom, const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
  FILE *f=fopen(output,"wx"); if(!f) return 4;
  fprintf(f,"ice,timer,ai,next_timer,next_ai,flash\n");
  const int timers[]={0,1,2,400};
  for(int ice=0;ice<2;ice++) for(int t=0;t<4;t++) for(int hurt=0;hurt<2;hurt++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    equipped_beams=ice?2:0;
    EnemyData *e=gEnemyData(0);
    e->frozen_timer=timers[t]; e->ai_handler_bits=4|hurt*2; e->flash_timer=18;
    ProbeRunBounded(0xa0957e);
    fprintf(f,"%d,%d,%d,%04X,%04X,%04X\n",ice,timers[t],4|hurt*2,
      e->frozen_timer,e->ai_handler_bits,e->flash_timer);
  }
  fclose(f); return 0;
}
