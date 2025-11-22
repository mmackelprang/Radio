import { Disc3, Power } from 'lucide-react';
import { useState } from 'react';

interface VinylPlayerProps {
  isPlaying: boolean;
}

export function VinylPlayer({ isPlaying }: VinylPlayerProps) {
  const [preampOn, setPreampOn] = useState(true);

  const handlePreampToggle = () => {
    setPreampOn(!preampOn);
    // API call: POST /api/vinyl/preamp
  };

  return (
    <div className="h-full flex items-center justify-center p-6 relative">
      {/* Background Vinyl Icon with Rotation */}
      <div className="absolute inset-0 flex items-center justify-center opacity-20">
        <Disc3 
          className={`w-64 h-64 ${isPlaying ? 'animate-spin' : ''}`}
          style={{ animationDuration: isPlaying ? '2s' : undefined }}
        />
      </div>

      {/* Controls */}
      <div className="relative z-10 flex flex-col items-center gap-8">
        <div className="text-2xl text-gray-300">Vinyl Phonograph</div>
        
        <button
          onClick={handlePreampToggle}
          className={`flex items-center gap-3 px-8 py-4 rounded-lg transition-all touch-manipulation ${
            preampOn 
              ? 'bg-green-600 hover:bg-green-700 shadow-lg shadow-green-600/50' 
              : 'bg-gray-700 hover:bg-gray-600'
          }`}
        >
          <Power className="w-6 h-6" />
          <span className="text-lg">Preamp {preampOn ? 'ON' : 'OFF'}</span>
        </button>

        {preampOn && (
          <div className="text-sm text-green-400 font-mono">
            Preamplifier Active
          </div>
        )}
      </div>
    </div>
  );
}
