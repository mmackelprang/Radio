import { useState } from 'react';
import { Radio, ChevronUp, ChevronDown, Save, Hash, Volume2, Signal, Sliders } from 'lucide-react';
import { NumericKeypad } from '../dialogs/NumericKeypad';
import { KeyboardDialog } from '../dialogs/KeyboardDialog';
import { MultiSelectDialog } from '../dialogs/MultiSelectDialog';

interface RadioPlayerProps {
  isPlaying: boolean;
}

type Band = 'AM' | 'FM' | 'SW' | 'AIR' | 'VHF';

const BAND_OPTIONS = [
  { value: 'AM', label: 'AM' },
  { value: 'FM', label: 'FM' },
  { value: 'SW', label: 'Short Wave' },
  { value: 'AIR', label: 'Air Band' },
  { value: 'VHF', label: 'VHF' }
];

const EQ_PRESETS = [
  { value: 'flat', label: 'Flat' },
  { value: 'bass-boost', label: 'Bass Boost' },
  { value: 'treble-boost', label: 'Treble Boost' },
  { value: 'voice', label: 'Voice' },
  { value: 'classical', label: 'Classical' },
  { value: 'rock', label: 'Rock' }
];

export function RadioPlayer({ isPlaying }: RadioPlayerProps) {
  const [frequency, setFrequency] = useState(101.5);
  const [band, setBand] = useState<Band>('FM');
  const [volume, setVolume] = useState(75);
  const [signalStrength, setSignalStrength] = useState(85);
  const [equalization, setEqualization] = useState('flat');
  
  const [showFrequencyKeypad, setShowFrequencyKeypad] = useState(false);
  const [showBandSelector, setShowBandSelector] = useState(false);
  const [showEqSelector, setShowEqSelector] = useState(false);
  const [showSaveDialog, setShowSaveDialog] = useState(false);

  const handleFrequencySet = (value: number) => {
    setFrequency(value);
    setShowFrequencyKeypad(false);
    // API call: POST /api/radio/frequency
  };

  const handleBandChange = (values: string[]) => {
    if (values.length > 0) {
      setBand(values[0] as Band);
      // API call: POST /api/radio/band
    }
    setShowBandSelector(false);
  };

  const handleEqChange = (values: string[]) => {
    if (values.length > 0) {
      setEqualization(values[0]);
      // API call: POST /api/radio/equalization
    }
    setShowEqSelector(false);
  };

  const handleSave = (name: string) => {
    console.log('Saving station:', name, frequency, band);
    setShowSaveDialog(false);
    // API call: POST /api/radio/save-station
  };

  const tuneUp = () => {
    const step = band === 'FM' ? 0.2 : 10;
    setFrequency(prev => prev + step);
    // API call: POST /api/radio/tune
  };

  const tuneDown = () => {
    const step = band === 'FM' ? 0.2 : 10;
    setFrequency(prev => Math.max(0, prev - step));
    // API call: POST /api/radio/tune
  };

  return (
    <div className="h-full flex items-center justify-center p-6 relative">
      {/* Background Radio Icon */}
      <div className="absolute inset-0 flex items-center justify-center opacity-10">
        <Radio className="w-64 h-64" />
      </div>

      {/* Main Content */}
      <div className="relative z-10 flex flex-col items-center gap-6 w-full">
        {/* Frequency Display - Large Legacy LED Font */}
        <div className="text-center">
          <div 
            className="font-mono text-green-400 tracking-widest mb-2"
            style={{ 
              fontSize: '3.5rem',
              textShadow: '0 0 20px rgba(34, 197, 94, 0.6)',
              fontFamily: '"Courier New", monospace'
            }}
          >
            {band === 'FM' ? frequency.toFixed(1) : frequency.toFixed(0)}
          </div>
          {/* Band - Legacy LED Font */}
          <div 
            className="font-mono text-green-400 tracking-wider"
            style={{ 
              fontSize: '1.5rem',
              textShadow: '0 0 15px rgba(34, 197, 94, 0.5)',
              fontFamily: '"Courier New", monospace'
            }}
          >
            {band}
          </div>
        </div>

        {/* Status Indicators */}
        <div className="flex gap-8 items-center">
          <div className="flex items-center gap-2">
            <Volume2 className="w-5 h-5 text-gray-400" />
            <span className="font-mono text-sm">{volume}%</span>
          </div>
          <div className="flex items-center gap-2">
            <Signal className="w-5 h-5 text-gray-400" />
            <span className="font-mono text-sm">{signalStrength}%</span>
          </div>
          <div className="flex items-center gap-2">
            <Sliders className="w-5 h-5 text-gray-400" />
            <span className="text-sm capitalize">{equalization.replace('-', ' ')}</span>
          </div>
        </div>

        {/* Controls Grid */}
        <div className="grid grid-cols-4 gap-3 w-full max-w-2xl">
          {/* Row 1 */}
          <button
            onClick={() => setShowFrequencyKeypad(true)}
            className="flex flex-col items-center gap-2 p-4 bg-gray-700 hover:bg-gray-600 rounded-lg transition-colors touch-manipulation"
          >
            <Hash className="w-6 h-6" />
            <span className="text-xs">Set Freq</span>
          </button>

          <button
            onClick={() => setShowBandSelector(true)}
            className="flex flex-col items-center gap-2 p-4 bg-gray-700 hover:bg-gray-600 rounded-lg transition-colors touch-manipulation"
          >
            <Radio className="w-6 h-6" />
            <span className="text-xs">Band</span>
          </button>

          <button
            onClick={tuneUp}
            className="flex flex-col items-center gap-2 p-4 bg-gray-700 hover:bg-gray-600 rounded-lg transition-colors touch-manipulation"
          >
            <ChevronUp className="w-6 h-6" />
            <span className="text-xs">Tune Up</span>
          </button>

          <button
            onClick={tuneDown}
            className="flex flex-col items-center gap-2 p-4 bg-gray-700 hover:bg-gray-600 rounded-lg transition-colors touch-manipulation"
          >
            <ChevronDown className="w-6 h-6" />
            <span className="text-xs">Tune Down</span>
          </button>

          {/* Row 2 */}
          <button
            onClick={() => {
              setFrequency(prev => prev + (band === 'FM' ? 0.2 : 10));
            }}
            className="flex flex-col items-center gap-2 p-4 bg-gray-700 hover:bg-gray-600 rounded-lg transition-colors touch-manipulation"
          >
            <ChevronUp className="w-6 h-6" />
            <span className="text-xs">Scan Up</span>
          </button>

          <button
            onClick={() => {
              setFrequency(prev => Math.max(0, prev - (band === 'FM' ? 0.2 : 10)));
            }}
            className="flex flex-col items-center gap-2 p-4 bg-gray-700 hover:bg-gray-600 rounded-lg transition-colors touch-manipulation"
          >
            <ChevronDown className="w-6 h-6" />
            <span className="text-xs">Scan Down</span>
          </button>

          <button
            onClick={() => setShowEqSelector(true)}
            className="flex flex-col items-center gap-2 p-4 bg-gray-700 hover:bg-gray-600 rounded-lg transition-colors touch-manipulation"
          >
            <Sliders className="w-6 h-6" />
            <span className="text-xs">EQ</span>
          </button>

          <button
            onClick={() => setShowSaveDialog(true)}
            className="flex flex-col items-center gap-2 p-4 bg-green-700 hover:bg-green-600 rounded-lg transition-colors touch-manipulation"
          >
            <Save className="w-6 h-6" />
            <span className="text-xs">Save</span>
          </button>
        </div>
      </div>

      {/* Dialogs */}
      {showFrequencyKeypad && (
        <NumericKeypad
          title="Set Frequency"
          initialValue={frequency}
          min={band === 'FM' ? 88 : 530}
          max={band === 'FM' ? 108 : 1700}
          decimalPlaces={band === 'FM' ? 1 : 0}
          onConfirm={handleFrequencySet}
          onCancel={() => setShowFrequencyKeypad(false)}
        />
      )}

      {showBandSelector && (
        <MultiSelectDialog
          title="Select Band"
          options={BAND_OPTIONS}
          selectedValues={[band]}
          onConfirm={handleBandChange}
          onCancel={() => setShowBandSelector(false)}
          maxSelections={1}
        />
      )}

      {showEqSelector && (
        <MultiSelectDialog
          title="Select Equalization"
          options={EQ_PRESETS}
          selectedValues={[equalization]}
          onConfirm={handleEqChange}
          onCancel={() => setShowEqSelector(false)}
          maxSelections={1}
        />
      )}

      {showSaveDialog && (
        <KeyboardDialog
          title="Save Station"
          placeholder="Enter station name..."
          initialValue=""
          onConfirm={handleSave}
          onCancel={() => setShowSaveDialog(false)}
        />
      )}
    </div>
  );
}
