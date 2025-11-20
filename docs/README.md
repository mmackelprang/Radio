# Radio Console Documentation Index

This directory contains project documentation organized by category.

## 📖 Current Documentation

### Architecture & Planning
- **[RadioPlan_v3.md](../RadioPlan_v3.md)** ⭐ - Master project blueprint (ACTIVE - in root)
- [RadioPlan_v2.md](RadioPlan_v2.md) - Previous version (archived)
- [RadioPlan.md](RadioPlan.md) - Original version (archived)

### Feature Documentation
- **[AUDIO_PRIORITY_SERVICE.md](AUDIO_PRIORITY_SERVICE.md)** - Audio priority/ducking developer guide
- [CONFIGURATION_SERVICE.md](CONFIGURATION_SERVICE.md) - Configuration management guide
- [UI_VISUALIZATION_GUIDE.md](UI_VISUALIZATION_GUIDE.md) - Audio visualization implementation
- [VISUALIZATION_README.md](VISUALIZATION_README.md) - Visualization system overview

### Project Status (Historical)
- [PHASE5_COMPLETION.md](PHASE5_COMPLETION.md) - Phase 5 completion summary
- [PHASE4_COMPLETION.md](PHASE4_COMPLETION.md) - Phase 4 completion summary
- [PHASE4_SUMMARY.md](PHASE4_SUMMARY.md) - Phase 4 summary
- [PHASE3_COMPLETION.md](PHASE3_COMPLETION.md) - Phase 3 completion summary
- [PHASE3_SUMMARY.md](PHASE3_SUMMARY.md) - Phase 3 summary
- [UPDATES_COMPLETED.md](UPDATES_COMPLETED.md) - Recent updates summary
- [IMPLEMENTATION_SUMMARY.md](IMPLEMENTATION_SUMMARY.md) - Implementation overview

### Development
- [CODE_REVIEW.md](CODE_REVIEW.md) - Code review guidelines and checklists
- [PromptPlan.md](PromptPlan.md) - AI coding assistant prompt guidelines

## 🗂️ Documentation Organization

### Active Documents (Root Directory)
Keep these in the root for easy access during active development:
- `README.md` - Main project documentation
- `RadioPlan_v3.md` - Current project specification
- `PHASE5_INTEGRATION_TODO.md` - Active integration tasks
- `LICENSE` - Project license

### Feature-Specific Documentation
Feature documentation should ideally be located near the feature code:
- Audio Priority Service: `docs/AUDIO_PRIORITY_SERVICE.md`
- Configuration Service: `docs/CONFIGURATION_SERVICE.md`
- Visualization: `docs/UI_VISUALIZATION_GUIDE.md`

## 📝 Documentation Standards

### When to Create New Documentation
1. **Feature Documentation**: When implementing a new major feature or service
2. **API Documentation**: For new REST API controllers (in addition to Swagger)
3. **Architecture Decisions**: When making significant architectural changes
4. **Integration Guides**: When integrating with external systems

### Where to Place Documentation
- **Root**: Active TODO lists, main README, current plan version
- **docs/**: Completed status documents, historical records, general guides
- **Feature folders**: Feature-specific technical documentation (future improvement)
- **Code**: XML comments for API documentation

### Documentation Format
- Use Markdown (.md) for all documentation
- Include table of contents for documents > 100 lines
- Use code blocks with syntax highlighting
- Include examples and usage patterns
- Keep documentation up-to-date with code changes

## 🔄 Documentation Lifecycle

1. **Planning** → Create in root as TODO/plan document
2. **Development** → Update with implementation details
3. **Completion** → Move summary to docs/, link from README
4. **Maintenance** → Keep feature docs updated with code

## 📚 Additional Resources

### External Documentation
- [SoundFlow Documentation](https://lsxprime.github.io/soundflow-docs/)
- [MudBlazor Components](https://mudblazor.com/)
- [TagLibSharp Wiki](https://github.com/mono/taglib-sharp/wiki)

### API Documentation
- Swagger UI: `http://localhost:5211/swagger` (when API is running)
- SignalR Hubs: See `RadioConsole.API/Hubs/`

## 🔍 Finding Documentation

### By Topic
- **Audio Management**: AUDIO_PRIORITY_SERVICE.md, VISUALIZATION_README.md
- **Configuration**: CONFIGURATION_SERVICE.md
- **UI/UX**: UI_VISUALIZATION_GUIDE.md
- **Architecture**: RadioPlan_v3.md
- **Testing**: CODE_REVIEW.md

### By Phase
- **Phase 1-2**: IMPLEMENTATION_SUMMARY.md
- **Phase 3**: PHASE3_COMPLETION.md, PHASE3_SUMMARY.md
- **Phase 4**: PHASE4_COMPLETION.md, PHASE4_SUMMARY.md
- **Phase 5**: PHASE5_COMPLETION.md

## 📞 Questions?

If you can't find the documentation you need:
1. Check the main [README.md](../README.md)
2. Search the codebase XML comments
3. Review relevant phase completion documents
4. Check the active TODO list: [PHASE5_INTEGRATION_TODO.md](../PHASE5_INTEGRATION_TODO.md)
