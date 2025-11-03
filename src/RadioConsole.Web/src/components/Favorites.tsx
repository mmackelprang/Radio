import React from 'react';
import { Box, Card, CardContent, Typography, List, ListItem, ListItemText } from '@mui/material';

export const Favorites: React.FC = () => {
  // Placeholder - will be implemented with actual favorites data
  const favoriteItems = [
    { id: '1', name: 'My Favorite Station', source: 'Radio' },
  ];

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Favorites
      </Typography>
      <Card>
        <CardContent>
          {favoriteItems.length === 0 ? (
            <Typography color="text.secondary">No favorites yet</Typography>
          ) : (
            <List>
              {favoriteItems.map((item) => (
                <ListItem key={item.id}>
                  <ListItemText primary={item.name} secondary={item.source} />
                </ListItem>
              ))}
            </List>
          )}
        </CardContent>
      </Card>
    </Box>
  );
};
