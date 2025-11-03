import { createTheme, ThemeOptions } from '@mui/material/styles';

// Material Design 3 Color Palette
const lightPalette = {
  primary: {
    main: '#6750A4',
    light: '#988CC0',
    dark: '#4E3B82',
    contrastText: '#FFFFFF',
  },
  secondary: {
    main: '#625B71',
    light: '#7F77A0',
    dark: '#4A4454',
    contrastText: '#FFFFFF',
  },
  error: {
    main: '#BA1A1A',
    light: '#DE3730',
    dark: '#93000A',
    contrastText: '#FFFFFF',
  },
  warning: {
    main: '#FF8C00',
    light: '#FFA726',
    dark: '#E65100',
    contrastText: '#000000',
  },
  info: {
    main: '#0091EA',
    light: '#33A4F4',
    dark: '#006DB3',
    contrastText: '#FFFFFF',
  },
  success: {
    main: '#198754',
    light: '#47A06B',
    dark: '#106538',
    contrastText: '#FFFFFF',
  },
  background: {
    default: '#FFFBFE',
    paper: '#FFFBFE',
  },
  text: {
    primary: '#1C1B1F',
    secondary: '#49454F',
  },
};

const darkPalette = {
  primary: {
    main: '#D0BCFF',
    light: '#E0D5FF',
    dark: '#A997E5',
    contrastText: '#381E72',
  },
  secondary: {
    main: '#CCC2DC',
    light: '#E8DEF8',
    dark: '#9A90B3',
    contrastText: '#332D41',
  },
  error: {
    main: '#F2B8B5',
    light: '#FFDAD6',
    dark: '#C5948E',
    contrastText: '#601410',
  },
  warning: {
    main: '#FFB74D',
    light: '#FFD54F',
    dark: '#F57C00',
    contrastText: '#000000',
  },
  info: {
    main: '#40C4FF',
    light: '#80D8FF',
    dark: '#0091EA',
    contrastText: '#000000',
  },
  success: {
    main: '#66BB6A',
    light: '#81C784',
    dark: '#388E3C',
    contrastText: '#000000',
  },
  background: {
    default: '#1C1B1F',
    paper: '#1C1B1F',
  },
  text: {
    primary: '#E6E1E5',
    secondary: '#CAC4D0',
  },
};

export const getTheme = (mode: 'light' | 'dark') => {
  const palette = mode === 'light' ? lightPalette : darkPalette;

  const themeOptions: ThemeOptions = {
    palette: {
      mode,
      ...palette,
    },
    typography: {
      fontFamily: '"Roboto", "Helvetica", "Arial", sans-serif',
      h1: {
        fontSize: '2.5rem',
        fontWeight: 500,
      },
      h2: {
        fontSize: '2rem',
        fontWeight: 500,
      },
      h3: {
        fontSize: '1.75rem',
        fontWeight: 500,
      },
      h4: {
        fontSize: '1.5rem',
        fontWeight: 500,
      },
      h5: {
        fontSize: '1.25rem',
        fontWeight: 500,
      },
      h6: {
        fontSize: '1rem',
        fontWeight: 500,
      },
      button: {
        textTransform: 'none',
      },
    },
    shape: {
      borderRadius: 12,
    },
    components: {
      MuiButton: {
        styleOverrides: {
          root: {
            borderRadius: 20,
            padding: '10px 24px',
          },
        },
      },
      MuiCard: {
        styleOverrides: {
          root: {
            borderRadius: 12,
          },
        },
      },
      MuiAppBar: {
        styleOverrides: {
          root: {
            backgroundColor: mode === 'light' ? palette.background.default : palette.background.paper,
            color: palette.text.primary,
          },
        },
      },
    },
  };

  return createTheme(themeOptions);
};
