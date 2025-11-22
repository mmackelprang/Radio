import { useState } from 'react';
import { MainBar } from './components/MainBar';
import { AudioSetup } from './components/AudioSetup';
import { NowPlaying } from './components/NowPlaying';
import { Playlist } from './components/Playlist';
import { SystemConfig } from './components/SystemConfig';

type ViewType = 'main' | 'system-config';
type InputDevice = 'spotify' | 'usb-radio' | 'vinyl' | 'file-player' | 'bluetooth' | 'aux' | 'googlecast';

export default function App() {
  const [currentView, setCurrentView] = useState<ViewType>('main');
  const [currentInput, setCurrentInput] = useState<InputDevice>('spotify');
  const [volume, setVolume] = useState(50);
  const [balance, setBalance] = useState(0);
  const [shuffle, setShuffle] = useState(false);
  const [isPlaying, setIsPlaying] = useState(false);
  const [showPlaylist, setShowPlaylist] = useState(false);

  return (
    <div className="h-screen w-screen bg-gray-900 text-white overflow-hidden flex flex-col">
      {/* Main Navigation Bar */}
      <MainBar 
        onSystemConfigClick={() => setCurrentView('system-config')}
        onHomeClick={() => setCurrentView('main')}
        onPlaylistToggle={() => setShowPlaylist(!showPlaylist)}
        showPlaylist={showPlaylist}
      />
      
      {/* Main Content Area */}
      <div className="flex-1 flex overflow-hidden">
        {currentView === 'main' ? (
          <>
            {/* Left Section - Audio Setup + Now Playing */}
            <div className="flex-1 flex flex-col p-2 gap-2">
              {/* Audio Setup */}
              <AudioSetup
                volume={volume}
                balance={balance}
                shuffle={shuffle}
                isPlaying={isPlaying}
                onVolumeChange={setVolume}
                onBalanceChange={setBalance}
                onShuffleToggle={() => setShuffle(!shuffle)}
                onPlayPauseToggle={() => setIsPlaying(!isPlaying)}
                currentInput={currentInput}
                onInputChange={setCurrentInput}
              />
              
              {/* Now Playing */}
              <div className="flex-1 min-h-0">
                <NowPlaying 
                  inputDevice={currentInput}
                  isPlaying={isPlaying}
                  onPlayPauseToggle={() => setIsPlaying(!isPlaying)}
                />
              </div>
            </div>
            
            {/* Right Section - Playlist (on-demand) */}
            {showPlaylist && (
              <div className="w-80 p-2">
                <Playlist />
              </div>
            )}
          </>
        ) : (
          <div className="flex-1 p-2">
            <SystemConfig onClose={() => setCurrentView('main')} />
          </div>
        )}
      </div>
    </div>
  );
}