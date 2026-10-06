
#include <windows.h>
#include <crtdbg.h>
#include <exception>
#include <stdexcept>
#include <filesystem>
#include <fstream>
#include <vector>
#include <set>
#include <string>
#include <sstream>
#include "libretro.h"
#include "snes9x.h"
#include "memmap.h"
#include "movie.h"
#include "snapshot.h"
#include "zlib.h"
static gzFile updateTrace=nullptr;
static std::ofstream eventCsv;
static unsigned eventCount=0;
static unsigned captureMovieLength=0;
void RidleyObserveBoundary(unsigned pc) {
 if(!updateTrace) return;
 unsigned frame=S9xMovieActive() ? S9xMovieGetFrameCounter() : captureMovieLength;
 auto word=[](unsigned a) { return unsigned(Memory.RAM[a]|Memory.RAM[a+1]<<8); };
 eventCsv << eventCount++ << ',' << frame << ',' << std::hex << pc << ','
          << word(0x8b) << ',' << word(0x8f) << ',' << word(0x5b6) << ','
          << word(0x998) << ',' << word(0x99c) << '\n' << std::dec;
 if(pc!=0x809459) return;
 if(gzwrite(updateTrace,&frame,4)!=4 || gzwrite(updateTrace,&pc,4)!=4 || gzwrite(updateTrace,Memory.RAM,131072)!=131072)
  throw std::runtime_error("Cannot write native update boundary");
}
static bool env(unsigned command, void* data) {
 if(command==RETRO_ENVIRONMENT_SET_PIXEL_FORMAT) return true;
 return false;
}
static void video(const void*,unsigned,unsigned,size_t) {}
static size_t audio(const int16_t*,size_t frames) { return frames; }
static void poll() {}
static int16_t input(unsigned,unsigned,unsigned,unsigned) { return 0; }
int main(int argc,char** argv) {
 SetErrorMode(SEM_FAILCRITICALERRORS|SEM_NOGPFAULTERRORBOX|SEM_NOOPENFILEERRORBOX);
 _set_abort_behavior(0,_WRITE_ABORT_MSG|_CALL_REPORTFAULT);
 _CrtSetReportMode(_CRT_ASSERT,_CRTDBG_MODE_FILE); _CrtSetReportFile(_CRT_ASSERT,_CRTDBG_FILE_STDERR);
 try {
  if(argc!=5) throw std::runtime_error("usage: capture ROM MOVIE OUT frames-comma-separated");
  std::filesystem::path output=argv[3]; std::filesystem::create_directories(output);
  std::set<unsigned> requested; std::stringstream args(argv[4]); std::string item;
  while(std::getline(args,item,',')) requested.insert(std::stoul(item));
  retro_set_environment(env); retro_set_video_refresh(video); retro_set_audio_sample_batch(audio);
  retro_set_input_poll(poll); retro_set_input_state(input); retro_init();
  std::ifstream rom(argv[1],std::ios::binary); rom.exceptions(std::ios::badbit);
  if(!rom) throw std::runtime_error("ROM open failed");
  std::vector<char> bytes((std::istreambuf_iterator<char>(rom)),{});
  retro_game_info info={argv[1],bytes.data(),bytes.size(),nullptr};
  if(!retro_load_game(&info)) throw std::runtime_error("ROM load failed");
  int result=S9xMovieOpen(argv[2],true);
  if(result!=SUCCESS) throw std::runtime_error("Movie restore failed: "+std::to_string(result));
  const auto length=S9xMovieGetLength();
  captureMovieLength=length;
  auto write=[&](unsigned frame) {
   std::ofstream out(output/("frame-"+std::to_string(frame)+".wram"),std::ios::binary);
   out.exceptions(std::ios::failbit|std::ios::badbit); out.write((char*)Memory.RAM,131072);
   fprintf(stderr,"Captured frame %u, room %04X\n",frame,Memory.RAM[0x79b]|Memory.RAM[0x79c]<<8);
  };
  gzFile trace=gzopen((output/"all-frames.wram.gz").string().c_str(),"wb1");
  if(!trace) throw std::runtime_error("Cannot open full movie trace");
  updateTrace=gzopen((output/"update-boundaries.wram.gz").string().c_str(),"wb1");
  if(!updateTrace) throw std::runtime_error("Cannot open update trace");
  eventCsv.open(output/"input-events.csv"); eventCsv.exceptions(std::ios::failbit|std::ios::badbit);
  eventCsv << "event,source_frame,pc,held,pressed,nmi,state,door_function\n";
  unsigned loops=0;
  auto traceFrame=[&](unsigned frame) {
   if(gzwrite(trace,&frame,sizeof(frame))!=sizeof(frame) || gzwrite(trace,Memory.RAM,131072)!=131072)
    throw std::runtime_error("Cannot write full movie trace");
  };
  while(S9xMovieActive()) {
   auto frame=S9xMovieGetFrameCounter();
   traceFrame(frame);
   if(requested.erase(frame)) write(frame);
   retro_run();
   if(++loops>length+300) throw std::runtime_error("Movie playback did not terminate");
  }
  write(length);
  traceFrame(length);
  unsigned terminalPc=0;
  if(gzwrite(updateTrace,&length,4)!=4 || gzwrite(updateTrace,&terminalPc,4)!=4 || gzwrite(updateTrace,Memory.RAM,131072)!=131072)
   throw std::runtime_error("Cannot write terminal update state");
  eventCsv.close();
  if(gzclose(updateTrace)!=Z_OK) throw std::runtime_error("Cannot finalize update trace");
  updateTrace=nullptr;
  if(gzclose(trace)!=Z_OK) throw std::runtime_error("Cannot finalize trace");
  fprintf(stderr,"Completed original movie: %u frames, %u host iterations\n",length,loops);
  retro_unload_game(); retro_deinit(); return 0;
 } catch(const std::exception& e) { fprintf(stderr,"%s\n",e.what()); return 1; }
 catch(...) { fprintf(stderr,"Unknown native capture exception\n"); return 2; }
}
