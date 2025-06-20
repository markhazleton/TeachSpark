# TeachSpark Admin Area

This document describes the comprehensive Admin area created for managing all non-identity tables in the TeachSpark application.

## Overview

The Admin area provides a full CRUD (Create, Read, Update, Delete) interface for managing all database entities except Identity tables. It features:

- **Modern Bootstrap 5 UI**: Clean, responsive design with professional styling
- **DataTables.js Integration**: Advanced table features with sorting, filtering, pagination, and export capabilities
- **Role-Based Access**: Admin functions are only accessible to authenticated users
- **World-Class Forms**: Comprehensive validation, user-friendly inputs, and excellent UX
- **Real-time Dashboard**: Statistics and monitoring capabilities

## Structure

### Controllers

All admin controllers are located in `Areas/Admin/Controllers/` and inherit from `BaseAdminController`:

1. **DashboardController**: Main admin dashboard with statistics and overview
2. **BloomLevelsController**: Manage Bloom's Taxonomy levels
3. **CommonCoreStandardsController**: Manage educational standards
4. **WorksheetTemplatesController**: Manage worksheet templates
5. **WorksheetsController**: Manage user-created worksheets
6. **ApiKeysController**: Manage API keys and access
7. **ApiUsageController**: View and analyze API usage statistics
8. **WorksheetExportsController**: Manage exported worksheet files

### Views

Each controller has corresponding views in `Areas/Admin/Views/[Controller]/`:

- **Index**: DataTables-powered listing with search, sort, filter, export
- **Create**: Comprehensive forms with validation and help text
- **Edit**: Update existing records with all safety checks
- **Delete**: Confirmation pages with usage dependency checks
- **Details**: Full record view with related data and actions

### Features by Entity

#### Bloom Levels ✅ COMPLETE

- Manage the 6 levels of Bloom's Taxonomy
- Color-coded visualization
- Action verb management
- Dependency checking before deletion
- Full CRUD operations with validation

#### Common Core Standards ✅ COMPLETE

- Complete standard management (Code, Grade, Subject, Domain, etc.)
- Active/inactive status tracking
- Grade and subject organization
- Full CRUD operations with validation
- Related worksheet impact warnings

#### Worksheet Templates ✅ COMPLETE

- Template creation and management
- JSON layout definition
- Public/private visibility controls
- System vs user-created templates
- Usage statistics tracking
- Full CRUD operations with validation

#### Worksheets ✅ PARTIAL (Index + Create views)

- Comprehensive worksheet management
- Standards and Bloom level integration
- Template-based creation
- Content JSON definition
- Public sharing and favorites
- Generation metadata tracking
- **Status**: Index and Create views completed, Edit/Details/Delete pending

#### API Keys ✅ PARTIAL (Index + Create views)

- Secure API key generation
- Rate limiting and scope controls
- IP address restrictions
- Expiration date management
- Usage tracking and analytics
- **Status**: Index and Create views completed, Edit/Details/Delete pending

#### API Usage ✅ PARTIAL (Index view only)

- Real-time usage monitoring
- Performance analytics
- Error tracking and reporting
- Endpoint usage statistics
- IP address and user agent logging
- **Status**: Index view with analytics completed, Details view pending

#### Worksheet Exports ✅ PARTIAL (Index view only)

- Export file management
- Format support (PDF, DOCX, HTML)
- File size and download tracking
- Automatic cleanup of expired files
- Storage management
- **Status**: Index view with analytics completed, Details/Delete views pending
- Category organization
- Example activities

#### Worksheet Templates

- Template creation and management
- JSON layout validation
- Public/private and system/user distinctions
- Usage tracking

#### Worksheets

- Complete worksheet management
- User association
- Template and standard linking
- Public/private status
- View and download tracking

#### API Keys

- Secure API key generation
- Usage limits and monitoring
- IP address restrictions
- Scope management

#### API Usage

- Real-time usage analytics
- Performance monitoring
- Error rate tracking
- Cleanup utilities

#### Worksheet Exports

- Export file management
- Format tracking (PDF, DOCX, HTML)
- Cleanup of expired/temporary files
- Download statistics

## Navigation

### Main Site Integration

When logged in, users see an "Admin" dropdown in the main navigation with links to:

- Dashboard
- Content Management (Bloom Levels, Standards, Templates)
- Data Management (Worksheets, API Keys)

### Admin Layout

The admin area uses a dedicated layout (`_AdminLayout.cshtml`) featuring:

- Dark theme navigation
- Comprehensive admin menu
- Alert message system
- Real-time status indicators

## DataTables Configuration

All index pages use DataTables with:

- **Responsive Design**: Works on all screen sizes
- **Export Functionality**: Excel, PDF, Print options
- **Advanced Search**: Real-time filtering
- **Custom Rendering**: Status badges, formatted data, action buttons
- **AJAX Data Loading**: Fast, server-side data processing

## Security Features

- **Authentication Required**: All admin functions require login
- **CSRF Protection**: All forms include anti-forgery tokens
- **Dependency Checking**: Prevents deletion of records in use
- **Input Validation**: Comprehensive server and client-side validation
- **JSON Validation**: Ensures valid JSON in layout and configuration fields

## Technical Implementation

### Base Controller

`BaseAdminController` provides:

- Common alert message functionality
- JSON validation helpers
- Consistent error handling

### Route Configuration

Admin routes are configured with area support:

```csharp
app.MapControllerRoute(
    name: "admin",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");
```

### Asset Management

- Bootstrap 5 for styling
- Bootstrap Icons for consistent iconography
- DataTables with Bootstrap 5 theme
- Custom admin JavaScript for enhanced UX

## Getting Started

1. **Access**: Log in to the main site, then use the "Admin" dropdown
2. **Dashboard**: Start with the dashboard for an overview
3. **Management**: Use the navigation to access specific entity management
4. **Data Export**: Use the export buttons on any table for reporting

## Best Practices

- Always check for dependencies before deleting records
- Use the bulk export features for data analysis
- Monitor API usage regularly
- Keep templates and standards organized with proper categorization
- Use the dashboard for quick system health checks

## Customization

The admin area is designed to be easily extensible:

- Add new controllers by inheriting from `BaseAdminController`
- Follow the established patterns for views and DataTables
- Use the common styling classes and components
- Implement proper validation and error handling

## Troubleshooting

Common issues and solutions:

- **DataTables not loading**: Check JavaScript console for errors
- **Form validation issues**: Ensure all required fields have validation attributes
- **Permission denied**: Verify user is authenticated
- **Export not working**: Check DataTables buttons configuration

This admin area provides a comprehensive, professional-grade management interface for the TeachSpark application, following modern web development best practices and providing an excellent user experience.
