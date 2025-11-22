import { useState } from 'react';
import { Save } from 'lucide-react';
import { ScrollArea } from '../ui/scroll-area';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../ui/select';

interface CoreParameter {
  id: string;
  name: string;
  type: 'int' | 'string';
  value: string;
  description?: string;
}

// Mock core parameters - would come from API: GET /api/parameters/core
const MOCK_PARAMETERS: CoreParameter[] = [
  { id: '1', name: 'max_volume', type: 'int', value: '100', description: 'Maximum allowed volume level' },
  { id: '2', name: 'default_input', type: 'string', value: 'spotify', description: 'Default audio input device' },
  { id: '3', name: 'sample_rate', type: 'int', value: '48000', description: 'Audio sample rate in Hz' },
  { id: '4', name: 'buffer_size', type: 'int', value: '512', description: 'Audio buffer size in samples' },
  { id: '5', name: 'device_name', type: 'string', value: 'AudioController', description: 'Device identifier name' },
  { id: '6', name: 'api_port', type: 'int', value: '8080', description: 'REST API port number' },
  { id: '7', name: 'websocket_port', type: 'int', value: '8081', description: 'WebSocket server port' },
  { id: '8', name: 'log_level', type: 'string', value: 'INFO', description: 'Logging level (DEBUG, INFO, WARN, ERROR)' },
  { id: '9', name: 'auto_play', type: 'int', value: '0', description: 'Auto-play on startup (0=off, 1=on)' },
  { id: '10', name: 'display_timeout', type: 'int', value: '300', description: 'Display timeout in seconds' },
  { id: '11', name: 'network_mode', type: 'string', value: 'auto', description: 'Network mode (auto, wifi, ethernet)' },
  { id: '12', name: 'bluetooth_discoverable', type: 'int', value: '1', description: 'Bluetooth discoverable (0=off, 1=on)' },
  { id: '13', name: 'max_playlist_size', type: 'int', value: '1000', description: 'Maximum playlist items' },
  { id: '14', name: 'cache_size_mb', type: 'int', value: '256', description: 'Cache size in megabytes' },
  { id: '15', name: 'default_equalizer', type: 'string', value: 'flat', description: 'Default equalizer preset' }
];

export function CoreParameters() {
  const [parameters, setParameters] = useState<CoreParameter[]>(MOCK_PARAMETERS);
  const [editingId, setEditingId] = useState<string | null>(null);

  const handleSave = () => {
    console.log('Saving core parameters:', parameters);
    // API call: PUT /api/parameters/core
    setEditingId(null);
  };

  const updateParameter = (id: string, value: string) => {
    setParameters(parameters.map(p => 
      p.id === id ? { ...p, value } : p
    ));
  };

  const validateValue = (param: CoreParameter, value: string): boolean => {
    if (param.type === 'int') {
      return /^-?\d+$/.test(value);
    }
    return true; // String values are always valid
  };

  return (
    <div className="h-full flex flex-col p-6">
      {/* Header Info */}
      <div className="mb-6">
        <h3 className="text-lg mb-2">Core System Parameters</h3>
        <p className="text-sm text-gray-400">
          Edit core system parameters. Changes will take effect after saving.
        </p>
      </div>

      {/* Parameters Grid */}
      <div className="flex-1 min-h-0 bg-gray-900 rounded-lg border border-gray-700 overflow-hidden flex flex-col">
        {/* Header */}
        <div className="grid grid-cols-12 gap-4 p-4 border-b border-gray-700 bg-gray-800">
          <div className="col-span-4 text-sm text-gray-400">Parameter Name</div>
          <div className="col-span-2 text-sm text-gray-400">Type</div>
          <div className="col-span-6 text-sm text-gray-400">Value</div>
        </div>

        {/* Rows */}
        <ScrollArea className="flex-1">
          <div className="p-4 space-y-2">
            {parameters.map((param) => {
              const isEditing = editingId === param.id;
              
              return (
                <div key={param.id}>
                  <div className="grid grid-cols-12 gap-4 items-center">
                    {/* Parameter Name (read-only) */}
                    <div className="col-span-4">
                      <div className="px-3 py-2 bg-gray-800 border border-gray-700 rounded text-gray-300">
                        {param.name}
                      </div>
                    </div>

                    {/* Type (read-only) */}
                    <div className="col-span-2">
                      <div className="px-3 py-2 bg-gray-800 border border-gray-700 rounded">
                        <span className={`inline-block px-2 py-1 rounded text-xs ${
                          param.type === 'int' 
                            ? 'bg-blue-900/50 text-blue-300' 
                            : 'bg-green-900/50 text-green-300'
                        }`}>
                          {param.type}
                        </span>
                      </div>
                    </div>

                    {/* Value (editable) */}
                    <div className="col-span-6">
                      <input
                        type={param.type === 'int' ? 'number' : 'text'}
                        value={param.value}
                        onChange={(e) => {
                          const newValue = e.target.value;
                          if (param.type !== 'int' || validateValue(param, newValue)) {
                            updateParameter(param.id, newValue);
                          }
                        }}
                        onFocus={() => setEditingId(param.id)}
                        onBlur={() => setEditingId(null)}
                        className={`w-full px-3 py-2 bg-gray-800 border rounded outline-none transition-colors ${
                          isEditing 
                            ? 'border-blue-500 bg-gray-750' 
                            : 'border-gray-700'
                        }`}
                        placeholder={`Enter ${param.type} value...`}
                      />
                    </div>
                  </div>

                  {/* Description */}
                  {param.description && (
                    <div className="ml-4 mt-1 text-xs text-gray-500">
                      {param.description}
                    </div>
                  )}
                </div>
              );
            })}
          </div>
        </ScrollArea>
      </div>

      {/* Save Button */}
      <div className="mt-6 flex gap-3">
        <button
          onClick={handleSave}
          className="flex items-center gap-2 px-6 py-3 bg-green-600 hover:bg-green-700 rounded-lg transition-colors touch-manipulation"
        >
          <Save className="w-5 h-5" />
          <span>Save Changes</span>
        </button>
        
        <div className="flex-1" />
        
        <div className="text-sm text-gray-400 flex items-center">
          {editingId && (
            <span className="text-blue-400">Editing...</span>
          )}
        </div>
      </div>

      {/* Info Panel */}
      <div className="mt-4 p-4 bg-blue-900/20 border border-blue-700/50 rounded-lg">
        <div className="text-sm text-blue-300">
          <strong>Note:</strong> Core parameters are fundamental system settings. 
          Changes may require a system restart to take full effect.
        </div>
      </div>
    </div>
  );
}
