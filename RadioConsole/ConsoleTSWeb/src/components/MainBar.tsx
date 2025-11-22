import { useState, useEffect } from 'react';
import { Settings, Home, List } from 'lucide-react';

interface MainBarProps {
  onSystemConfigClick: () => void;
  onHomeClick: () => void;
  onPlaylistToggle: () => void;
  showPlaylist: boolean;
}

export function MainBar({ onSystemConfigClick, onHomeClick, onPlaylistToggle, showPlaylist }: MainBarProps) {
  const [currentTime, setCurrentTime] = useState(new Date());
  const [systemStats, setSystemStats] = useState({
    cpu: 0,
    ram: 0,
    threads: 0
  });

  useEffect(() => {
    const timer = setInterval(() => {
      setCurrentTime(new Date());
    }, 1000);

    return () => clearInterval(timer);
  }, []);

  useEffect(() => {
    // Mock system stats - would be fetched from API: GET /api/system/stats
    const statsInterval = setInterval(() => {
      setSystemStats({
        cpu: Math.floor(Math.random() * 30 + 10),
        ram: Math.floor(Math.random() * 20 + 40),
        threads: Math.floor(Math.random() * 10 + 15)
      });
    }, 2000);

    return () => clearInterval(statsInterval);
  }, []);

  const formatTime = (date: Date) => {
    return date.toLocaleTimeString('en-US', { 
      hour: '2-digit', 
      minute: '2-digit',
      second: '2-digit',
      hour12: true 
    });
  };

  const formatDate = (date: Date) => {
    return date.toLocaleDateString('en-US', { 
      weekday: 'short',
      month: 'short', 
      day: 'numeric',
      year: 'numeric'
    });
  };

  return (
    <div className="h-14 bg-black border-b border-gray-700 flex items-center px-4 gap-4">
      {/* Date/Time in Retro LED Font */}
      <div className="flex flex-col">
        <div className="font-mono text-green-400 tracking-wider" style={{ 
          fontSize: '1.25rem',
          textShadow: '0 0 10px rgba(34, 197, 94, 0.5)',
          fontFamily: '"Courier New", monospace'
        }}>
          {formatTime(currentTime)}
        </div>
        <div className="text-xs text-green-400/70 font-mono">
          {formatDate(currentTime)}
        </div>
      </div>

      {/* System Stats in Smaller Retro LED Font */}
      <div className="flex gap-4 ml-4 font-mono text-amber-400" style={{
        fontSize: '0.75rem',
        textShadow: '0 0 8px rgba(251, 191, 36, 0.4)',
        fontFamily: '"Courier New", monospace'
      }}>
        <div>CPU: {systemStats.cpu}%</div>
        <div>RAM: {systemStats.ram}%</div>
        <div>THR: {systemStats.threads}</div>
      </div>

      {/* Spacer */}
      <div className="flex-1" />

      {/* Navigation Icons */}
      <div className="flex gap-2">
        <button
          onClick={onHomeClick}
          className="p-3 hover:bg-gray-800 rounded-lg transition-colors active:bg-gray-700 touch-manipulation"
          title="Home"
        >
          <Home className="w-7 h-7 text-gray-300" />
        </button>
        <button
          onClick={onPlaylistToggle}
          className={`p-3 rounded-lg transition-colors touch-manipulation ${
            showPlaylist ? 'bg-blue-600 hover:bg-blue-700' : 'hover:bg-gray-800 active:bg-gray-700'
          }`}
          title="Playlist"
        >
          <List className="w-7 h-7 text-gray-300" />
        </button>
        <button
          onClick={onSystemConfigClick}
          className="p-3 hover:bg-gray-800 rounded-lg transition-colors active:bg-gray-700 touch-manipulation"
          title="System Configuration"
        >
          <Settings className="w-7 h-7 text-gray-300" />
        </button>
      </div>
    </div>
  );
}