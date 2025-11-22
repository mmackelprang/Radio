import { useState } from 'react';
import { X, MessageCircle, Check } from 'lucide-react';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../ui/select';
import { Slider } from '../ui/slider';

interface TTSPromptDialogProps {
  onConfirm: (name: string, text: string, voice: string, speed: number) => void;
  onCancel: () => void;
}

const VOICE_OPTIONS = [
  { value: 'en-US-Neural2-A', label: 'US Female (A)' },
  { value: 'en-US-Neural2-B', label: 'US Female (B)' },
  { value: 'en-US-Neural2-C', label: 'US Male (C)' },
  { value: 'en-US-Neural2-D', label: 'US Male (D)' },
  { value: 'en-GB-Neural2-A', label: 'UK Female (A)' },
  { value: 'en-GB-Neural2-B', label: 'UK Male (B)' },
  { value: 'en-AU-Neural2-A', label: 'AU Female (A)' },
  { value: 'en-AU-Neural2-B', label: 'AU Male (B)' }
];

export function TTSPromptDialog({ onConfirm, onCancel }: TTSPromptDialogProps) {
  const [name, setName] = useState('');
  const [text, setText] = useState('');
  const [voice, setVoice] = useState('en-US-Neural2-A');
  const [speed, setSpeed] = useState(1.0);

  const handleConfirm = () => {
    if (name.trim() && text.trim()) {
      onConfirm(name.trim(), text.trim(), voice, speed);
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && e.ctrlKey) {
      handleConfirm();
    } else if (e.key === 'Escape') {
      onCancel();
    }
  };

  return (
    <div className="fixed inset-0 bg-black/80 flex items-center justify-center z-50">
      <div className="bg-gray-800 rounded-xl border border-gray-700 p-6 w-[700px] shadow-2xl flex flex-col gap-4">
        {/* Header */}
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <MessageCircle className="w-6 h-6 text-blue-400" />
            <h3 className="text-lg">Add TTS Prompt</h3>
          </div>
          <button
            onClick={onCancel}
            className="p-2 hover:bg-gray-700 rounded-lg transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Prompt Name */}
        <div>
          <label className="block text-sm text-gray-400 mb-2">Prompt Name</label>
          <input
            type="text"
            value={name}
            onChange={(e) => setName(e.target.value)}
            onKeyDown={handleKeyDown}
            placeholder="Enter prompt name..."
            className="w-full px-4 py-3 bg-gray-700 border border-gray-600 rounded-lg focus:border-blue-500 outline-none transition-colors text-lg"
            autoFocus
          />
        </div>

        {/* Text to Speak */}
        <div>
          <label className="block text-sm text-gray-400 mb-2">Text to Speak</label>
          <textarea
            value={text}
            onChange={(e) => setText(e.target.value)}
            placeholder="Enter text to speak..."
            rows={4}
            className="w-full px-4 py-3 bg-gray-700 border border-gray-600 rounded-lg focus:border-blue-500 outline-none transition-colors text-lg resize-none"
          />
          <div className="text-xs text-gray-500 mt-1">
            {text.length} characters
          </div>
        </div>

        {/* Voice Selection */}
        <div>
          <label className="block text-sm text-gray-400 mb-2">Voice</label>
          <Select value={voice} onValueChange={setVoice}>
            <SelectTrigger className="w-full bg-gray-700 border-gray-600 h-12">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {VOICE_OPTIONS.map(option => (
                <SelectItem key={option.value} value={option.value}>
                  {option.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        {/* Speed Control */}
        <div>
          <label className="block text-sm text-gray-400 mb-2">
            Speed: {speed.toFixed(1)}x
          </label>
          <div className="flex items-center gap-4">
            <span className="text-sm text-gray-500 w-12">0.5x</span>
            <Slider
              value={[speed * 10]}
              onValueChange={(values) => setSpeed(values[0] / 10)}
              min={5}
              max={20}
              step={1}
              className="flex-1 cursor-pointer"
            />
            <span className="text-sm text-gray-500 w-12">2.0x</span>
          </div>
          <div className="flex justify-between text-xs text-gray-500 mt-2">
            <button
              onClick={() => setSpeed(0.5)}
              className="hover:text-gray-300 transition-colors"
            >
              Slow
            </button>
            <button
              onClick={() => setSpeed(1.0)}
              className="hover:text-gray-300 transition-colors"
            >
              Normal
            </button>
            <button
              onClick={() => setSpeed(1.5)}
              className="hover:text-gray-300 transition-colors"
            >
              Fast
            </button>
          </div>
        </div>

        {/* Actions */}
        <div className="flex gap-3 mt-2">
          <button
            onClick={onCancel}
            className="flex-1 py-3 bg-gray-700 hover:bg-gray-600 rounded-lg transition-colors touch-manipulation"
          >
            Cancel
          </button>
          <button
            onClick={handleConfirm}
            disabled={!name.trim() || !text.trim()}
            className="flex-1 py-3 bg-green-600 hover:bg-green-700 rounded-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed touch-manipulation flex items-center justify-center gap-2"
          >
            <Check className="w-5 h-5" />
            Add Prompt
          </button>
        </div>

        {/* Help Text */}
        <div className="text-xs text-gray-500 text-center">
          Press Ctrl+Enter to confirm, Esc to cancel
        </div>
      </div>
    </div>
  );
}
