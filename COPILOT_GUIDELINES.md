# GitHub Copilot Guidelines for TeachSpark

> **Note**: This document serves as comprehensive team guidelines. For repository-specific instructions that Copilot automatically uses, see [`.github/copilot-instructions.md`](.github/copilot-instructions.md).

## Project Overview

TeachSpark is a .NET 9.0 MVC Web Application with an integrated npm build process for managing client-side assets. This project follows a modern web development workflow with strict styling guidelines and best practices.

## Technology Stack

- **Backend**: .NET 9.0 MVC (C#)
- **Frontend Build**: Webpack + npm
- **Styling**: Bootstrap 5 (native styles only)
- **JavaScript**: Vanilla JavaScript with validation support
- **CSS Processing**: SCSS with webpack compilation

## Core Development Principles

### 🎨 Styling Guidelines - CRITICAL

**NEVER create custom CSS or inline styles. ALWAYS use native Bootstrap 5 classes.**

#### ✅ Correct Styling Approach

```html
<!-- Use Bootstrap 5 classes exclusively -->
<div class="container-fluid">
    <div class="row justify-content-center">
        <div class="col-md-6">
            <div class="card shadow-sm">
                <div class="card-header bg-primary text-white">
                    <h5 class="card-title mb-0">Title</h5>
                </div>
                <div class="card-body">
                    <p class="text-muted">Content here</p>
                    <button class="btn btn-success btn-lg">Action</button>
                </div>
            </div>
        </div>
    </div>
</div>
```

#### ❌ Incorrect Styling Approach

```html
<!-- NEVER do this -->
<div style="margin: 20px; color: blue;">Content</div>
<div class="my-custom-class">Content</div>

<!-- NEVER create custom CSS -->
<style>
.custom-button { background: #007bff; }
</style>
```

### 📁 Project Structure

```
TeachSpark.Web/
├── Controllers/          # MVC Controllers
├── Models/              # View Models and Data Models
├── Views/               # Razor Views (.cshtml)
│   ├── Shared/         # Layout and partial views
│   └── [Controller]/   # Controller-specific views
├── src/                # Source assets (pre-build)
│   ├── js/            # JavaScript source files
│   └── scss/          # SCSS source files
├── wwwroot/           # Compiled/static assets
│   ├── css/          # Generated CSS files
│   ├── js/           # Generated JavaScript files
│   └── fonts/        # Font assets
└── package.json      # npm configuration
```

## GitHub Copilot Best Practices

### 🎯 Prompting Guidelines

#### For .NET MVC Development

```
"Create a controller action that returns a view with Bootstrap 5 card layout"
"Generate a model with data annotations for form validation"
"Create a Razor view using only Bootstrap 5 classes for a responsive form"
```

#### For Frontend Development

```
"Create JavaScript validation using native Bootstrap 5 form validation classes"
"Generate webpack configuration for SCSS compilation with Bootstrap 5"
"Create a responsive navigation using only Bootstrap 5 navbar classes"
```

### 🛠️ Development Workflows

#### Backend Development (.NET MVC)

1. **Controllers**: Follow MVC patterns with dependency injection
2. **Models**: Use data annotations for validation
3. **Views**: Razor syntax with Bootstrap 5 markup only
4. **Configuration**: Leverage appsettings.json for environment-specific settings

#### Frontend Development (npm/Webpack)

1. **SCSS**: Import Bootstrap 5, no custom styles
2. **JavaScript**: Vanilla JS or minimal libraries
3. **Build Process**: Use npm scripts for development and production builds
4. **Asset Management**: Webpack handles bundling and optimization

### 📝 Code Generation Examples

#### MVC Controller with Bootstrap 5 View

```csharp
// When asking Copilot to generate controllers:
"Create a UserController with CRUD operations and views that use Bootstrap 5 cards and forms"
```

#### Responsive Form with Bootstrap 5

```html
<!-- Always request Bootstrap 5 specific markup -->
"Generate a registration form using Bootstrap 5 form controls and validation classes"
```

#### JavaScript with Bootstrap Integration

```javascript
// Request Bootstrap-compatible JavaScript
"Create form validation JavaScript that works with Bootstrap 5 form validation classes"
```

### 🔧 Build Process Integration

#### npm Scripts

- `npm run build` - Production build
- `npm run build:dev` - Development build
- `npm run watch` - Development with file watching
- `npm run clean` - Clean generated assets

#### Visual Studio Code Tasks

- **Run TeachSpark.Web**: Starts the .NET application
- **Build Frontend Assets**: Compiles client-side assets
- **Watch Frontend Assets**: Watches for changes during development

### 📋 Copilot Prompt Templates

#### For New Features

```
Create a [feature name] with:
- .NET MVC controller with [specific actions]
- Razor view using Bootstrap 5 [specific components]
- JavaScript functionality using vanilla JS
- Form validation with Bootstrap 5 validation classes
```

#### For UI Components

```
Generate a [component type] using:
- Bootstrap 5 [specific classes/components]
- Responsive design for mobile and desktop
- Accessibility attributes
- No custom CSS or inline styles
```

#### For Data Operations

```
Create a [operation type] that:
- Uses Entity Framework Core (if applicable)
- Includes proper error handling
- Returns appropriate HTTP status codes
- Follows async/await patterns
```

### 🚫 Anti-Patterns to Avoid

#### Styling Anti-Patterns

- ❌ Custom CSS classes
- ❌ Inline styles
- ❌ CSS frameworks other than Bootstrap 5
- ❌ Custom SCSS variables overriding Bootstrap

#### Code Anti-Patterns

- ❌ Mixing server-side and client-side logic inappropriately
- ❌ Hardcoded values instead of configuration
- ❌ Synchronous operations for async-capable tasks
- ❌ Missing error handling and validation

### 🎯 Specific Copilot Instructions

#### For HTML/Razor Views

```
"Generate HTML using ONLY Bootstrap 5 classes. Never use custom CSS or inline styles. 
Focus on responsive design with Bootstrap's grid system and utility classes."
```

#### For JavaScript

```
"Create vanilla JavaScript that integrates with Bootstrap 5 components. 
Use Bootstrap's built-in validation and modal systems."
```

#### For .NET Controllers

```
"Generate MVC controllers following REST principles with proper async/await patterns, 
error handling, and dependency injection."
```

### 📚 Bootstrap 5 Quick Reference

#### Common Layout Patterns

- **Container**: `container`, `container-fluid`
- **Grid**: `row`, `col-*`, `col-sm-*`, `col-md-*`, `col-lg-*`, `col-xl-*`
- **Flexbox**: `d-flex`, `justify-content-*`, `align-items-*`
- **Spacing**: `m-*`, `p-*`, `mt-*`, `mb-*`, `ms-*`, `me-*`

#### Component Classes

- **Cards**: `card`, `card-header`, `card-body`, `card-footer`
- **Buttons**: `btn`, `btn-primary`, `btn-secondary`, `btn-lg`, `btn-sm`
- **Forms**: `form-control`, `form-label`, `form-text`, `was-validated`
- **Navigation**: `navbar`, `nav`, `nav-link`, `nav-pills`, `nav-tabs`

### 🔍 Quality Assurance

#### Before Committing

1. Verify all HTML uses only Bootstrap 5 classes
2. Ensure no custom CSS or inline styles exist
3. Test responsive behavior on multiple screen sizes
4. Validate form functionality and accessibility
5. Run `npm run quality` to check code standards

#### Code Review Checklist

- [ ] No custom CSS classes or inline styles
- [ ] Bootstrap 5 classes used appropriately
- [ ] Responsive design implemented
- [ ] Accessibility attributes included
- [ ] JavaScript follows vanilla JS patterns
- [ ] .NET code follows MVC best practices

### 🎉 Success Metrics

A successful implementation using these guidelines will have:

- 100% Bootstrap 5 native styling
- Responsive design across all devices
- Clean separation of concerns
- Maintainable and readable code
- Fast build times with webpack optimization
- Accessible user interface

---

## Quick Start for Copilot Users

1. **Always specify "Bootstrap 5 only"** in your prompts
2. **Request responsive designs** for all UI components
3. **Ask for vanilla JavaScript** rather than jQuery or other libraries
4. **Specify MVC patterns** for backend development
5. **Include accessibility requirements** in UI prompts

Remember: The goal is to create a maintainable, professional web application using industry standards and best practices while leveraging Bootstrap 5's comprehensive design system.
