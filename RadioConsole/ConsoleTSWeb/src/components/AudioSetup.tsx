import { useState } from 'react';
import { Volume2, Play, Pause, SkipBack, SkipForward, Shuffle, Settings2, Cast } from 'lucide-react';
import { Slider } from './ui/slider';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from './ui/select';
import { CastDeviceDialog } from './dialogs/CastDeviceDialog';

interface AudioSetupProps {
  volume: number;
  balance: number;
  shuffle: boolean;
  isPlaying: boolean;
  onVolumeChange: (value: number) => void;
  onBalanceChange: (value: number) => void;
  onShuffleToggle: () => void;
  onPlayPauseToggle: () => void;
  currentInput: string;
  onInputChange: (value: string) => void;
}

const INPUT_OPTIONS = [
  { value: 'spotify', label: 'Spotify' },
  { value: 'usb-radio', label: 'USB Radio' },
  { value: 'vinyl', label: 'Vinyl Phonograph' },
  { value: 'file-player', label: 'File Player' },
  { value: 'bluetooth', label: 'Bluetooth' },
  { value: 'aux', label: 'AUX Input' },
  { value: 'googlecast', label: 'Google Cast' }
];

const OUTPUT_OPTIONS = [
  { value: 'speakers', label: 'Built-in Speakers' },
  { value: 'headphones', label: 'Headphones' },
  { value: 'bluetooth', label: 'Bluetooth Output' },
  { value: 'line-out', label: 'Line Out' }
];

export function AudioSetup({
  volume,
  balance,
  shuffle,
  isPlaying,
  onVolumeChange,
  onBalanceChange,
  onShuffleToggle,
  onPlayPauseToggle,
  currentInput,
  onInputChange
}: AudioSetupProps) {
  const [output, setOutput] = useState('speakers');
  const [showCastDialog, setShowCastDialog] = useState(false);
  const [selectedCastDevice, setSelectedCastDevice] = useState<string>('');

  const handleCastDeviceSelect = (deviceId: string) => {
    setSelectedCastDevice(deviceId);
    setShowCastDialog(false);
    // API call: POST /api/googlecast/connect
  };

  return (
    <div className="bg-gray-800 rounded-lg p-4 border border-gray-700">
      <div className="flex items-center gap-4">
        {/* Playback Controls */}
        <div className="flex items-center gap-2">
          <button
            onClick={onShuffleToggle}
            className={`p-3 rounded-lg transition-colors touch-manipulation ${
              shuffle ? 'bg-green-600 hover:bg-green-700' : 'bg-gray-700 hover:bg-gray-600'
            }`}
            title="Shuffle"
          >
            <Shuffle className="w-5 h-5" />
          </button>
          
          <button
            className="p-3 bg-gray-700 hover:bg-gray-600 rounded-lg transition-colors touch-manipulation"
            title="Previous"
          >
            <SkipBack className="w-5 h-5" />
          </button>
          
          <button
            onClick={onPlayPauseToggle}
            className="p-4 bg-green-600 hover:bg-green-700 rounded-lg transition-colors touch-manipulation"
            title={isPlaying ? 'Pause' : 'Play'}
          >
            {isPlaying ? <Pause className="w-6 h-6" /> : <Play className="w-6 h-6" />}
          </button>
          
          <button
            className="p-3 bg-gray-700 hover:bg-gray-600 rounded-lg transition-colors touch-manipulation"
            title="Next"
          >
            <SkipForward className="w-5 h-5" />
          </button>
        </div>

        {/* Volume Control */}
        <div className="flex items-center gap-3 flex-1">
          <Volume2 className="w-5 h-5 text-gray-400" />
          <div className="flex-1 max-w-48">
            <Slider
              value={[volume]}
              onValueChange={(values) => onVolumeChange(values[0])}
              max={100}
              step={1}
              className="cursor-pointer"
            />
          </div>
          <span className="text-sm font-mono w-12 text-right">{volume}%</span>
        </div>

        {/* Balance Control */}
        <div className="flex items-center gap-3 flex-1">
          <span className="text-sm text-gray-400">BAL</span>
          <div className="flex-1 max-w-32">
            <Slider
              value={[balance + 50]}
              onValueChange={(values) => onBalanceChange(values[0] - 50)}
              max={100}
              step={1}
              className="cursor-pointer"
            />
          </div>
          <span className="text-sm font-mono w-12 text-right">
            {balance > 0 ? `R${balance}` : balance < 0 ? `L${Math.abs(balance)}` : 'C'}
          </span>
        </div>

        {/* Input/Output Selection */}
        <div className="flex items-center gap-2">
          <div className="flex flex-col gap-1">
            <div className="flex items-center gap-2">
              <span className="text-xs text-gray-400 w-12">Input</span>
              <Select value={currentInput} onValueChange={onInputChange}>
                <SelectTrigger className="w-40 h-9 bg-gray-700 border-gray-600">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {INPUT_OPTIONS.map(option => (
                    <SelectItem key={option.value} value={option.value}>
                      {option.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              <button className="p-2 bg-gray-700 hover:bg-gray-600 rounded transition-colors touch-manipulation">
                <Settings2 className="w-4 h-4" />
              </button>
            </div>
            
            <div className="flex items-center gap-2">
              <span className="text-xs text-gray-400 w-12">Output</span>
              <Select value={output} onValueChange={setOutput}>
                <SelectTrigger className="w-40 h-9 bg-gray-700 border-gray-600">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {OUTPUT_OPTIONS.map(option => (
                    <SelectItem key={option.value} value={option.value}>
                      {option.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              <button className="p-2 bg-gray-700 hover:bg-gray-600 rounded transition-colors touch-manipulation">
                <Settings2 className="w-4 h-4" />
              </button>
            </div>

            {/* Cast Device Selection for GoogleCast */}
            {currentInput === 'googlecast' && (
              <button
                onClick={() => setShowCastDialog(true)}
                className="flex items-center gap-2 px-3 py-2 bg-blue-600 hover:bg-blue-700 rounded transition-colors touch-manipulation"
              >
                <Cast className="w-4 h-4" />
                <span className="text-xs">
                  {selectedCastDevice ? 'Change Device' : 'Select Device'}
                </span>
              </button>
            )}
          </div>
        </div>
      </div>

      {/* Cast Device Dialog */}
      {showCastDialog && (
        <CastDeviceDialog
          onConfirm={handleCastDeviceSelect}
          onCancel={() => setShowCastDialog(false)}
          selectedDevice={selectedCastDevice}
        />
      )}
    </div>
  );
}