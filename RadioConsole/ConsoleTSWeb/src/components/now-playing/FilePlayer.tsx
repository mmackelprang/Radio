import { useState } from 'react';
import { FolderOpen, Play, Pause, SkipBack, SkipForward } from 'lucide-react';
import { ImageWithFallback } from '../figma/ImageWithFallback';
import { FileDialog } from '../dialogs/FileDialog';

interface FilePlayerProps {
  isPlaying: boolean;
}

export function FilePlayer({ isPlaying }: FilePlayerProps) {
  const [showFileDialog, setShowFileDialog] = useState(false);
  
  // Mock data - would come from API: GET /api/file-player/current
  const currentFile = {
    songName: 'Summer Breeze',
    fileName: '/music/jazz/summer-breeze.flac',
    artist: 'Jazz Ensemble',
    duration: 245,
    currentTime: 123,
    albumArt: 'https://images.unsplash.com/photo-1511379938547-c1f69419868d?w=400&h=400&fit=crop'
  };

  const formatTime = (seconds: number) => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  };

  const progress = (currentFile.currentTime / currentFile.duration) * 100;

  const handleFileSelect = (path: string) => {
    console.log('Selected file:', path);
    setShowFileDialog(false);
    // API call: POST /api/file-player/select
  };

  return (
    <div className="h-full flex items-center p-6 gap-6">
      {/* Album Art */}
      <div className="w-32 h-32 flex-shrink-0">
        <ImageWithFallback
          src={currentFile.albumArt}
          alt="Album art"
          className="w-full h-full rounded-lg object-cover shadow-lg"
        />
      </div>

      {/* Track Info & Controls */}
      <div className="flex-1 flex flex-col gap-3">
        <div>
          <div className="text-xl mb-1">{currentFile.songName}</div>
          <div className="text-gray-400 text-sm mb-1">{currentFile.artist}</div>
          <div className="text-gray-500 text-xs font-mono">{currentFile.fileName}</div>
        </div>

        {/* Progress Bar */}
        <div className="flex items-center gap-3">
          <span className="text-sm font-mono text-gray-400 w-12">
            {formatTime(currentFile.currentTime)}
          </span>
          <div className="flex-1 h-2 bg-gray-700 rounded-full overflow-hidden">
            <div 
              className="h-full bg-blue-500 transition-all duration-300"
              style={{ width: `${progress}%` }}
            />
          </div>
          <span className="text-sm font-mono text-gray-400 w-12 text-right">
            {formatTime(currentFile.duration)}
          </span>
        </div>

        {/* Playback Controls */}
        <div className="flex items-center gap-3">
          <button className="p-3 bg-gray-700 hover:bg-gray-600 rounded-lg transition-colors touch-manipulation">
            <SkipBack className="w-5 h-5" />
          </button>
          
          <button className="p-4 bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors touch-manipulation">
            {isPlaying ? <Pause className="w-6 h-6" /> : <Play className="w-6 h-6" />}
          </button>
          
          <button className="p-3 bg-gray-700 hover:bg-gray-600 rounded-lg transition-colors touch-manipulation">
            <SkipForward className="w-5 h-5" />
          </button>

          <div className="flex-1" />

          <button
            onClick={() => setShowFileDialog(true)}
            className="flex items-center gap-2 px-4 py-3 bg-gray-700 hover:bg-gray-600 rounded-lg transition-colors touch-manipulation"
          >
            <FolderOpen className="w-5 h-5" />
            <span className="text-sm">Select File</span>
          </button>
        </div>
      </div>

      {/* File Dialog */}
      {showFileDialog && (
        <FileDialog
          onConfirm={handleFileSelect}
          onCancel={() => setShowFileDialog(false)}
        />
      )}
    </div>
  );
}
