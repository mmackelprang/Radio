import React, { useEffect, useState } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Slider,
  Button,
  Stack,
  CircularProgress,
  Alert,
} from '@mui/material';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';
import StopIcon from '@mui/icons-material/Stop';
import VolumeUpIcon from '@mui/icons-material/VolumeUp';
import { audioService, AudioInput, AudioOutput, AudioStatus } from '../services/audioService';

export const AudioControl: React.FC = () => {
  const [inputs, setInputs] = useState<AudioInput[]>([]);
  const [outputs, setOutputs] = useState<AudioOutput[]>([]);
  const [selectedInput, setSelectedInput] = useState('');
  const [selectedOutput, setSelectedOutput] = useState('');
  const [status, setStatus] = useState<AudioStatus | null>(null);
  const [volume, setVolume] = useState(50);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadData();
    const interval = setInterval(loadStatus, 2000); // Poll status every 2 seconds
    return () => clearInterval(interval);
  }, []);

  const loadData = async () => {
    try {
      const [inputsData, outputsData, statusData] = await Promise.all([
        audioService.getInputs(),
        audioService.getOutputs(),
        audioService.getStatus(),
      ]);
      setInputs(inputsData);
      setOutputs(outputsData);
      setStatus(statusData);
      setVolume(statusData.volume);
    } catch (err) {
      setError('Failed to load audio devices');
      console.error(err);
    }
  };

  const loadStatus = async () => {
    try {
      const statusData = await audioService.getStatus();
      setStatus(statusData);
    } catch (err) {
      console.error('Failed to load status', err);
    }
  };

  const handleStart = async () => {
    if (!selectedInput || !selectedOutput) {
      setError('Please select both input and output');
      return;
    }

    setLoading(true);
    setError(null);
    try {
      await audioService.start(selectedInput, selectedOutput);
      await loadStatus();
    } catch (err) {
      setError('Failed to start playback');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleStop = async () => {
    setLoading(true);
    setError(null);
    try {
      await audioService.stop();
      await loadStatus();
    } catch (err) {
      setError('Failed to stop playback');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleVolumeChange = async (_event: Event, newValue: number | number[]) => {
    const vol = newValue as number;
    setVolume(vol);
    try {
      await audioService.setVolume(vol);
    } catch (err) {
      console.error('Failed to set volume', err);
    }
  };

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Audio Control
      </Typography>

      {error && (
        <Alert severity="error" onClose={() => setError(null)} sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      <Stack spacing={3}>
        {/* Input Selection */}
        <Card>
          <CardContent>
            <FormControl fullWidth>
              <InputLabel>Audio Input</InputLabel>
              <Select
                value={selectedInput}
                label="Audio Input"
                onChange={(e) => setSelectedInput(e.target.value)}
                disabled={loading || status?.isPlaying}
              >
                {inputs.map((input) => (
                  <MenuItem
                    key={input.id}
                    value={input.id}
                    disabled={!input.isAvailable}
                  >
                    {input.name} {!input.isAvailable && '(Unavailable)'}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </CardContent>
        </Card>

        {/* Output Selection */}
        <Card>
          <CardContent>
            <FormControl fullWidth>
              <InputLabel>Audio Output</InputLabel>
              <Select
                value={selectedOutput}
                label="Audio Output"
                onChange={(e) => setSelectedOutput(e.target.value)}
                disabled={loading || status?.isPlaying}
              >
                {outputs.map((output) => (
                  <MenuItem
                    key={output.id}
                    value={output.id}
                    disabled={!output.isAvailable}
                  >
                    {output.name} {!output.isAvailable && '(Unavailable)'}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </CardContent>
        </Card>

        {/* Volume Control */}
        <Card>
          <CardContent>
            <Typography gutterBottom>Volume</Typography>
            <Stack direction="row" spacing={2} alignItems="center">
              <VolumeUpIcon />
              <Slider
                value={volume}
                onChange={handleVolumeChange}
                min={0}
                max={100}
                valueLabelDisplay="auto"
                disabled={loading}
              />
              <Typography>{volume}%</Typography>
            </Stack>
          </CardContent>
        </Card>

        {/* Playback Controls */}
        <Card>
          <CardContent>
            <Stack direction="row" spacing={2}>
              <Button
                variant="contained"
                startIcon={loading ? <CircularProgress size={20} /> : <PlayArrowIcon />}
                onClick={handleStart}
                disabled={loading || status?.isPlaying || !selectedInput || !selectedOutput}
                fullWidth
              >
                Start
              </Button>
              <Button
                variant="outlined"
                startIcon={loading ? <CircularProgress size={20} /> : <StopIcon />}
                onClick={handleStop}
                disabled={loading || !status?.isPlaying}
                fullWidth
              >
                Stop
              </Button>
            </Stack>
          </CardContent>
        </Card>

        {/* Status Display */}
        {status && (
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Status
              </Typography>
              <Stack spacing={1}>
                <Typography>
                  <strong>Playing:</strong> {status.isPlaying ? 'Yes' : 'No'}
                </Typography>
                {status.currentInput && (
                  <>
                    <Typography>
                      <strong>Input:</strong> {status.currentInput.name}
                    </Typography>
                    {status.currentInput.metadata.title && (
                      <Typography>
                        <strong>Title:</strong> {status.currentInput.metadata.title}
                      </Typography>
                    )}
                    {status.currentInput.metadata.artist && (
                      <Typography>
                        <strong>Artist:</strong> {status.currentInput.metadata.artist}
                      </Typography>
                    )}
                    <Typography>
                      <strong>Status:</strong> {status.currentInput.status}
                    </Typography>
                  </>
                )}
                {status.currentOutput && (
                  <Typography>
                    <strong>Output:</strong> {status.currentOutput.name}
                  </Typography>
                )}
              </Stack>
            </CardContent>
          </Card>
        )}
      </Stack>
    </Box>
  );
};
