-- Check if any worksheets exist in the database
SELECT COUNT(*) as TotalWorksheets
FROM Worksheets;

-- Show all worksheets with basic info
SELECT
    Id,
    Title,
    UserId,
    WorksheetType,
    CreatedAt,
    CASE WHEN ContentMarkdown IS NOT NULL THEN 'Has Content' ELSE 'No Content' END as HasContent,
    LlmModel,
    TokensUsed,
    QuestionCount
FROM Worksheets
ORDER BY CreatedAt DESC
LIMIT 10;

-- Check database schema for Worksheets table
PRAGMA
table_info
(Worksheets);
