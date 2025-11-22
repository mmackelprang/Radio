import { useState } from 'react';
import { Plus, Save, Trash2, FileAudio, MessageCircle } from 'lucide-react';
import { ScrollArea } from '../ui/scroll-area';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../ui/select';
import { KeyboardDialog } from '../dialogs/KeyboardDialog';
import { TTSPromptDialog } from '../dialogs/TTSPromptDialog';

interface TTSPrompt {
  id: string;
  name: string;
  type: 'TTS';
  data: string; // Text to speak
  voice: string;
  speed: number;
}

interface FilePrompt {
  id: string;
  name: string;
  type: 'File';
  data: string; // Filename
}

type Prompt = TTSPrompt | FilePrompt;

type PromptType = 'TTS' | 'File';

// Mock prompts - would come from API: GET /api/prompts/tts and /api/prompts/file
const MOCK_TTS_PROMPTS: TTSPrompt[] = [
  { id: '1', name: 'Welcome', type: 'TTS', data: 'Welcome to the audio controller system', voice: 'en-US-Neural2-A', speed: 1.0 },
  { id: '2', name: 'Goodbye', type: 'TTS', data: 'Goodbye, thank you for using the system', voice: 'en-US-Neural2-B', speed: 1.0 },
  { id: '3', name: 'Error', type: 'TTS', data: 'An error has occurred, please try again', voice: 'en-US-Neural2-A', speed: 1.1 },
  { id: '6', name: 'Volume Max', type: 'TTS', data: 'Maximum volume reached', voice: 'en-US-Neural2-C', speed: 0.9 }
];

const MOCK_FILE_PROMPTS: FilePrompt[] = [
  { id: '4', name: 'Startup', type: 'File', data: '/prompts/startup.wav' },
  { id: '5', name: 'Shutdown', type: 'File', data: '/prompts/shutdown.wav' },
  { id: '7', name: 'Low Battery', type: 'File', data: '/prompts/low-battery.wav' }
];

export function PromptManagement() {
  const [promptType, setPromptType] = useState<PromptType>('TTS');
  const [ttsPrompts, setTtsPrompts] = useState<TTSPrompt[]>(MOCK_TTS_PROMPTS);
  const [filePrompts, setFilePrompts] = useState<FilePrompt[]>(MOCK_FILE_PROMPTS);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [showAddDialog, setShowAddDialog] = useState(false);
  const [showKeyboardDialog, setShowKeyboardDialog] = useState(false);
  const [keyboardEditingPrompt, setKeyboardEditingPrompt] = useState<TTSPrompt | null>(null);

  const handleAddTTSPrompt = (name: string, text: string, voice: string, speed: number) => {
    const newPrompt: TTSPrompt = {
      id: Date.now().toString(),
      name: name,
      type: 'TTS',
      data: text,
      voice: voice,
      speed: speed
    };
    setTtsPrompts([...ttsPrompts, newPrompt]);
    setShowAddDialog(false);
    // API call: POST /api/prompts/tts
  };

  const handleAddFilePrompt = (name: string) => {
    const newPrompt: FilePrompt = {
      id: Date.now().toString(),
      name: name,
      type: 'File',
      data: ''
    };
    setFilePrompts([...filePrompts, newPrompt]);
    setEditingId(newPrompt.id);
    setShowAddDialog(false);
    // API call: POST /api/prompts/file
  };

  const handleSave = () => {
    console.log('Saving prompts:', { tts: ttsPrompts, file: filePrompts });
    // API call: PUT /api/prompts/tts/{id} for each modified TTS prompt
    // API call: PUT /api/prompts/file/{id} for each modified file prompt
    setEditingId(null);
  };

  const handleDeleteTTS = (id: string) => {
    if (confirm('Are you sure you want to delete this prompt?')) {
      setTtsPrompts(ttsPrompts.filter(p => p.id !== id));
      // API call: DELETE /api/prompts/tts/{id}
    }
  };

  const handleDeleteFile = (id: string) => {
    if (confirm('Are you sure you want to delete this prompt?')) {
      setFilePrompts(filePrompts.filter(p => p.id !== id));
      // API call: DELETE /api/prompts/file/{id}
    }
  };

  const updateTTSPrompt = (id: string, field: keyof TTSPrompt, value: string | number) => {
    setTtsPrompts(ttsPrompts.map(p => 
      p.id === id ? { ...p, [field]: value } : p
    ));
  };

  const updateFilePrompt = (id: string, field: keyof FilePrompt, value: string) => {
    setFilePrompts(filePrompts.map(p => 
      p.id === id ? { ...p, [field]: value } : p
    ));
  };

  const handleTTSTextClick = (prompt: TTSPrompt) => {
    setKeyboardEditingPrompt(prompt);
    setShowKeyboardDialog(true);
  };

  const handleKeyboardConfirm = (text: string) => {
    if (keyboardEditingPrompt) {
      updateTTSPrompt(keyboardEditingPrompt.id, 'data', text);
    }
    setShowKeyboardDialog(false);
    setKeyboardEditingPrompt(null);
  };

  return (
    <div className="h-full flex flex-col p-6">
      {/* Header Controls */}
      <div className="flex items-center gap-3 mb-6">
        {/* Prompt Type Selector */}
        <div className="flex items-center gap-2 bg-gray-900 rounded-lg p-1 border border-gray-700">
          <button
            onClick={() => setPromptType('TTS')}
            className={`flex items-center gap-2 px-4 py-2 rounded-md transition-colors touch-manipulation ${
              promptType === 'TTS'
                ? 'bg-blue-600 hover:bg-blue-700'
                : 'hover:bg-gray-800'
            }`}
          >
            <MessageCircle className="w-4 h-4" />
            <span>TTS</span>
          </button>
          <button
            onClick={() => setPromptType('File')}
            className={`flex items-center gap-2 px-4 py-2 rounded-md transition-colors touch-manipulation ${
              promptType === 'File'
                ? 'bg-blue-600 hover:bg-blue-700'
                : 'hover:bg-gray-800'
            }`}
          >
            <FileAudio className="w-4 h-4" />
            <span>File</span>
          </button>
        </div>

        <button
          onClick={() => setShowAddDialog(true)}
          className="flex items-center gap-2 px-4 py-3 bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors touch-manipulation"
        >
          <Plus className="w-5 h-5" />
          <span>Add Prompt</span>
        </button>

        <button
          onClick={handleSave}
          className="flex items-center gap-2 px-4 py-3 bg-green-600 hover:bg-green-700 rounded-lg transition-colors touch-manipulation"
        >
          <Save className="w-5 h-5" />
          <span>Save All</span>
        </button>
      </div>

      {/* Prompts List */}
      <div className="flex-1 min-h-0 bg-gray-900 rounded-lg border border-gray-700 overflow-hidden flex flex-col">
        {/* TTS Grid */}
        {promptType === 'TTS' && (
          <>
            {/* Header */}
            <div className="grid grid-cols-12 gap-4 p-4 border-b border-gray-700 bg-gray-800">
              <div className="col-span-2 text-sm text-gray-400">Prompt Name</div>
              <div className="col-span-4 text-sm text-gray-400">Text</div>
              <div className="col-span-2 text-sm text-gray-400">Voice</div>
              <div className="col-span-2 text-sm text-gray-400">Speed</div>
              <div className="col-span-2 text-sm text-gray-400">Actions</div>
            </div>

            {/* Rows */}
            <ScrollArea className="flex-1">
              <div className="p-4 space-y-2">
                {ttsPrompts.map(prompt => (
                  <div key={prompt.id} className="grid grid-cols-12 gap-4 items-center">
                    <input
                      type="text"
                      value={prompt.name}
                      onChange={(e) => updateTTSPrompt(prompt.id, 'name', e.target.value)}
                      onFocus={() => setEditingId(prompt.id)}
                      className="col-span-2 px-3 py-2 bg-gray-800 border border-gray-700 rounded focus:border-blue-500 outline-none transition-colors"
                      placeholder="Prompt Name"
                    />

                    <button
                      onClick={() => handleTTSTextClick(prompt)}
                      className="col-span-4 px-3 py-2 bg-gray-800 border border-gray-700 rounded hover:border-blue-500 transition-colors text-left truncate"
                    >
                      {prompt.data || 'Enter text...'}
                    </button>

                    <div className="col-span-2">
                      <Select 
                        value={prompt.voice} 
                        onValueChange={(value) => updateTTSPrompt(prompt.id, 'voice', value)}
                      >
                        <SelectTrigger className="bg-gray-800 border-gray-700">
                          <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="en-US-Neural2-A">US Female (A)</SelectItem>
                          <SelectItem value="en-US-Neural2-B">US Female (B)</SelectItem>
                          <SelectItem value="en-US-Neural2-C">US Male (C)</SelectItem>
                          <SelectItem value="en-US-Neural2-D">US Male (D)</SelectItem>
                          <SelectItem value="en-GB-Neural2-A">UK Female (A)</SelectItem>
                          <SelectItem value="en-GB-Neural2-B">UK Male (B)</SelectItem>
                        </SelectContent>
                      </Select>
                    </div>

                    <input
                      type="number"
                      value={prompt.speed}
                      onChange={(e) => updateTTSPrompt(prompt.id, 'speed', parseFloat(e.target.value))}
                      onFocus={() => setEditingId(prompt.id)}
                      min="0.5"
                      max="2.0"
                      step="0.1"
                      className="col-span-2 px-3 py-2 bg-gray-800 border border-gray-700 rounded focus:border-blue-500 outline-none transition-colors"
                    />

                    <button
                      onClick={() => handleDeleteTTS(prompt.id)}
                      className="col-span-2 p-2 bg-red-700 hover:bg-red-600 rounded-lg transition-colors touch-manipulation"
                      title="Delete"
                    >
                      <Trash2 className="w-5 h-5 mx-auto" />
                    </button>
                  </div>
                ))}
              </div>
            </ScrollArea>
          </>
        )}

        {/* File Grid */}
        {promptType === 'File' && (
          <>
            {/* Header */}
            <div className="grid grid-cols-12 gap-4 p-4 border-b border-gray-700 bg-gray-800">
              <div className="col-span-4 text-sm text-gray-400">Prompt Name</div>
              <div className="col-span-6 text-sm text-gray-400">File Path</div>
              <div className="col-span-2 text-sm text-gray-400">Actions</div>
            </div>

            {/* Rows */}
            <ScrollArea className="flex-1">
              <div className="p-4 space-y-2">
                {filePrompts.map(prompt => (
                  <div key={prompt.id} className="grid grid-cols-12 gap-4 items-center">
                    <input
                      type="text"
                      value={prompt.name}
                      onChange={(e) => updateFilePrompt(prompt.id, 'name', e.target.value)}
                      onFocus={() => setEditingId(prompt.id)}
                      className="col-span-4 px-3 py-2 bg-gray-800 border border-gray-700 rounded focus:border-blue-500 outline-none transition-colors"
                      placeholder="Prompt Name"
                    />

                    <input
                      type="text"
                      value={prompt.data}
                      onChange={(e) => updateFilePrompt(prompt.id, 'data', e.target.value)}
                      onFocus={() => setEditingId(prompt.id)}
                      className="col-span-6 px-3 py-2 bg-gray-800 border border-gray-700 rounded focus:border-blue-500 outline-none transition-colors"
                      placeholder="Enter filename..."
                    />

                    <button
                      onClick={() => handleDeleteFile(prompt.id)}
                      className="col-span-2 p-2 bg-red-700 hover:bg-red-600 rounded-lg transition-colors touch-manipulation"
                      title="Delete"
                    >
                      <Trash2 className="w-5 h-5 mx-auto" />
                    </button>
                  </div>
                ))}
              </div>
            </ScrollArea>
          </>
        )}
      </div>

      {/* Add TTS Prompt Dialog */}
      {showAddDialog && promptType === 'TTS' && (
        <TTSPromptDialog
          onConfirm={handleAddTTSPrompt}
          onCancel={() => setShowAddDialog(false)}
        />
      )}

      {/* Add File Prompt Dialog */}
      {showAddDialog && promptType === 'File' && (
        <KeyboardDialog
          title="Add New File Prompt"
          placeholder="Enter prompt name..."
          initialValue=""
          onConfirm={handleAddFilePrompt}
          onCancel={() => setShowAddDialog(false)}
        />
      )}

      {/* Edit TTS Text Dialog */}
      {showKeyboardDialog && keyboardEditingPrompt && (
        <KeyboardDialog
          title={`Edit Text: ${keyboardEditingPrompt.name}`}
          placeholder="Enter text..."
          initialValue={keyboardEditingPrompt.data}
          onConfirm={handleKeyboardConfirm}
          onCancel={() => {
            setShowKeyboardDialog(false);
            setKeyboardEditingPrompt(null);
          }}
        />
      )}
    </div>
  );
}