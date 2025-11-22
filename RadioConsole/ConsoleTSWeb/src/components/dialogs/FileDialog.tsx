import { useState } from 'react';
import { X, Folder, Music, ChevronRight, Home, Check } from 'lucide-react';
import { ScrollArea } from '../ui/scroll-area';

interface FileDialogProps {
  onConfirm: (path: string) => void;
  onCancel: () => void;
}

interface FileItem {
  name: string;
  type: 'file' | 'folder';
  path: string;
}

// Mock file system structure - would come from API: GET /api/file-player/browse
const MOCK_FILES: { [key: string]: FileItem[] } = {
  '/': [
    { name: 'music', type: 'folder', path: '/music' },
    { name: 'podcasts', type: 'folder', path: '/podcasts' },
    { name: 'audiobooks', type: 'folder', path: '/audiobooks' }
  ],
  '/music': [
    { name: 'jazz', type: 'folder', path: '/music/jazz' },
    { name: 'rock', type: 'folder', path: '/music/rock' },
    { name: 'classical', type: 'folder', path: '/music/classical' },
    { name: 'playlist.m3u', type: 'file', path: '/music/playlist.m3u' }
  ],
  '/music/jazz': [
    { name: 'summer-breeze.flac', type: 'file', path: '/music/jazz/summer-breeze.flac' },
    { name: 'blue-note.mp3', type: 'file', path: '/music/jazz/blue-note.mp3' },
    { name: 'smooth-jazz.flac', type: 'file', path: '/music/jazz/smooth-jazz.flac' }
  ],
  '/music/rock': [
    { name: 'classic-rock.mp3', type: 'file', path: '/music/rock/classic-rock.mp3' },
    { name: 'hard-rock.flac', type: 'file', path: '/music/rock/hard-rock.flac' },
    { name: 'indie-rock.mp3', type: 'file', path: '/music/rock/indie-rock.mp3' }
  ],
  '/music/classical': [
    { name: 'beethoven-symphony-9.flac', type: 'file', path: '/music/classical/beethoven-symphony-9.flac' },
    { name: 'mozart-requiem.flac', type: 'file', path: '/music/classical/mozart-requiem.flac' }
  ],
  '/podcasts': [
    { name: 'tech-talk-ep1.mp3', type: 'file', path: '/podcasts/tech-talk-ep1.mp3' },
    { name: 'news-daily.mp3', type: 'file', path: '/podcasts/news-daily.mp3' }
  ],
  '/audiobooks': [
    { name: 'science-fiction', type: 'folder', path: '/audiobooks/science-fiction' },
    { name: 'mystery', type: 'folder', path: '/audiobooks/mystery' }
  ]
};

export function FileDialog({ onConfirm, onCancel }: FileDialogProps) {
  const [currentPath, setCurrentPath] = useState('/');
  const [selectedPath, setSelectedPath] = useState<string | null>(null);

  const currentFiles = MOCK_FILES[currentPath] || [];

  const handleItemClick = (item: FileItem) => {
    if (item.type === 'folder') {
      setCurrentPath(item.path);
      setSelectedPath(null);
    } else {
      setSelectedPath(item.path);
    }
  };

  const handleConfirm = () => {
    if (selectedPath) {
      onConfirm(selectedPath);
    } else {
      // If no file selected, use the current directory
      onConfirm(currentPath);
    }
  };

  const navigateUp = () => {
    const parts = currentPath.split('/').filter(Boolean);
    parts.pop();
    setCurrentPath(parts.length === 0 ? '/' : '/' + parts.join('/'));
    setSelectedPath(null);
  };

  const navigateHome = () => {
    setCurrentPath('/');
    setSelectedPath(null);
  };

  return (
    <div className="fixed inset-0 bg-black/80 flex items-center justify-center z-50">
      <div className="bg-gray-800 rounded-xl border border-gray-700 p-6 w-[600px] h-[500px] shadow-2xl flex flex-col">
        {/* Header */}
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg">Select File or Folder</h3>
          <button
            onClick={onCancel}
            className="p-2 hover:bg-gray-700 rounded-lg transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Path Navigation */}
        <div className="bg-gray-900 rounded-lg p-3 mb-4 flex items-center gap-2">
          <button
            onClick={navigateHome}
            className="p-2 hover:bg-gray-700 rounded transition-colors"
            title="Home"
          >
            <Home className="w-4 h-4" />
          </button>
          <span className="text-sm font-mono text-gray-400 flex-1">{currentPath}</span>
          {currentPath !== '/' && (
            <button
              onClick={navigateUp}
              className="text-sm px-3 py-1 bg-gray-700 hover:bg-gray-600 rounded transition-colors"
            >
              Up
            </button>
          )}
        </div>

        {/* File List */}
        <ScrollArea className="flex-1 -mx-2 mb-4">
          <div className="space-y-1 px-2">
            {currentFiles.map(item => (
              <button
                key={item.path}
                onClick={() => handleItemClick(item)}
                className={`w-full p-3 rounded-lg transition-all touch-manipulation text-left flex items-center gap-3 ${
                  selectedPath === item.path
                    ? 'bg-blue-600 hover:bg-blue-700'
                    : 'bg-gray-700 hover:bg-gray-600'
                }`}
              >
                {item.type === 'folder' ? (
                  <Folder className="w-5 h-5 text-yellow-500 flex-shrink-0" />
                ) : (
                  <Music className="w-5 h-5 text-blue-400 flex-shrink-0" />
                )}
                <span className="flex-1">{item.name}</span>
                {item.type === 'folder' && (
                  <ChevronRight className="w-5 h-5 text-gray-400" />
                )}
              </button>
            ))}
            {currentFiles.length === 0 && (
              <div className="text-center py-8 text-gray-500">
                No files or folders
              </div>
            )}
          </div>
        </ScrollArea>

        {/* Selected Path Display */}
        {selectedPath && (
          <div className="bg-gray-900 rounded-lg p-3 mb-4">
            <div className="text-xs text-gray-400 mb-1">Selected:</div>
            <div className="text-sm font-mono text-green-400">{selectedPath}</div>
          </div>
        )}

        {/* Actions */}
        <div className="flex gap-2">
          <button
            onClick={onCancel}
            className="flex-1 py-3 bg-gray-700 hover:bg-gray-600 rounded-lg transition-colors touch-manipulation"
          >
            Cancel
          </button>
          <button
            onClick={handleConfirm}
            className="flex-1 py-3 bg-green-600 hover:bg-green-700 rounded-lg transition-colors touch-manipulation flex items-center justify-center gap-2"
          >
            <Check className="w-5 h-5" />
            Select
          </button>
        </div>
      </div>
    </div>
  );
}
