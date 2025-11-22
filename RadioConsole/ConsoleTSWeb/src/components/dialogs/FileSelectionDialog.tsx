import { useState, useEffect } from 'react';
import { X, Folder, File, Check, RefreshCw, ChevronUp } from 'lucide-react';
import { ScrollArea } from '../ui/scroll-area';

interface FileSelectionDialogProps {
  title: string;
  filter?: string; // e.g., ".json,.config"
  onConfirm: (filePath: string) => void;
  onCancel: () => void;
}

interface FileSystemItem {
  name: string;
  path: string;
  type: 'file' | 'directory';
  size?: number;
  modified?: string;
}

// Mock file system - would come from API: GET /api/filesystem/list?path={path}
const MOCK_FILES: { [path: string]: FileSystemItem[] } = {
  '/': [
    { name: 'config', path: '/config', type: 'directory' },
    { name: 'backups', path: '/backups', type: 'directory' },
    { name: 'prompts', path: '/prompts', type: 'directory' },
    { name: 'media', path: '/media', type: 'directory' }
  ],
  '/config': [
    { name: 'system.json', path: '/config/system.json', type: 'file', size: 2048, modified: '2024-11-20' },
    { name: 'audio.json', path: '/config/audio.json', type: 'file', size: 1024, modified: '2024-11-19' },
    { name: 'network.config', path: '/config/network.config', type: 'file', size: 512, modified: '2024-11-18' }
  ],
  '/backups': [
    { name: 'backup_2024-11-20.json', path: '/backups/backup_2024-11-20.json', type: 'file', size: 5120, modified: '2024-11-20' },
    { name: 'backup_2024-11-15.json', path: '/backups/backup_2024-11-15.json', type: 'file', size: 4896, modified: '2024-11-15' },
    { name: 'backup_2024-11-10.json', path: '/backups/backup_2024-11-10.json', type: 'file', size: 4732, modified: '2024-11-10' }
  ],
  '/prompts': [
    { name: 'welcome.wav', path: '/prompts/welcome.wav', type: 'file', size: 8192, modified: '2024-11-01' },
    { name: 'goodbye.wav', path: '/prompts/goodbye.wav', type: 'file', size: 7856, modified: '2024-11-01' }
  ],
  '/media': [
    { name: 'music', path: '/media/music', type: 'directory' },
    { name: 'playlists', path: '/media/playlists', type: 'directory' }
  ]
};

export function FileSelectionDialog({ title, filter, onConfirm, onCancel }: FileSelectionDialogProps) {
  const [currentPath, setCurrentPath] = useState('/');
  const [items, setItems] = useState<FileSystemItem[]>(MOCK_FILES['/']);
  const [selectedPath, setSelectedPath] = useState<string>('');
  const [isLoading, setIsLoading] = useState(false);

  const loadDirectory = (path: string) => {
    setIsLoading(true);
    // API call: GET /api/filesystem/list?path={path}
    setTimeout(() => {
      setItems(MOCK_FILES[path] || []);
      setCurrentPath(path);
      setIsLoading(false);
    }, 200);
  };

  const handleItemClick = (item: FileSystemItem) => {
    if (item.type === 'directory') {
      loadDirectory(item.path);
      setSelectedPath('');
    } else {
      // Check if file matches filter
      if (filter) {
        const extensions = filter.split(',');
        const fileExt = '.' + item.name.split('.').pop();
        if (extensions.includes(fileExt)) {
          setSelectedPath(item.path);
        }
      } else {
        setSelectedPath(item.path);
      }
    }
  };

  const handleGoUp = () => {
    const parts = currentPath.split('/').filter(p => p);
    if (parts.length > 0) {
      parts.pop();
      const newPath = '/' + parts.join('/');
      loadDirectory(newPath || '/');
      setSelectedPath('');
    }
  };

  const handleConfirm = () => {
    if (selectedPath) {
      onConfirm(selectedPath);
    }
  };

  const formatSize = (bytes?: number) => {
    if (!bytes) return '';
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
  };

  const matchesFilter = (fileName: string) => {
    if (!filter) return true;
    const extensions = filter.split(',');
    const fileExt = '.' + fileName.split('.').pop();
    return extensions.includes(fileExt);
  };

  return (
    <div className="fixed inset-0 bg-black/80 flex items-center justify-center z-50">
      <div className="bg-gray-800 rounded-xl border border-gray-700 p-6 w-[700px] h-[600px] shadow-2xl flex flex-col">
        {/* Header */}
        <div className="flex items-center justify-between mb-4">
          <div className="flex items-center gap-3">
            <Folder className="w-6 h-6 text-blue-400" />
            <h3 className="text-lg">{title}</h3>
          </div>
          <div className="flex items-center gap-2">
            <button
              onClick={() => loadDirectory(currentPath)}
              disabled={isLoading}
              className="p-2 bg-gray-700 hover:bg-gray-600 rounded-lg transition-colors disabled:opacity-50"
              title="Refresh"
            >
              <RefreshCw className={`w-5 h-5 ${isLoading ? 'animate-spin' : ''}`} />
            </button>
            <button
              onClick={onCancel}
              className="p-2 hover:bg-gray-700 rounded-lg transition-colors"
            >
              <X className="w-5 h-5" />
            </button>
          </div>
        </div>

        {/* Current Path */}
        <div className="flex items-center gap-2 mb-4 p-3 bg-gray-900 rounded-lg border border-gray-700">
          <button
            onClick={handleGoUp}
            disabled={currentPath === '/'}
            className="p-1 hover:bg-gray-700 rounded transition-colors disabled:opacity-30 disabled:cursor-not-allowed"
            title="Go up"
          >
            <ChevronUp className="w-5 h-5" />
          </button>
          <span className="text-sm font-mono text-gray-300">{currentPath}</span>
        </div>

        {/* File List */}
        <ScrollArea className="flex-1 -mx-2 mb-4">
          <div className="space-y-1 px-2">
            {items.map((item) => {
              const isFile = item.type === 'file';
              const isSelectable = !isFile || matchesFilter(item.name);
              const isSelected = selectedPath === item.path;
              
              return (
                <button
                  key={item.path}
                  onClick={() => handleItemClick(item)}
                  disabled={isFile && !isSelectable}
                  className={`w-full p-3 rounded-lg transition-all touch-manipulation text-left flex items-center gap-3 ${
                    isSelected
                      ? 'bg-blue-600 hover:bg-blue-700'
                      : isFile && !isSelectable
                      ? 'bg-gray-700/30 opacity-40 cursor-not-allowed'
                      : 'bg-gray-700 hover:bg-gray-600'
                  }`}
                >
                  {/* Icon */}
                  <div className={`p-2 rounded ${
                    isSelected ? 'bg-blue-700' : 'bg-gray-800'
                  }`}>
                    {item.type === 'directory' ? (
                      <Folder className="w-5 h-5 text-yellow-400" />
                    ) : (
                      <File className="w-5 h-5 text-gray-400" />
                    )}
                  </div>

                  {/* Info */}
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2">
                      <span className="truncate">{item.name}</span>
                      {isSelected && <Check className="w-4 h-4 flex-shrink-0" />}
                    </div>
                    {isFile && (
                      <div className="text-xs text-gray-400 mt-1">
                        {formatSize(item.size)} • {item.modified}
                      </div>
                    )}
                  </div>
                </button>
              );
            })}
            {items.length === 0 && !isLoading && (
              <div className="text-center py-12 text-gray-500">
                <Folder className="w-12 h-12 mx-auto mb-3 opacity-50" />
                <div>Empty directory</div>
              </div>
            )}
          </div>
        </ScrollArea>

        {/* Filter Info */}
        {filter && (
          <div className="mb-4 text-xs text-gray-500">
            Showing files: {filter}
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
            disabled={!selectedPath}
            className="flex-1 py-3 bg-green-600 hover:bg-green-700 rounded-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed touch-manipulation flex items-center justify-center gap-2"
          >
            <Check className="w-5 h-5" />
            Select File
          </button>
        </div>
      </div>
    </div>
  );
}
