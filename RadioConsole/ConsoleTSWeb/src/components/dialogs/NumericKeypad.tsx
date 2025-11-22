import { useState } from 'react';
import { X, Delete, Check } from 'lucide-react';

interface NumericKeypadProps {
  title: string;
  initialValue: number;
  min: number;
  max: number;
  decimalPlaces?: number;
  onConfirm: (value: number) => void;
  onCancel: () => void;
}

export function NumericKeypad({
  title,
  initialValue,
  min,
  max,
  decimalPlaces = 0,
  onConfirm,
  onCancel
}: NumericKeypadProps) {
  const [value, setValue] = useState(initialValue.toString());

  const handleNumberClick = (num: string) => {
    setValue(prev => prev + num);
  };

  const handleDecimalClick = () => {
    if (decimalPlaces > 0 && !value.includes('.')) {
      setValue(prev => prev + '.');
    }
  };

  const handleBackspace = () => {
    setValue(prev => prev.slice(0, -1) || '0');
  };

  const handleClear = () => {
    setValue('0');
  };

  const handleConfirm = () => {
    const numValue = parseFloat(value);
    if (!isNaN(numValue) && numValue >= min && numValue <= max) {
      onConfirm(numValue);
    }
  };

  const currentValue = parseFloat(value);
  const isValid = !isNaN(currentValue) && currentValue >= min && currentValue <= max;

  return (
    <div className="fixed inset-0 bg-black/80 flex items-center justify-center z-50">
      <div className="bg-gray-800 rounded-xl border border-gray-700 p-6 w-96 shadow-2xl">
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

        {/* Display */}
        <div className="bg-gray-900 rounded-lg p-4 mb-4">
          <div className="font-mono text-3xl text-green-400 text-right tracking-wider"
            style={{ textShadow: '0 0 10px rgba(34, 197, 94, 0.5)' }}
          >
            {value || '0'}
          </div>
          <div className="text-xs text-gray-500 text-right mt-1">
            Range: {min} - {max}
          </div>
          {!isValid && value !== '' && (
            <div className="text-xs text-red-400 text-right mt-1">
              Value out of range
            </div>
          )}
        </div>

        {/* Keypad */}
        <div className="grid grid-cols-3 gap-2 mb-4">
          {[1, 2, 3, 4, 5, 6, 7, 8, 9].map(num => (
            <button
              key={num}
              onClick={() => handleNumberClick(num.toString())}
              className="p-4 bg-gray-700 hover:bg-gray-600 rounded-lg text-xl transition-colors touch-manipulation"
            >
              {num}
            </button>
          ))}
          
          {decimalPlaces > 0 ? (
            <button
              onClick={handleDecimalClick}
              disabled={value.includes('.')}
              className="p-4 bg-gray-700 hover:bg-gray-600 rounded-lg text-xl transition-colors disabled:opacity-50 disabled:cursor-not-allowed touch-manipulation"
            >
              .
            </button>
          ) : (
            <button
              onClick={handleClear}
              className="p-4 bg-red-700 hover:bg-red-600 rounded-lg text-xl transition-colors touch-manipulation"
            >
              C
            </button>
          )}
          
          <button
            onClick={() => handleNumberClick('0')}
            className="p-4 bg-gray-700 hover:bg-gray-600 rounded-lg text-xl transition-colors touch-manipulation"
          >
            0
          </button>
          
          <button
            onClick={handleBackspace}
            className="p-4 bg-gray-700 hover:bg-gray-600 rounded-lg transition-colors touch-manipulation"
          >
            <Delete className="w-6 h-6 mx-auto" />
          </button>
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
            onClick={handleConfirm}
            disabled={!isValid}
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
