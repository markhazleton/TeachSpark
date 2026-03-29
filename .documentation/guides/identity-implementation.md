# TeachSpark Identity Implementation

## Overview

This implementation adds Microsoft.AspNetCore.Identity to the TeachSpark.Web application using SQLite database for user authentication and authorization.

## Database Location

- **SQLite Database**: `c:\websites\teachspark\teachspark.db`
- **Connection String**: `Data Source=c:\\websites\\teachspark\\teachspark.db`

## Features Implemented

### 1. User Authentication

- **User Registration**: Users can create new accounts
- **User Login/Logout**: Secure authentication system
- **Password Requirements**:
  - Minimum 8 characters
  - Requires uppercase, lowercase, and digit
  - No special characters required

### 2. Custom User Model

- **ApplicationUser** extends IdentityUser with additional fields:
  - `FirstName`: User's first name
  - `LastName`: User's last name
  - `CreatedAt`: Account creation timestamp
  - `LastLoginAt`: Last login timestamp
  - `IsActive`: Account status flag

### 3. Database Context

- **ApplicationDbContext** extends IdentityDbContext
- Configured with SQLite provider
- Custom entity configurations for ApplicationUser
- Unique email constraint

### 4. Security Configuration

- **Lockout Settings**:
  - 30-minute lockout duration
  - Maximum 5 failed attempts
  - Enabled for new users
- **Cookie Settings**:
  - 14-day expiration
  - Sliding expiration enabled
  - Custom paths for login/logout

### 5. UI Components

- **Bootstrap 5 Navigation**: Responsive authentication menu
- **Login Partial**: Dynamic login/logout links with user dropdown
- **Identity Pages**: Scaffolded registration, login, and account management
- **User Dashboard**: Protected area showing user information
- **Profile Page**: Detailed user profile view

### 6. Authorization

- **Account Controller**: Requires authentication
- **Dashboard**: Protected user area
- **Profile Management**: Secure user data access

## File Structure

```
TeachSpark.Web/
├── Controllers/
│   └── AccountController.cs          # User dashboard and profile controller
├── Data/
│   └── ApplicationDbContext.cs       # Entity Framework DbContext
├── Models/
│   └── ApplicationUser.cs            # Custom user model
├── Views/
│   ├── Account/
│   │   ├── Dashboard.cshtml          # User dashboard
│   │   └── Profile.cshtml            # User profile
│   ├── Shared/
│   │   ├── _Layout.cshtml            # Updated with auth navigation
│   │   └── _LoginPartial.cshtml      # Authentication menu
│   └── Home/
│       └── Index.cshtml              # Updated with auth status
├── Areas/
│   └── Identity/                     # Scaffolded Identity pages
├── Migrations/
│   └── [timestamp]_InitialIdentity.cs # EF Core migration
└── Program.cs                        # Identity service configuration
```

## Usage

### For Developers

1. **Run the application**: `dotnet run`
2. **Access authentication**: Navigate to `/Identity/Account/Register` or `/Identity/Account/Login`
3. **Protected areas**: Visit `/Account/Dashboard` (requires login)

### For Users

1. **Register**: Click "Register" in navigation to create account
2. **Login**: Use email and password to authenticate
3. **Dashboard**: Access user dashboard after login
4. **Profile**: View and manage profile information
5. **Settings**: Manage account settings through Identity pages

## Database Schema

The SQLite database includes standard Identity tables:

- `AspNetUsers`: User accounts with custom fields
- `AspNetRoles`: User roles (if role-based auth is added)
- `AspNetUserClaims`: User claims
- `AspNetUserLogins`: External login providers
- `AspNetUserRoles`: User-role mappings
- `AspNetUserTokens`: Authentication tokens
- `AspNetRoleClaims`: Role-based claims

## Security Notes

- Passwords are hashed using ASP.NET Core Identity's default hasher
- Email confirmation is disabled by default (can be enabled)
- SQLite database includes WAL mode for better concurrency
- Connection string should be secured in production environments

## Next Steps

To enhance the authentication system:

1. **Add Role-based Authorization**: Implement admin/user roles
2. **Enable Email Confirmation**: Set up email service
3. **Add External Providers**: Google, Microsoft, etc.
4. **Implement Two-Factor Authentication**: SMS or authenticator apps
5. **Add Account Lockout Notifications**: Email alerts for security events
