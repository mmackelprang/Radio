import { Radio } from 'lucide-react';

interface DefaultPlayerProps {
  inputDevice: string;
  isPlaying: boolean;
}

export function DefaultPlayer({ inputDevice, isPlaying }: DefaultPlayerProps) {
  return (
    <div className="h-full flex items-center justify-center p-6 relative">
      {/* Background Radio Icon */}
      <div className="absolute inset-0 flex items-center justify-center opacity-10">
        <Radio className="w-64 h-64" />
      </div>

      {/* Content */}
      <div className="relative z-10 text-center">
        <div className="text-2xl text-gray-300 mb-4 capitalize">
          {inputDevice.replace('-', ' ')}
        </div>
        <div className="text-gray-500">
          {isPlaying ? 'Playing...' : 'Ready'}
        </div>
      </div>
    </div>
  );
}
