import { Heart, Play, Pause, SkipBack, SkipForward } from 'lucide-react';
import { useState } from 'react';
import { ImageWithFallback } from '../figma/ImageWithFallback';

interface SpotifyPlayerProps {
  isPlaying: boolean;
  onPlayPauseToggle: () => void;
}

export function SpotifyPlayer({ isPlaying, onPlayPauseToggle }: SpotifyPlayerProps) {
  const [liked, setLiked] = useState(false);
  
  // Mock data - would come from API: GET /api/spotify/current-track
  const currentTrack = {
    name: 'Blinding Lights',
    artist: 'The Weeknd',
    album: 'After Hours',
    duration: 200,
    currentTime: 87,
    albumArt: 'https://images.unsplash.com/photo-1470225620780-dba8ba36b745?w=400&h=400&fit=crop'
  };

  const formatTime = (seconds: number) => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  };

  const progress = (currentTrack.currentTime / currentTrack.duration) * 100;

  return (
    <div className="h-full flex items-center p-6 gap-6">
      {/* Album Art */}
      <div className="relative w-32 h-32 flex-shrink-0">
        <ImageWithFallback
          src={currentTrack.albumArt}
          alt="Album art"
          className="w-full h-full rounded-lg object-cover shadow-lg"
        />
        <div className="absolute top-2 right-2">
          <button
            onClick={() => setLiked(!liked)}
            className={`p-2 rounded-full backdrop-blur-sm transition-colors touch-manipulation ${
              liked ? 'bg-green-600 text-white' : 'bg-black/50 text-white hover:bg-black/70'
            }`}
          >
            <Heart className={`w-5 h-5 ${liked ? 'fill-current' : ''}`} />
          </button>
        </div>
      </div>

      {/* Track Info & Controls */}
      <div className="flex-1 flex flex-col gap-3">
        <div>
          <div className="text-xl mb-1">{currentTrack.name}</div>
          <div className="text-gray-400">{currentTrack.artist}</div>
        </div>

        {/* Progress Bar */}
        <div className="flex items-center gap-3">
          <span className="text-sm font-mono text-gray-400 w-12">
            {formatTime(currentTrack.currentTime)}
          </span>
          <div className="flex-1 h-2 bg-gray-700 rounded-full overflow-hidden">
            <div 
              className="h-full bg-green-500 transition-all duration-300"
              style={{ width: `${progress}%` }}
            />
          </div>
          <span className="text-sm font-mono text-gray-400 w-12 text-right">
            {formatTime(currentTrack.duration)}
          </span>
        </div>

        {/* Playback Controls */}
        <div className="flex items-center gap-3">
          <button className="p-3 bg-gray-700 hover:bg-gray-600 rounded-lg transition-colors touch-manipulation">
            <SkipBack className="w-5 h-5" />
          </button>
          
          <button 
            onClick={onPlayPauseToggle}
            className="p-4 bg-green-600 hover:bg-green-700 rounded-lg transition-colors touch-manipulation"
          >
            {isPlaying ? <Pause className="w-6 h-6" /> : <Play className="w-6 h-6" />}
          </button>
          
          <button className="p-3 bg-gray-700 hover:bg-gray-600 rounded-lg transition-colors touch-manipulation">
            <SkipForward className="w-5 h-5" />
          </button>
        </div>
      </div>
    </div>
  );
}