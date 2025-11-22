import { useState } from 'react';
import { X, Check } from 'lucide-react';
import { ScrollArea } from '../ui/scroll-area';

interface MultiSelectOption {
  value: string;
  label: string;
}

interface MultiSelectDialogProps {
  title: string;
  options: MultiSelectOption[];
  selectedValues: string[];
  onConfirm: (values: string[]) => void;
  onCancel: () => void;
  maxSelections?: number;
}

export function MultiSelectDialog({
  title,
  options,
  selectedValues: initialValues,
  onConfirm,
  onCancel,
  maxSelections
}: MultiSelectDialogProps) {
  const [selectedValues, setSelectedValues] = useState<string[]>(initialValues);

  const toggleSelection = (value: string) => {
    setSelectedValues(prev => {
      const isSelected = prev.includes(value);
      
      if (isSelected) {
        return prev.filter(v => v !== value);
      } else {
        if (maxSelections && prev.length >= maxSelections) {
          // Replace the last selection if max is reached
          return [...prev.slice(0, -1), value];
        }
        return [...prev, value];
      }
    });
  };

  return (
    <div className="fixed inset-0 bg-black/80 flex items-center justify-center z-50">
      <div className="bg-gray-800 rounded-xl border border-gray-700 p-6 w-[500px] max-h-[600px] shadow-2xl flex flex-col">
        {/* Header */}
        <div className="flex items-center justify-between mb-4">
          <div>
            <h3 className="text-lg">{title}</h3>
            {maxSelections && (
              <p className="text-sm text-gray-400 mt-1">
                {maxSelections === 1 ? 'Select one option' : `Select up to ${maxSelections} options`}
              </p>
            )}
          </div>
          <button
            onClick={onCancel}
            className="p-2 hover:bg-gray-700 rounded-lg transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Options List */}
        <ScrollArea className="flex-1 -mx-2">
          <div className="space-y-2 px-2">
            {options.map(option => {
              const isSelected = selectedValues.includes(option.value);
              
              return (
                <button
                  key={option.value}
                  onClick={() => toggleSelection(option.value)}
                  className={`w-full p-4 rounded-lg transition-all touch-manipulation text-left flex items-center justify-between ${
                    isSelected
                      ? 'bg-blue-600 hover:bg-blue-700'
                      : 'bg-gray-700 hover:bg-gray-600'
                  }`}
                >
                  <span className="text-lg">{option.label}</span>
                  {isSelected && (
                    <Check className="w-5 h-5" />
                  )}
                </button>
              );
            })}
          </div>
        </ScrollArea>

        {/* Actions */}
        <div className="flex gap-2 mt-4">
          <button
            onClick={onCancel}
            className="flex-1 py-3 bg-gray-700 hover:bg-gray-600 rounded-lg transition-colors touch-manipulation"
          >
            Cancel
          </button>
          <button
            onClick={() => onConfirm(selectedValues)}
            className="flex-1 py-3 bg-green-600 hover:bg-green-700 rounded-lg transition-colors touch-manipulation flex items-center justify-center gap-2"
          >
            <Check className="w-5 h-5" />
            Confirm ({selectedValues.length})
          </button>
        </div>
      </div>
    </div>
  );
}
