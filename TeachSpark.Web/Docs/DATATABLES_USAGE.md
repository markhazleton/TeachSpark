# DataTables.js Usage Guide

DataTables.js has been successfully added to your TeachSpark.Web project. Here's how to use it:

## Basic Usage

### 1. Add the `datatable` class to any table

```html
<table class="table table-striped datatable">
    <thead>
        <tr>
            <th>Name</th>
            <th>Email</th>
            <th>Date Created</th>
            <th>Actions</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>John Doe</td>
            <td>john@example.com</td>
            <td>2024-01-15</td>
            <td>
                <button class="btn btn-sm btn-primary">Edit</button>
                <button class="btn btn-sm btn-danger">Delete</button>
            </td>
        </tr>
        <!-- more rows -->
    </tbody>
</table>
```

### 2. The table will automatically be initialized with DataTables features

- Searching
- Sorting
- Pagination (25 records per page by default)
- Responsive design
- Bootstrap 5 styling

## Advanced Usage

### Manual initialization with custom options

```javascript
// Initialize a specific table with custom options
TeachSpark.dataTable.init('#my-custom-table', {
    pageLength: 50,
    order: [[2, 'desc']], // Sort by third column descending
    columnDefs: [
        { orderable: false, targets: -1 } // Disable sorting on last column (actions)
    ]
});
```

### Refresh data in an AJAX DataTable

```javascript
TeachSpark.dataTable.refresh('#my-table');
```

### Destroy a DataTable

```javascript
TeachSpark.dataTable.destroy('#my-table');
```

## Features Included

- **Search**: Global search across all columns
- **Sort**: Click column headers to sort
- **Pagination**: Navigate through pages of data
- **Length menu**: Choose how many records to show (10, 25, 50, 100)
- **Responsive**: Tables adapt to different screen sizes
- **Bootstrap 5 styling**: Consistent with your site's theme

## Customization

The DataTables styling has been customized to match your site's Bootstrap theme:

- Primary color (#1b6ec2) for pagination and focus states
- Consistent form styling for search and length inputs
- Hover effects on table rows
- Custom header styling

## Notes

- Tables with the `datatable` class are automatically initialized when the page loads
- Make sure your table has proper `<thead>` and `<tbody>` structure
- For large datasets, consider server-side processing
- The DataTables instance is available globally through `TeachSpark.dataTable` utilities
