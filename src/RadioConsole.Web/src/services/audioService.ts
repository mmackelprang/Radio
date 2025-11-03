import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

export interface AudioInput {
  id: string;
  name: string;
  description: string;
  isAvailable: boolean;
  isActive: boolean;
}

export interface AudioOutput {
  id: string;
  name: string;
  description: string;
  isAvailable: boolean;
  isActive: boolean;
}

export interface AudioMetadata {
  title?: string;
  artist?: string;
  album?: string;
  station?: string;
  genre?: string;
  duration?: string;
  position?: string;
  albumArtUrl?: string;
}

export interface AudioStatus {
  isPlaying: boolean;
  volume: number;
  currentInput?: {
    id: string;
    name: string;
    metadata: AudioMetadata;
    status: string;
  };
  currentOutput?: {
    id: string;
    name: string;
  };
}

export const audioService = {
  async getInputs(): Promise<AudioInput[]> {
    const response = await apiClient.get<AudioInput[]>('/audio/inputs');
    return response.data;
  },

  async getOutputs(): Promise<AudioOutput[]> {
    const response = await apiClient.get<AudioOutput[]>('/audio/outputs');
    return response.data;
  },

  async start(inputId: string, outputId: string): Promise<void> {
    await apiClient.post('/audio/start', { inputId, outputId });
  },

  async stop(): Promise<void> {
    await apiClient.post('/audio/stop');
  },

  async getStatus(): Promise<AudioStatus> {
    const response = await apiClient.get<AudioStatus>('/audio/status');
    return response.data;
  },

  async setVolume(volume: number): Promise<void> {
    await apiClient.put('/audio/volume', { volume });
  },
};
