# Asset Loading Fix Summary

## Problem

When publishing to a zip file and deploying to Windows IIS, the `assets-manifest.json` was not working correctly, resulting in 404 errors on wwwroot assets.

## Root Causes Identified

1. **Inconsistent manifest format**: Some asset entries had leading slashes while others didn't
2. **Missing IIS configuration**: No web.config file for proper static file handling
3. **Asset service limitations**: Path normalization couldn't handle all manifest variations
4. **Deployment-specific issues**: IIS requires specific MIME types and static file configuration

## Key Fixes Applied

### 1. Enhanced web.config ✅

- Added comprehensive static file MIME type mappings
- Configured proper URL compression and caching
- Added security headers for production deployment

### 2. Improved Asset Service ✅

- Enhanced `GetAssetPath()` method with multiple lookup strategies
- Added fallback logic for common asset patterns
- Improved error logging and diagnostics

### 3. Fixed Webpack Manifest Generation ✅

- Modified `webpack.config.prod.js` to generate consistent asset entries
- Each asset now has multiple entries for maximum compatibility:
  - `/js/site.js` → `/js/site.abc123.js`
  - `js/site.js` → `/js/site.abc123.js`
  - `site.js` → `/js/site.abc123.js`

### 4. Added Diagnostic Tools ✅

- Created `DiagnosticsController` for troubleshooting asset issues
- Provides detailed asset resolution information via `/Diagnostics/Assets`

## Testing the Fix

### Build and Deploy Steps

```powershell
# 1. Clean and rebuild assets
npm run clean
npm run build

# 2. Build release version
dotnet build -c Release
dotnet publish -c Release -o ./publish

# 3. Create deployment package
Compress-Archive -Path ./publish/* -DestinationPath deployment.zip

# 4. Deploy to IIS
# Extract deployment.zip to your IIS application directory
```

### Verification Steps

1. **Check the diagnostic endpoint**: Visit `/Diagnostics/Assets` to verify asset resolution
2. **Inspect network tab**: Use browser dev tools to ensure no 404 errors
3. **Verify manifest format**: Check that `wwwroot/assets-manifest.json` has multiple entries per asset

## Expected Manifest Format

The manifest should now look like this:

```json
{
  "/js/site.js": "/js/site.325e445e.js",
  "js/site.js": "/js/site.325e445e.js",
  "site.js": "/js/site.325e445e.js",
  "/css/site.css": "/css/site.0dee3456.css",
  "css/site.css": "/css/site.0dee3456.css",
  "site.css": "/css/site.0dee3456.css"
}
```

## Important Files Modified

- `web.config` - **CRITICAL for IIS deployment**
- `Services/AssetsService.cs` - Enhanced asset resolution
- `webpack.config.prod.js` - Fixed manifest generation
- `Controllers/DiagnosticsController.cs` - Troubleshooting endpoint

## Troubleshooting

If issues persist after deployment:

1. **Access diagnostic endpoint**: `https://yoursite.com/Diagnostics/Assets`
2. **Check IIS logs**: Look in `C:\inetpub\logs\LogFiles\W3SVC1\`
3. **Verify file permissions**: Ensure IIS can read the wwwroot directory
4. **Check manifest content**: Ensure the assets-manifest.json file is present

## Next Steps

1. Deploy the updated application to your IIS server
2. Test asset loading thoroughly
3. Monitor for any remaining 404 errors
4. Use the diagnostic endpoint if issues arise

The fixes provide comprehensive asset resolution capabilities that should work across different deployment scenarios and IIS configurations.
