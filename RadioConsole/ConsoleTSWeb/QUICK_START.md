# Quick Start Guide: ConsoleTSWeb Integration

This guide gets you started quickly with the ConsoleTSWeb to RadioConsole.API integration project.

## 5-Minute Overview

**What:** Integrate a modern React/TypeScript web UI with a .NET API backend for an audio controller system.

**Where:** 
- UI: `/RadioConsole/ConsoleTSWeb` (React + TypeScript)
- API: `/RadioConsole/RadioConsole.API` (.NET 8)

**Status:**
- ✅ UI is complete but uses mock data
- 🟡 API is ~25% complete (8/50 endpoints ready)
- ❌ Integration layer doesn't exist yet

**Goal:** Wire them together and implement missing API endpoints.

## Essential Reading (in order)

1. **Start here:** `INTEGRATION_SUMMARY.md` - 15 min read
2. **UI details:** `TYPESCRIPT_APP_OVERVIEW.md` - 20 min read  
3. **API mapping:** `API_ENDPOINT_RECONCILIATION.md` - 30 min read
4. **Work plan:** `MIGRATION_GUIDE.md` - 45 min read (skim for now)

**Total reading time:** ~2 hours (worth it!)

## Quick Setup

### Prerequisites
```bash
# Required
- .NET 8 SDK
- Node.js 18+
- Git

# Optional but recommended
- VS Code with C# and TypeScript extensions
- Postman or similar for API testing
```

### Get Running in 5 Minutes

```bash
# 1. Clone and navigate
cd /path/to/Radio

# 2. Start the API (Terminal 1)
cd RadioConsole/RadioConsole.API
dotnet run
# Wait for: "Now listening on: http://localhost:5100"
# Swagger UI: http://localhost:5100/swagger

# 3. Start the UI (Terminal 2)
cd RadioConsole/ConsoleTSWeb
npm install  # First time only
npm run dev
# Wait for: "Local: http://localhost:5173"

# 4. Open browser
# UI: http://localhost:5173
# API Docs: http://localhost:5100/swagger
```

**Expected behavior:**
- ✅ UI loads and displays
- ❌ Most functionality doesn't work (no API integration yet)
- ✅ API Swagger shows available endpoints

## What to Work On

### If you're a Backend Developer (.NET/C#)

**Start with:** Phase 1 of MIGRATION_GUIDE.md

**First task:** Implement UnifiedAudioController
```bash
# Location: RadioConsole/RadioConsole.API/Controllers/UnifiedAudioController.cs
# See: MIGRATION_GUIDE.md Phase 1.1 for detailed prompt
```

**Test your work:**
```bash
# API should be running
curl http://localhost:5100/api/audio/status
curl -X POST http://localhost:5100/api/audio/volume \
  -H "Content-Type: application/json" \
  -d '{"volume": 75}'
```

**Check progress:** Mark endpoints in `MISSING_ENDPOINTS.md` as complete

### If you're a Frontend Developer (React/TypeScript)

**Start with:** Phase 3 of MIGRATION_GUIDE.md (but Phase 1-2 must be done first!)

**First task:** Create API client infrastructure
```bash
# Location: RadioConsole/ConsoleTSWeb/src/api/
# See: MIGRATION_GUIDE.md Phase 3.1 for detailed prompt
```

**Test your work:**
```typescript
// Can you make API calls?
import { apiClient } from './api/client';
const status = await apiClient.get('/api/SystemStatus');
console.log(status);
```

**Check progress:** Update UI components to use API data instead of mock data

### If you're Full Stack

**Recommended order:**
1. Pick one feature (e.g., "Audio Control")
2. Implement backend endpoints (Phase 1)
3. Implement frontend API client (Phase 3)  
4. Wire up UI components (Phase 4)
5. Test end-to-end
6. Move to next feature

**Advantage:** You see complete features working quickly

## Common Tasks

### Test an API endpoint
```bash
# Using curl
curl http://localhost:5100/api/SystemStatus

# Using Swagger UI
# Visit http://localhost:5100/swagger
# Click endpoint > "Try it out" > "Execute"
```

### Add a new API endpoint
```csharp
// 1. Add method to controller (e.g., RaddyRadioController.cs)
[HttpPost("scan")]
public async Task<IActionResult> Scan([FromBody] ScanRequest request) {
    // Implementation
}

// 2. Test with curl or Swagger
// 3. Update MISSING_ENDPOINTS.md status
```

### Create API client hook (frontend)
```typescript
// src/api/hooks/useAudioStatus.ts
export function useAudioStatus() {
  const [status, setStatus] = useState<AudioStatus | null>(null);
  
  useEffect(() => {
    // Fetch from API
    fetch('http://localhost:5100/api/audio/status')
      .then(r => r.json())
      .then(setStatus);
  }, []);
  
  return { status };
}

// Use in component
const { status } = useAudioStatus();
```

### Wire up a component
```typescript
// Before (mock data)
const [volume, setVolume] = useState(50);

// After (API data)
const { status, setVolume } = useAudioStatus();
const volume = status?.volume ?? 50;
```

## Troubleshooting

### API won't start
```bash
# Check .NET version
dotnet --version  # Should be 8.0.x

# Check for port conflicts
# Kill process using port 5100 if needed
# On Linux/Mac: lsof -ti:5100 | xargs kill
# On Windows: netstat -ano | findstr :5100

# Rebuild
cd RadioConsole/RadioConsole.API
dotnet clean
dotnet build
```

### UI won't start
```bash
# Check Node version
node --version  # Should be 18+

# Clear node_modules and reinstall
cd RadioConsole/ConsoleTSWeb
rm -rf node_modules package-lock.json
npm install
npm run dev
```

### CORS errors in browser
```bash
# API needs CORS configuration
# Add to RadioConsole.API/Program.cs:

builder.Services.AddCors(options => {
  options.AddPolicy("Dev", builder => {
    builder.WithOrigins("http://localhost:5173")
           .AllowAnyMethod()
           .AllowAnyHeader();
  });
});

// After services configuration:
app.UseCors("Dev");
```

### API endpoint not found (404)
```bash
# Check controller route
[Route("api/[controller]")]  # Uses controller name
public class AudioController  # Results in /api/Audio

# Check method route
[HttpGet("status")]  # Combines to /api/Audio/status

# Verify in Swagger UI
```

### TypeScript errors in UI
```bash
# Rebuild TypeScript
npm run build

# Check for type errors
npx tsc --noEmit
```

## Quick Reference

### API Ports
- Main API: `http://localhost:5100`
- Swagger UI: `http://localhost:5100/swagger`

### UI Port
- Development: `http://localhost:5173`

### Key Files

**Backend:**
- Controllers: `RadioConsole/RadioConsole.API/Controllers/`
- Models: `RadioConsole/RadioConsole.Core/Models/`
- Services: `RadioConsole/RadioConsole.Infrastructure/`

**Frontend:**
- Components: `RadioConsole/ConsoleTSWeb/src/components/`
- API Layer: `RadioConsole/ConsoleTSWeb/src/api/` (to be created)
- Main App: `RadioConsole/ConsoleTSWeb/src/App.tsx`

### Documentation
- All docs in: `RadioConsole/ConsoleTSWeb/`
- Start with: `INTEGRATION_SUMMARY.md`

## Getting Help

### Documentation Order
1. Having trouble understanding the UI? → Read `TYPESCRIPT_APP_OVERVIEW.md`
2. Not sure if endpoint exists? → Check `API_ENDPOINT_RECONCILIATION.md`
3. Don't know what to build next? → See `MIGRATION_GUIDE.md`
4. Need the big picture? → Review `INTEGRATION_SUMMARY.md`

### Code Examples
All coding agent prompts in `MIGRATION_GUIDE.md` include:
- Clear requirements
- Code examples
- Implementation guidance
- Testing instructions

### Testing Your Changes
```bash
# Backend
cd RadioConsole/RadioConsole.API
dotnet test

# Frontend (when tests exist)
cd RadioConsole/ConsoleTSWeb
npm test
```

## Next Steps After Setup

1. ✅ Got API and UI running? → Good!
2. 📖 Read INTEGRATION_SUMMARY.md → Understand the project
3. 🗺️ Review MIGRATION_GUIDE.md Phase 1 → See what to build
4. 💻 Pick a task from the guide → Start coding
5. ✅ Test your changes → Verify it works
6. 📝 Update MISSING_ENDPOINTS.md → Track progress
7. 🔁 Repeat steps 4-6 → Keep building

## Success Indicators

You're on the right track when:
- ✅ API responds to requests in Swagger UI
- ✅ UI makes API calls (check Network tab in DevTools)
- ✅ UI updates when API data changes
- ✅ No CORS errors in browser console
- ✅ Loading states appear briefly during API calls
- ✅ Error messages display when API is down

## Time Expectations

**To get productive:**
- Setup environment: 30 minutes
- Read documentation: 2-3 hours
- First meaningful contribution: 4-6 hours

**To complete core integration:**
- Backend work (Phases 1-2): 5-8 days
- Frontend work (Phases 3-4): 7-9 days
- Testing & polish (Phase 5): 2-3 days
- **Total:** ~3-4 weeks for core functionality

**To add all optional features:**
- Phase 6 work: 1-2 weeks additional

## Remember

- 📖 Read the docs (they're comprehensive for a reason)
- 🧪 Test frequently (catch issues early)
- 💬 Ask questions (check docs first)
- ✅ Update progress (mark completed endpoints)
- 🎯 Stay focused (don't add scope)

Good luck! 🚀
