import { SpotifyPlayer } from './now-playing/SpotifyPlayer';
import { RadioPlayer } from './now-playing/RadioPlayer';
import { VinylPlayer } from './now-playing/VinylPlayer';
import { FilePlayer } from './now-playing/FilePlayer';
import { DefaultPlayer } from './now-playing/DefaultPlayer';

interface NowPlayingProps {
  inputDevice: string;
  isPlaying: boolean;
  onPlayPauseToggle: () => void;
}

export function NowPlaying({ inputDevice, isPlaying, onPlayPauseToggle }: NowPlayingProps) {
  return (
    <div className="h-full bg-gray-800 rounded-lg border border-gray-700 overflow-hidden">
      {inputDevice === 'spotify' && <SpotifyPlayer isPlaying={isPlaying} onPlayPauseToggle={onPlayPauseToggle} />}
      {inputDevice === 'usb-radio' && <RadioPlayer isPlaying={isPlaying} />}
      {inputDevice === 'vinyl' && <VinylPlayer isPlaying={isPlaying} />}
      {inputDevice === 'file-player' && <FilePlayer isPlaying={isPlaying} />}
      {!['spotify', 'usb-radio', 'vinyl', 'file-player'].includes(inputDevice) && (
        <DefaultPlayer inputDevice={inputDevice} isPlaying={isPlaying} />
      )}
    </div>
  );
}