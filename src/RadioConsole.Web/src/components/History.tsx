import React from 'react';
import { Box, Card, CardContent, Typography, List, ListItem, ListItemText } from '@mui/material';

export const History: React.FC = () => {
  // Placeholder - will be implemented with actual history data
  const historyItems = [
    { id: '1', title: 'Sample Radio Station', timestamp: new Date().toISOString() },
  ];

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        History
      </Typography>
      <Card>
        <CardContent>
          {historyItems.length === 0 ? (
            <Typography color="text.secondary">No history items yet</Typography>
          ) : (
            <List>
              {historyItems.map((item) => (
                <ListItem key={item.id}>
                  <ListItemText
                    primary={item.title}
                    secondary={new Date(item.timestamp).toLocaleString()}
                  />
                </ListItem>
              ))}
            </List>
          )}
        </CardContent>
      </Card>
    </Box>
  );
};
