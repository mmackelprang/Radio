import { useState } from 'react';
import { X, Delete, Check } from 'lucide-react';

interface KeyboardDialogProps {
  title: string;
  placeholder?: string;
  initialValue: string;
  onConfirm: (value: string) => void;
  onCancel: () => void;
}

const KEYBOARD_LAYOUT = [
  ['1', '2', '3', '4', '5', '6', '7', '8', '9', '0'],
  ['q', 'w', 'e', 'r', 't', 'y', 'u', 'i', 'o', 'p'],
  ['a', 's', 'd', 'f', 'g', 'h', 'j', 'k', 'l'],
  ['z', 'x', 'c', 'v', 'b', 'n', 'm']
];

export function KeyboardDialog({
  title,
  placeholder = '',
  initialValue,
  onConfirm,
  onCancel
}: KeyboardDialogProps) {
  const [value, setValue] = useState(initialValue);
  const [capsLock, setCapsLock] = useState(false);

  const handleKeyClick = (key: string) => {
    const char = capsLock ? key.toUpperCase() : key;
    setValue(prev => prev + char);
  };

  const handleSpace = () => {
    setValue(prev => prev + ' ');
  };

  const handleBackspace = () => {
    setValue(prev => prev.slice(0, -1));
  };

  const handleClear = () => {
    setValue('');
  };

  return (
    <div className="fixed inset-0 bg-black/80 flex items-center justify-center z-50">
      <div className="bg-gray-800 rounded-xl border border-gray-700 p-6 w-[700px] shadow-2xl">
        {/* Header */}
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg">{title}</h3>
          <button
            onClick={onCancel}
            className="p-2 hover:bg-gray-700 rounded-lg transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Text Input Display */}
        <div className="bg-gray-900 rounded-lg p-4 mb-4">
          <input
            type="text"
            value={value}
            onChange={(e) => setValue(e.target.value)}
            placeholder={placeholder}
            className="w-full bg-transparent text-xl text-white outline-none"
          />
        </div>

        {/* Keyboard */}
        <div className="space-y-2 mb-4">
          {KEYBOARD_LAYOUT.map((row, rowIndex) => (
            <div key={rowIndex} className="flex justify-center gap-2">
              {row.map(key => (
                <button
                  key={key}
                  onClick={() => handleKeyClick(key)}
                  className="w-12 h-12 bg-gray-700 hover:bg-gray-600 rounded-lg transition-colors touch-manipulation flex items-center justify-center"
                >
                  {capsLock ? key.toUpperCase() : key}
                </button>
              ))}
            </div>
          ))}
          
          {/* Bottom Row */}
          <div className="flex justify-center gap-2">
            <button
              onClick={() => setCapsLock(!capsLock)}
              className={`h-12 px-4 rounded-lg transition-colors touch-manipulation flex items-center gap-2 ${
                capsLock ? 'bg-blue-600 hover:bg-blue-700' : 'bg-gray-700 hover:bg-gray-600'
              }`}
            >
              <span className="text-sm">⇪ CAPS</span>
            </button>
            
            <button
              onClick={handleSpace}
              className="flex-1 h-12 bg-gray-700 hover:bg-gray-600 rounded-lg transition-colors touch-manipulation"
            >
              Space
            </button>
            
            <button
              onClick={handleBackspace}
              className="h-12 px-4 bg-gray-700 hover:bg-gray-600 rounded-lg transition-colors touch-manipulation flex items-center gap-2"
            >
              <Delete className="w-4 h-4" />
              Back
            </button>
            
            <button
              onClick={handleClear}
              className="h-12 px-4 bg-red-700 hover:bg-red-600 rounded-lg transition-colors touch-manipulation"
            >
              Clear
            </button>
          </div>
        </div>

        {/* Actions */}
        <div className="flex gap-2">
          <button
            onClick={onCancel}
            className="flex-1 py-3 bg-gray-700 hover:bg-gray-600 rounded-lg transition-colors touch-manipulation"
          >
            Cancel
          </button>
          <button
            onClick={() => onConfirm(value)}
            disabled={!value.trim()}
            className="flex-1 py-3 bg-green-600 hover:bg-green-700 rounded-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed touch-manipulation flex items-center justify-center gap-2"
          >
            <Check className="w-5 h-5" />
            Confirm
          </button>
        </div>
      </div>
    </div>
  );
}
