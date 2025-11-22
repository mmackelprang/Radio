import { ScrollArea } from './ui/scroll-area';
import { Music } from 'lucide-react';

interface PlaylistItem {
  id: string;
  songName: string;
  artist: string;
  duration: number;
}

// Mock data - would come from API: GET /api/playlist
const PLAYLIST_ITEMS: PlaylistItem[] = [
  { id: '1', songName: 'Blinding Lights', artist: 'The Weeknd', duration: 200 },
  { id: '2', songName: 'Shape of You', artist: 'Ed Sheeran', duration: 234 },
  { id: '3', songName: 'Levitating', artist: 'Dua Lipa', duration: 203 },
  { id: '4', songName: 'Starboy', artist: 'The Weeknd', duration: 230 },
  { id: '5', songName: 'Perfect', artist: 'Ed Sheeran', duration: 263 },
  { id: '6', songName: "Don't Start Now", artist: 'Dua Lipa', duration: 183 },
  { id: '7', songName: 'Save Your Tears', artist: 'The Weeknd', duration: 215 },
  { id: '8', songName: 'Bad Habits', artist: 'Ed Sheeran', duration: 231 },
  { id: '9', songName: 'Physical', artist: 'Dua Lipa', duration: 194 },
  { id: '10', songName: 'After Hours', artist: 'The Weeknd', duration: 361 }
];

export function Playlist() {
  const formatTime = (seconds: number) => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  };

  return (
    <div className="h-full bg-gray-800 rounded-lg border border-gray-700 flex flex-col overflow-hidden">
      {/* Header */}
      <div className="p-4 border-b border-gray-700">
        <div className="flex items-center gap-2">
          <Music className="w-5 h-5 text-gray-400" />
          <h2 className="text-lg">Playlist</h2>
        </div>
      </div>

      {/* Playlist Items */}
      <ScrollArea className="flex-1">
        <div className="p-2">
          {PLAYLIST_ITEMS.map((item, index) => (
            <button
              key={item.id}
              className="w-full p-3 mb-1 bg-gray-700/50 hover:bg-gray-700 rounded-lg transition-colors text-left touch-manipulation group"
            >
              <div className="flex items-start gap-3">
                <div className="text-gray-500 font-mono text-sm w-6 flex-shrink-0">
                  {(index + 1).toString().padStart(2, '0')}
                </div>
                <div className="flex-1 min-w-0">
                  <div className="truncate mb-1">{item.songName}</div>
                  <div className="text-sm text-gray-400 truncate">{item.artist}</div>
                </div>
                <div className="text-sm font-mono text-gray-500 flex-shrink-0">
                  {formatTime(item.duration)}
                </div>
              </div>
            </button>
          ))}
        </div>
      </ScrollArea>
    </div>
  );
}
