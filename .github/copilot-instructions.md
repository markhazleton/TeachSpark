# TeachSpark Copilot Instructions

This is a .NET 10.0 MVC Web Application with npm build process for client-side assets.

## Core Styling Rule - CRITICAL

**NEVER create custom CSS or inline styles. ALWAYS use native Bootstrap 5 classes exclusively.**

## Technology Guidelines

- Use .NET 10.0 MVC patterns with dependency injection
- Frontend assets managed via webpack and npm
- JavaScript can be vanilla JS, jQuery is used for DataTables and Bootstrap 5 components
- SCSS imports Bootstrap 5 only, no custom styles
- Forms use Bootstrap 5 validation classes and patterns

## Code Standards

- Follow async/await patterns for all async operations
- Use data annotations for model validation
- Razor views must use Bootstrap 5 markup exclusively
- JavaScript must integrate with Bootstrap 5 components and validation
- File organization follows MVC conventions with src/ for pre-build assets

## Build Process

- Use npm scripts: `npm run build` (production), `npm run build:dev` (development), `npm run watch` (development with watching)
- Webpack handles asset compilation and bundling
- Generated assets go to wwwroot/ directory

## UI Components Requirements

When generating HTML/Razor views:

- Use Bootstrap 5 grid system (container, row, col-\*)
- Use Bootstrap 5 components (card, btn, form-control, nav, etc.)
- Implement responsive design with Bootstrap utility classes
- Include accessibility attributes
- No custom CSS classes or inline styles allowed

## Documentation
- Use XML comments for public methods and classes
 - Any markdown documentation should be in the Docs folder
