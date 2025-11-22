import { useState } from 'react';
import { Plus, Save, Download, Upload, Power } from 'lucide-react';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../ui/select';
import { ScrollArea } from '../ui/scroll-area';
import { FileSelectionDialog } from '../dialogs/FileSelectionDialog';

interface ConfigItem {
  category: string;
  key: string;
  value: string;
}

// Mock configuration data - would come from API: GET /api/config/{component}
const MOCK_CONFIG: { [component: string]: ConfigItem[] } = {
  'Audio': [
    { category: 'Playback', key: 'default_volume', value: '50' },
    { category: 'Playback', key: 'balance', value: '0' },
    { category: 'Playback', key: 'sample_rate', value: '48000' },
    { category: 'Effects', key: 'eq_enabled', value: 'true' },
    { category: 'Effects', key: 'reverb', value: 'false' }
  ],
  'Network': [
    { category: 'Connection', key: 'wifi_ssid', value: 'AudioDevice_5G' },
    { category: 'Connection', key: 'ip_address', value: '192.168.1.100' },
    { category: 'Connection', key: 'gateway', value: '192.168.1.1' },
    { category: 'API', key: 'api_endpoint', value: 'https://api.example.com' },
    { category: 'API', key: 'api_timeout', value: '5000' }
  ],
  'Display': [
    { category: 'Screen', key: 'brightness', value: '80' },
    { category: 'Screen', key: 'timeout', value: '300' },
    { category: 'Theme', key: 'accent_color', value: '#3b82f6' },
    { category: 'Theme', key: 'led_color', value: '#22c55e' }
  ],
  'Radio': [
    { category: 'Tuner', key: 'default_band', value: 'FM' },
    { category: 'Tuner', key: 'scan_sensitivity', value: '75' },
    { category: 'Tuner', key: 'rds_enabled', value: 'true' }
  ]
};

export function ConfigManagement() {
  const [selectedComponent, setSelectedComponent] = useState('Audio');
  const [configItems, setConfigItems] = useState<ConfigItem[]>(MOCK_CONFIG[selectedComponent]);
  const [editingIndex, setEditingIndex] = useState<number | null>(null);
  const [showFileDialog, setShowFileDialog] = useState(false);

  const handleComponentChange = (component: string) => {
    setSelectedComponent(component);
    setConfigItems(MOCK_CONFIG[component] || []);
    setEditingIndex(null);
  };

  const handleAddRow = () => {
    setConfigItems([...configItems, { category: '', key: '', value: '' }]);
    setEditingIndex(configItems.length);
  };

  const handleSave = () => {
    console.log('Saving configuration:', selectedComponent, configItems);
    // API call: POST /api/config/{component}
    setEditingIndex(null);
  };

  const handleBackup = () => {
    console.log('Backing up configuration');
    // API call: POST /api/config/backup
  };

  const handleRestore = () => {
    setShowFileDialog(true);
  };

  const handleFileSelect = (filePath: string) => {
    console.log('Restoring configuration from:', filePath);
    // API call: POST /api/config/restore with file path
    setShowFileDialog(false);
  };

  const handleShutdown = () => {
    if (confirm('Are you sure you want to shutdown the system?')) {
      console.log('Shutting down system');
      // API call: POST /api/system/shutdown
    }
  };

  const updateItem = (index: number, field: keyof ConfigItem, value: string) => {
    const updated = [...configItems];
    updated[index] = { ...updated[index], [field]: value };
    setConfigItems(updated);
  };

  return (
    <div className="h-full flex flex-col p-6">
      {/* Component Selection */}
      <div className="mb-6">
        <label className="block text-sm text-gray-400 mb-2">Select Component</label>
        <Select value={selectedComponent} onValueChange={handleComponentChange}>
          <SelectTrigger className="w-64 bg-gray-700 border-gray-600">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            {Object.keys(MOCK_CONFIG).map(component => (
              <SelectItem key={component} value={component}>
                {component}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      {/* Configuration Grid */}
      <div className="flex-1 min-h-0 bg-gray-900 rounded-lg border border-gray-700 overflow-hidden flex flex-col">
        {/* Header */}
        <div className="grid grid-cols-3 gap-4 p-4 border-b border-gray-700 bg-gray-800">
          <div className="text-sm text-gray-400">Category</div>
          <div className="text-sm text-gray-400">Key</div>
          <div className="text-sm text-gray-400">Value</div>
        </div>

        {/* Rows */}
        <ScrollArea className="flex-1">
          <div className="p-4 space-y-2">
            {configItems.map((item, index) => (
              <div key={index} className="grid grid-cols-3 gap-4">
                <input
                  type="text"
                  value={item.category}
                  onChange={(e) => updateItem(index, 'category', e.target.value)}
                  onFocus={() => setEditingIndex(index)}
                  className="px-3 py-2 bg-gray-800 border border-gray-700 rounded focus:border-blue-500 outline-none transition-colors"
                  placeholder="Category"
                />
                <input
                  type="text"
                  value={item.key}
                  onChange={(e) => updateItem(index, 'key', e.target.value)}
                  onFocus={() => setEditingIndex(index)}
                  className="px-3 py-2 bg-gray-800 border border-gray-700 rounded focus:border-blue-500 outline-none transition-colors"
                  placeholder="Key"
                />
                <input
                  type="text"
                  value={item.value}
                  onChange={(e) => updateItem(index, 'value', e.target.value)}
                  onFocus={() => setEditingIndex(index)}
                  className="px-3 py-2 bg-gray-800 border border-gray-700 rounded focus:border-blue-500 outline-none transition-colors"
                  placeholder="Value"
                />
              </div>
            ))}
          </div>
        </ScrollArea>
      </div>

      {/* Controls */}
      <div className="mt-6 flex flex-wrap gap-3">
        <button
          onClick={handleAddRow}
          className="flex items-center gap-2 px-4 py-3 bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors touch-manipulation"
        >
          <Plus className="w-5 h-5" />
          <span>Add Row</span>
        </button>

        <button
          onClick={handleSave}
          className="flex items-center gap-2 px-4 py-3 bg-green-600 hover:bg-green-700 rounded-lg transition-colors touch-manipulation"
        >
          <Save className="w-5 h-5" />
          <span>Save Changes</span>
        </button>

        <button
          onClick={handleBackup}
          className="flex items-center gap-2 px-4 py-3 bg-gray-700 hover:bg-gray-600 rounded-lg transition-colors touch-manipulation"
        >
          <Download className="w-5 h-5" />
          <span>Backup Config</span>
        </button>

        <button
          onClick={handleRestore}
          className="flex items-center gap-2 px-4 py-3 bg-gray-700 hover:bg-gray-600 rounded-lg transition-colors touch-manipulation"
        >
          <Upload className="w-5 h-5" />
          <span>Restore Config</span>
        </button>

        <div className="flex-1" />

        <button
          onClick={handleShutdown}
          className="flex items-center gap-2 px-4 py-3 bg-red-700 hover:bg-red-600 rounded-lg transition-colors touch-manipulation"
        >
          <Power className="w-5 h-5" />
          <span>System Shutdown</span>
        </button>
      </div>

      {/* File Selection Dialog */}
      {showFileDialog && (
        <FileSelectionDialog
          title="Select Configuration File to Restore"
          filter=".json,.config"
          onConfirm={handleFileSelect}
          onCancel={() => setShowFileDialog(false)}
        />
      )}
    </div>
  );
}