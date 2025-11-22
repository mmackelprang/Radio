import { useState, useEffect } from 'react';
import { X, Cast, Wifi, Check, RefreshCw } from 'lucide-react';
import { ScrollArea } from '../ui/scroll-area';

interface CastDeviceDialogProps {
  onConfirm: (deviceId: string) => void;
  onCancel: () => void;
  selectedDevice: string;
}

interface CastDevice {
  id: string;
  name: string;
  type: string;
  status: 'available' | 'connected' | 'busy';
  signalStrength: number;
}

// Mock cast devices - would come from API: GET /api/googlecast/devices
const MOCK_DEVICES: CastDevice[] = [
  { id: '1', name: 'Living Room TV', type: 'Chromecast', status: 'available', signalStrength: 95 },
  { id: '2', name: 'Bedroom Speaker', type: 'Google Home', status: 'available', signalStrength: 80 },
  { id: '3', name: 'Kitchen Display', type: 'Nest Hub', status: 'connected', signalStrength: 88 },
  { id: '4', name: 'Office Monitor', type: 'Chromecast Ultra', status: 'available', signalStrength: 70 },
  { id: '5', name: 'Basement TV', type: 'Chromecast', status: 'busy', signalStrength: 60 }
];

export function CastDeviceDialog({ onConfirm, onCancel, selectedDevice }: CastDeviceDialogProps) {
  const [devices, setDevices] = useState<CastDevice[]>(MOCK_DEVICES);
  const [isScanning, setIsScanning] = useState(false);
  const [selectedId, setSelectedId] = useState<string>(selectedDevice);

  const handleScan = () => {
    setIsScanning(true);
    // API call: POST /api/googlecast/scan
    setTimeout(() => {
      setIsScanning(false);
    }, 2000);
  };

  const handleDeviceClick = (deviceId: string, status: string) => {
    if (status !== 'busy') {
      setSelectedId(deviceId);
    }
  };

  const handleConfirm = () => {
    if (selectedId) {
      onConfirm(selectedId);
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'connected':
        return 'text-green-400';
      case 'busy':
        return 'text-red-400';
      default:
        return 'text-gray-400';
    }
  };

  const getStatusLabel = (status: string) => {
    switch (status) {
      case 'connected':
        return 'Connected';
      case 'busy':
        return 'In Use';
      default:
        return 'Available';
    }
  };

  return (
    <div className="fixed inset-0 bg-black/80 flex items-center justify-center z-50">
      <div className="bg-gray-800 rounded-xl border border-gray-700 p-6 w-[600px] h-[500px] shadow-2xl flex flex-col">
        {/* Header */}
        <div className="flex items-center justify-between mb-4">
          <div className="flex items-center gap-3">
            <Cast className="w-6 h-6 text-blue-400" />
            <h3 className="text-lg">Select Cast Device</h3>
          </div>
          <div className="flex items-center gap-2">
            <button
              onClick={handleScan}
              disabled={isScanning}
              className="p-2 bg-gray-700 hover:bg-gray-600 rounded-lg transition-colors disabled:opacity-50"
              title="Scan for devices"
            >
              <RefreshCw className={`w-5 h-5 ${isScanning ? 'animate-spin' : ''}`} />
            </button>
            <button
              onClick={onCancel}
              className="p-2 hover:bg-gray-700 rounded-lg transition-colors"
            >
              <X className="w-5 h-5" />
            </button>
          </div>
        </div>

        {/* Scanning Status */}
        {isScanning && (
          <div className="bg-blue-900/30 border border-blue-700 rounded-lg p-3 mb-4 flex items-center gap-3">
            <RefreshCw className="w-5 h-5 text-blue-400 animate-spin" />
            <span className="text-sm text-blue-400">Scanning for Cast devices...</span>
          </div>
        )}

        {/* Device List */}
        <ScrollArea className="flex-1 -mx-2 mb-4">
          <div className="space-y-2 px-2">
            {devices.map(device => {
              const isSelected = selectedId === device.id;
              const isDisabled = device.status === 'busy';
              
              return (
                <button
                  key={device.id}
                  onClick={() => handleDeviceClick(device.id, device.status)}
                  disabled={isDisabled}
                  className={`w-full p-4 rounded-lg transition-all touch-manipulation text-left ${
                    isSelected
                      ? 'bg-blue-600 hover:bg-blue-700'
                      : isDisabled
                      ? 'bg-gray-700/50 opacity-50 cursor-not-allowed'
                      : 'bg-gray-700 hover:bg-gray-600'
                  }`}
                >
                  <div className="flex items-center gap-4">
                    {/* Device Icon */}
                    <div className={`p-3 rounded-lg ${
                      isSelected ? 'bg-blue-700' : 'bg-gray-800'
                    }`}>
                      <Cast className="w-6 h-6" />
                    </div>

                    {/* Device Info */}
                    <div className="flex-1">
                      <div className="flex items-center gap-2 mb-1">
                        <span className="font-medium">{device.name}</span>
                        {isSelected && <Check className="w-4 h-4" />}
                      </div>
                      <div className="text-sm text-gray-400">{device.type}</div>
                      <div className={`text-xs mt-1 ${getStatusColor(device.status)}`}>
                        {getStatusLabel(device.status)}
                      </div>
                    </div>

                    {/* Signal Strength */}
                    <div className="flex flex-col items-end gap-1">
                      <Wifi className={`w-5 h-5 ${
                        device.signalStrength > 70 ? 'text-green-400' :
                        device.signalStrength > 40 ? 'text-yellow-400' :
                        'text-red-400'
                      }`} />
                      <span className="text-xs text-gray-400">{device.signalStrength}%</span>
                    </div>
                  </div>
                </button>
              );
            })}
            {devices.length === 0 && !isScanning && (
              <div className="text-center py-12 text-gray-500">
                <Cast className="w-12 h-12 mx-auto mb-3 opacity-50" />
                <div>No Cast devices found</div>
                <div className="text-sm mt-2">Click the refresh button to scan</div>
              </div>
            )}
          </div>
        </ScrollArea>

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
            disabled={!selectedId}
            className="flex-1 py-3 bg-green-600 hover:bg-green-700 rounded-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed touch-manipulation flex items-center justify-center gap-2"
          >
            <Check className="w-5 h-5" />
            Connect
          </button>
        </div>
      </div>
    </div>
  );
}
