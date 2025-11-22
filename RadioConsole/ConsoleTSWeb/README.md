# ConsoleTSWeb - Touchscreen Audio Controller UI

Modern React/TypeScript web interface for the Radio Console audio controller system, designed for ultra-wide touchscreen displays (12.5" x 3.75").

This is a code bundle for Touchscreen Audio Device UI. The original Figma design is available at https://www.figma.com/design/b2tRYRU8adJCeL0qN9jlZQ/Touchscreen-Audio-Device-UI.

## Quick Start

### Development
```bash
# Install dependencies
npm install

# Start dev server (with hot reload)
npm run dev
# UI available at http://localhost:5173
```

### Production Build
```bash
# Build for production
npm run build
# Output: dist/ directory
```

## Project Status

**Current State:** ✅ UI Complete, ⚠️ API Integration In Progress

- ✅ Full UI implementation with mock data
- ⚠️ Backend API partially complete (~25% of endpoints)
- ❌ Integration layer not yet implemented

## Documentation

### Start Here
📖 **[QUICK_START.md](./QUICK_START.md)** - Get up and running in 5 minutes

### Essential Reading
1. **[INTEGRATION_SUMMARY.md](./INTEGRATION_SUMMARY.md)** - Executive overview of the integration project
2. **[TYPESCRIPT_APP_OVERVIEW.md](./TYPESCRIPT_APP_OVERVIEW.md)** - Complete analysis of this UI application
3. **[API_ENDPOINT_RECONCILIATION.md](./API_ENDPOINT_RECONCILIATION.md)** - Mapping of required endpoints to available API
4. **[MIGRATION_GUIDE.md](./MIGRATION_GUIDE.md)** - Detailed phased implementation plan

### Technical Specs
- **[src/MISSING_ENDPOINTS.md](./src/MISSING_ENDPOINTS.md)** - API endpoint requirements with current status

## Technology Stack

- **React 18.3** - Modern React with hooks
- **TypeScript** - Type-safe JavaScript
- **Vite 6.3** - Fast build tool and dev server
- **Radix UI** - Accessible UI primitives
- **Tailwind CSS** - Utility-first CSS framework
- **Lucide React** - Icon library

## Features

### Audio Sources
- Spotify streaming
- USB Radio (Raddy RF320)
- Vinyl phonograph
- Local file player
- Bluetooth input
- AUX input

### Audio Outputs
- Built-in speakers
- Headphones
- Bluetooth output
- Line out
- Google Cast

### Key Functions
- Volume and balance control
- Playback controls (play/pause/next/previous)
- Input/output device selection
- Now playing display (source-specific)
- Playlist management
- System configuration
- Radio tuning and presets

## Project Structure

```
ConsoleTSWeb/
├── src/
│   ├── components/          # React components
│   │   ├── MainBar.tsx     # Top navigation
│   │   ├── AudioSetup.tsx  # Audio controls
│   │   ├── NowPlaying.tsx  # Main content area
│   │   ├── Playlist.tsx    # Playlist sidebar
│   │   ├── SystemConfig.tsx # Configuration UI
│   │   ├── now-playing/    # Source-specific players
│   │   ├── dialogs/        # Modal dialogs
│   │   └── ui/             # Reusable UI components
│   ├── api/                # API integration layer (to be created)
│   ├── styles/             # CSS styles
│   └── App.tsx             # Root component
├── docs/                   # Documentation (see above)
└── package.json            # Dependencies
```

## Integration with RadioConsole.API

This UI is designed to work with the RadioConsole.API backend (.NET 8).

**API Configuration:**
- Development: `http://localhost:5100` (with CORS enabled)
- Production: Same origin (UI served from API root)

**Integration Status:**
- ✅ 8 endpoints fully available
- 🟡 2 endpoints partially available
- ⚠️ 8 endpoints need modification
- ❌ 30+ endpoints missing

See [API_ENDPOINT_RECONCILIATION.md](./API_ENDPOINT_RECONCILIATION.md) for details.

## Development Roadmap

### Phase 1: Backend Enhancement (3-5 days)
Implement missing core API endpoints:
- Unified audio status/control
- Radio enhancements (tuning, scanning, presets)
- File player (browse, select, play)
- Playlist management

### Phase 2: Backend Enhancement (2-3 days)
Spotify and system features:
- Full Spotify integration
- System management
- Prompts management

### Phase 3: Frontend API Layer (3-4 days)
Create integration infrastructure:
- API client and error handling
- React hooks for API calls
- TypeScript types for API models

### Phase 4: Frontend Integration (4-5 days)
Wire up components:
- Replace mock data with API calls
- Add loading and error states
- Implement real-time updates

### Phase 5: Production Ready (2-3 days)
Polish and deploy:
- Comprehensive testing
- Error recovery
- Production build configuration

See [MIGRATION_GUIDE.md](./MIGRATION_GUIDE.md) for detailed implementation plan.

## Contributing

### Before You Start
1. Read [QUICK_START.md](./QUICK_START.md) for setup
2. Review [INTEGRATION_SUMMARY.md](./INTEGRATION_SUMMARY.md) for context
3. Check [MIGRATION_GUIDE.md](./MIGRATION_GUIDE.md) for your assigned phase

### Making Changes
1. Pick a task from MIGRATION_GUIDE.md
2. Use the provided coding agent prompt as specification
3. Test your changes thoroughly
4. Update [src/MISSING_ENDPOINTS.md](./src/MISSING_ENDPOINTS.md) status
5. Commit with clear message

### Code Standards
- Follow existing TypeScript patterns
- Use existing UI components from ui/
- Add proper TypeScript types
- Handle loading and error states
- Test on target display size (ultra-wide)

## Target Hardware

- **Primary:** Raspberry Pi 5 with 12.5" x 3.75" touchscreen
- **Development:** Any modern browser (Chrome/Edge recommended)
- **Requirements:** Touch-optimized interface, responsive layout

## License

See main repository LICENSE file.

## Support

- **Issues:** Check documentation first, then file GitHub issue
- **Questions:** Review INTEGRATION_SUMMARY.md and MIGRATION_GUIDE.md
- **API Docs:** http://localhost:5100/swagger (when API running)

---

**Last Updated:** November 22, 2024  
**Status:** Documentation Complete, Implementation In Progress
