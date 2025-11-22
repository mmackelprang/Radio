import { useEffect, useState } from 'react';
import { Package, Clock, BookOpen, FileText } from 'lucide-react';
import { ScrollArea } from '../ui/scroll-area';

export function SoftwareInfo() {
  const [uptime, setUptime] = useState(0);

  useEffect(() => {
    // Mock uptime counter - would come from API: GET /api/system/info
    const startTime = Date.now();
    const interval = setInterval(() => {
      setUptime(Math.floor((Date.now() - startTime) / 1000));
    }, 1000);

    return () => clearInterval(interval);
  }, []);

  const formatUptime = (seconds: number) => {
    const days = Math.floor(seconds / 86400);
    const hours = Math.floor((seconds % 86400) / 3600);
    const mins = Math.floor((seconds % 3600) / 60);
    const secs = seconds % 60;
    return `${days}d ${hours}h ${mins}m ${secs}s`;
  };

  const softwareInfo = {
    version: '2.4.1',
    buildDate: '2024-11-15',
    libraries: [
      { name: 'React', version: '18.3.1', license: 'MIT' },
      { name: 'Tailwind CSS', version: '4.0.0', license: 'MIT' },
      { name: 'Lucide Icons', version: '0.263.1', license: 'ISC' },
      { name: 'Recharts', version: '2.10.3', license: 'MIT' },
      { name: 'React Hook Form', version: '7.55.0', license: 'MIT' }
    ]
  };

  return (
    <ScrollArea className="h-full p-6">
      <div className="space-y-6 max-w-4xl">
        {/* Software Version */}
        <div className="bg-gray-900 rounded-lg p-6 border border-gray-700">
          <div className="flex items-center gap-3 mb-4">
            <Package className="w-6 h-6 text-blue-400" />
            <h3 className="text-xl">Software Version</h3>
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <div className="text-sm text-gray-400 mb-1">Version</div>
              <div className="font-mono text-2xl text-green-400">{softwareInfo.version}</div>
            </div>
            <div>
              <div className="text-sm text-gray-400 mb-1">Build Date</div>
              <div className="font-mono">{softwareInfo.buildDate}</div>
            </div>
          </div>
        </div>

        {/* Uptime */}
        <div className="bg-gray-900 rounded-lg p-6 border border-gray-700">
          <div className="flex items-center gap-3 mb-4">
            <Clock className="w-6 h-6 text-green-400" />
            <h3 className="text-xl">System Uptime</h3>
          </div>
          <div className="font-mono text-2xl text-green-400" style={{ 
            textShadow: '0 0 10px rgba(34, 197, 94, 0.5)'
          }}>
            {formatUptime(uptime)}
          </div>
        </div>

        {/* Software Libraries */}
        <div className="bg-gray-900 rounded-lg p-6 border border-gray-700">
          <div className="flex items-center gap-3 mb-4">
            <BookOpen className="w-6 h-6 text-purple-400" />
            <h3 className="text-xl">Software Libraries</h3>
          </div>
          <div className="space-y-3">
            {softwareInfo.libraries.map(lib => (
              <div key={lib.name} className="flex items-center justify-between p-3 bg-gray-800 rounded-lg">
                <div className="flex-1">
                  <div className="font-medium">{lib.name}</div>
                  <div className="text-sm text-gray-400">Version {lib.version}</div>
                </div>
                <div className="text-sm text-gray-500 font-mono">{lib.license}</div>
              </div>
            ))}
          </div>
        </div>

        {/* License Info */}
        <div className="bg-gray-900 rounded-lg p-6 border border-gray-700">
          <div className="flex items-center gap-3 mb-4">
            <FileText className="w-6 h-6 text-yellow-400" />
            <h3 className="text-xl">License Information</h3>
          </div>
          <div className="text-sm text-gray-400 space-y-2">
            <p>
              This software is provided under a proprietary license. All rights reserved.
            </p>
            <p>
              For licensing inquiries, please contact the system administrator.
            </p>
            <div className="mt-4 p-3 bg-gray-800 rounded font-mono text-xs">
              © 2024 Embedded Audio Controller System
            </div>
          </div>
        </div>
      </div>
    </ScrollArea>
  );
}
