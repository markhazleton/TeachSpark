# IIS Deployment Asset Loading Fix Guide

This document explains the fixes applied to resolve 404 errors on wwwroot assets when publishing to a zip file and deploying to Windows IIS.

## Issues Identified

1. **Inconsistent path format in manifest**: Some entries had leading slashes (`/css/site.css`) while others didn't (`vendor.js`)
2. **Missing web.config configuration** for static files in IIS
3. **Asset service path normalization** wasn't handling all manifest variations properly
4. **Case sensitivity issues** possible on Windows IIS

## Fixes Applied

### 1. Web.config Configuration (CRITICAL for IIS)

Created a comprehensive `web.config` file with:

- **Static file MIME type mappings** for .js, .css, .woff, .woff2, .json files
- **URL compression** enabled for better performance
- **Cache control** for static assets (30 days)
- **Security headers** for production deployment

### 2. Enhanced Asset Service (`Services/AssetsService.cs`)

Improved the `GetAssetPath` method to:

- **Try multiple path variations** when looking up assets in the manifest
- **Handle both leading slash and no-slash variations**
- **Add fallback logic** for common asset patterns
- **Enhanced logging** to help identify missing assets
- **Consistent path normalization**

### 3. Improved Webpack Manifest Generation (`webpack.config.prod.js`)

Enhanced the `WebpackManifestPlugin` configuration to:

- **Generate consistent logical names** with leading slashes
- **Add multiple entries** for backward compatibility (with and without leading slash)
- **Include actual filenames** for direct lookups
- **Better handling of all asset types** (JS, CSS, fonts)

### 4. Diagnostic Tools

Created diagnostic utilities to help troubleshoot asset loading issues:

- **DiagnosticsController** (`/Diagnostics/Assets`) - provides detailed asset resolution information
- **DiagnosticUtility** class for logging diagnostic information

## Deployment Steps

### Step 1: Build the Application

```powershell
# Clean and rebuild assets
npm run clean
npm run build

# Build the .NET application
dotnet build -c Release
dotnet publish -c Release -o ./publish
```

### Step 2: Verify Manifest Generation

Check that `wwwroot/assets-manifest.json` contains entries like:

```json
{
  "/js/site.js": "/js/site.abc123.js",
  "js/site.js": "/js/site.abc123.js",
  "/css/site.css": "/css/site.def456.css",
  "css/site.css": "/css/site.def456.css"
}
```

### Step 3: Create Deployment Package

```powershell
# Create zip package
Compress-Archive -Path ./publish/* -DestinationPath deployment.zip
```

### Step 4: Deploy to IIS

1. Extract the zip file to your IIS application directory
2. Ensure the `web.config` file is present in the root
3. Verify that the Application Pool is set to "No Managed Code" or ".NET Core"
4. Ensure the ASP.NET Core Hosting Bundle is installed on the server

## Troubleshooting

### If Assets Still Return 404

1. **Check the diagnostic endpoint**: Visit `/Diagnostics/Assets` to see detailed asset information
2. **Verify file permissions**: Ensure IIS has read access to the wwwroot directory
3. **Check manifest content**: Verify the assets-manifest.json file is present and correctly formatted
4. **Enable detailed logging**: Add `"EnableAssetDiagnostics": true` to appsettings.json

### Common Issues and Solutions

#### Issue: Manifest file not found

**Solution**: Ensure webpack build runs during publish. Check that `npm run build` completes successfully.

#### Issue: Assets found in manifest but return 404

**Solution**: Check file permissions and ensure the physical files exist in the wwwroot directory.

#### Issue: Wrong file paths in manifest

**Solution**: Verify webpack configuration and rebuild assets.

### IIS-Specific Considerations

1. **URL Rewrite**: If using URL Rewrite, ensure static files are excluded
2. **Static File Module**: Ensure the Static File module is enabled in IIS
3. **MIME Types**: The web.config includes necessary MIME types, but verify they're not overridden
4. **Case Sensitivity**: IIS on Windows is case-insensitive, but ensure consistent casing

## Testing the Fix

### Manual Testing

1. Deploy the application to IIS
2. Open the browser developer tools (F12)
3. Navigate to your application
4. Check the Network tab for any 404 errors on static assets
5. Visit `/Diagnostics/Assets` to see detailed diagnostic information

### Expected Results

- All CSS and JS files should load without 404 errors
- The assets-manifest.json should be accessible via HTTP
- Fonts and other static assets should load correctly
- Browser console should show no 404 errors for assets

## Additional Notes

- The diagnostic controller is only available in development or when explicitly enabled
- Enhanced logging will help identify any remaining asset resolution issues
- The webpack configuration now generates multiple manifest entries for maximum compatibility
- The web.config is essential for proper IIS deployment and should not be omitted

## If Issues Persist

1. Check IIS logs in `C:\inetpub\logs\LogFiles\W3SVC1\`
2. Enable detailed error messages in web.config temporarily
3. Use the diagnostic controller to identify specific asset resolution problems
4. Verify that all required files are present in the deployment package
5. Consider using Failed Request Tracing in IIS for detailed diagnostics
