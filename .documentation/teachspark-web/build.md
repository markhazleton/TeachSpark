# TeachSpark.Web Build System

This document explains the integrated npm + webpack build system for the TeachSpark.Web project.

## Overview

The project uses webpack to compile SCSS and JavaScript source files into optimized bundles that include Bootstrap 5 and Bootstrap Icons. The build process is integrated with the .NET build system, so running `dotnet build` automatically compiles frontend assets.

## Source Structure

```
src/
├── scss/
│   └── site.scss          # Main SCSS file (imports Bootstrap + custom styles)
└── js/
    ├── admin.js           # Admin-area behaviors
    ├── site.js            # Main application JavaScript
    └── validation.js      # jQuery validation bootstrap
```

## Output Structure

```
wwwroot/
├── css/
│   ├── site.css           # Compiled CSS (includes Bootstrap + custom styles)
│   └── site.css.map       # Source map (development builds only)
├── js/
│   ├── site.js            # Main compiled application JavaScript
│   └── validation.js      # Compiled validation JavaScript
└── fonts/
    ├── bootstrap-icons.woff
    └── bootstrap-icons.woff2
```

## Available NPM Scripts

- `npm run build` - Production build (minified, no source maps)
- `npm run build:dev` - Development build (source maps included)
- `npm run build:analyze` - Production build with bundle analysis
- `npm run watch` - Development build with file watching
- `npm run dev` - Start webpack dev server with hot reload
- `npm run clean` - Remove all generated files

## Integration with .NET Build

The build system is integrated with MSBuild through the following targets:

### NpmInstall Target

- Runs before `BuildAssets` if `node_modules` doesn't exist
- Ensures npm packages are installed before building

### BuildAssets Target

- Runs before .NET `Build` target
- Executes `npm run build` for Release builds
- Executes `npm run build:dev` for Debug builds

### CleanAssets Target

- Runs after .NET `Clean` target
- Removes generated frontend assets

### Up-to-Date Check

- MSBuild tracks changes to source files (`src/**/*.scss`, `src/**/*.js`, `package.json`, `webpack.config.js`)
- Skips asset compilation if output files are newer than input files

## Build Commands

### Development

```bash
# Build in development mode (includes source maps)
dotnet build

# Watch for changes and rebuild automatically
npm run watch
```

### Production

```bash
# Build optimized assets for production
dotnet build --configuration Release

# Or directly with npm
npm run build
```

### Clean

```bash
# Clean all generated files
dotnet clean

# Or just frontend assets
npm run clean
```

## Configuration Files

- `webpack.config.js` - Webpack configuration
- `package.json` - NPM dependencies and scripts
- `.babelrc` - Babel configuration for JavaScript transpilation
- `TeachSpark.Web.csproj` - MSBuild integration targets

## Included Libraries

### CSS/SCSS

- Bootstrap 5 (complete framework)
- Bootstrap Icons (icon font)
- Custom TeachSpark styles

### JavaScript

- Bootstrap 5 JavaScript components
- Custom TeachSpark application logic

## Development Workflow

1. **Initial Setup**: Run `dotnet build` to install npm packages and build assets
2. **Development**: Use `npm run watch` for automatic rebuilding during development
3. **Testing**: The application automatically includes the compiled assets
4. **Production**: Use `dotnet build --configuration Release` for optimized builds

## Customization

### Adding SCSS

- Add new `.scss` files to `src/scss/`
- Import them in `src/scss/site.scss`

### Adding JavaScript

- Add new `.js` files to `src/js/`
- Register them in the webpack entry configuration when they need their own output bundle

### Adding NPM Packages

- Install with `npm install package-name`
- Import in your source files as needed

### Webpack Configuration

- Modify `webpack.config.js` to customize the build process
- Add new loaders, plugins, or entry points as needed

## Performance Notes

- Production builds are minified and optimized
- Development builds include source maps for debugging
- Font files are automatically copied and optimized
- The build system uses webpack's built-in caching for faster rebuilds

## Troubleshooting

### Build Issues

- Ensure Node.js and npm are installed
- Run `npm install` to restore packages
- Check for syntax errors in source files

### Missing Assets

- Run `dotnet clean` followed by `dotnet build`
- Verify webpack configuration is correct
- Check that source files exist in the expected locations
