import { useState } from 'react';
import { X, Info, Settings, MessageSquare, Sliders } from 'lucide-react';
import { SoftwareInfo } from './system-config/SoftwareInfo';
import { ConfigManagement } from './system-config/ConfigManagement';
import { PromptManagement } from './system-config/PromptManagement';
import { CoreParameters } from './system-config/CoreParameters';

interface SystemConfigProps {
  onClose: () => void;
}

type ConfigTab = 'core' | 'software' | 'config' | 'prompts';

export function SystemConfig({ onClose }: SystemConfigProps) {
  const [activeTab, setActiveTab] = useState<ConfigTab>('core');

  return (
    <div className="h-full bg-gray-800 rounded-lg border border-gray-700 flex flex-col">
      {/* Header */}
      <div className="flex items-center justify-between p-4 border-b border-gray-700">
        <h2 className="text-xl">System Configuration</h2>
        <button
          onClick={onClose}
          className="p-2 hover:bg-gray-700 rounded-lg transition-colors"
        >
          <X className="w-6 h-6" />
        </button>
      </div>

      {/* Tabs */}
      <div className="flex gap-2 p-4 border-b border-gray-700">
        <button
          onClick={() => setActiveTab('core')}
          className={`flex items-center gap-2 px-6 py-3 rounded-lg transition-colors touch-manipulation ${
            activeTab === 'core'
              ? 'bg-blue-600 hover:bg-blue-700'
              : 'bg-gray-700 hover:bg-gray-600'
          }`}
        >
          <Sliders className="w-5 h-5" />
          <span>Core Parameters</span>
        </button>
        
        <button
          onClick={() => setActiveTab('software')}
          className={`flex items-center gap-2 px-6 py-3 rounded-lg transition-colors touch-manipulation ${
            activeTab === 'software'
              ? 'bg-blue-600 hover:bg-blue-700'
              : 'bg-gray-700 hover:bg-gray-600'
          }`}
        >
          <Info className="w-5 h-5" />
          <span>Software Info</span>
        </button>
        
        <button
          onClick={() => setActiveTab('config')}
          className={`flex items-center gap-2 px-6 py-3 rounded-lg transition-colors touch-manipulation ${
            activeTab === 'config'
              ? 'bg-blue-600 hover:bg-blue-700'
              : 'bg-gray-700 hover:bg-gray-600'
          }`}
        >
          <Settings className="w-5 h-5" />
          <span>Configuration</span>
        </button>
        
        <button
          onClick={() => setActiveTab('prompts')}
          className={`flex items-center gap-2 px-6 py-3 rounded-lg transition-colors touch-manipulation ${
            activeTab === 'prompts'
              ? 'bg-blue-600 hover:bg-blue-700'
              : 'bg-gray-700 hover:bg-gray-600'
          }`}
        >
          <MessageSquare className="w-5 h-5" />
          <span>Prompts</span>
        </button>
      </div>

      {/* Content */}
      <div className="flex-1 overflow-hidden">
        {activeTab === 'core' && <CoreParameters />}
        {activeTab === 'software' && <SoftwareInfo />}
        {activeTab === 'config' && <ConfigManagement />}
        {activeTab === 'prompts' && <PromptManagement />}
      </div>
    </div>
  );
}