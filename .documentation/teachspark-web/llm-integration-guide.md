# LLM Integration Setup Guide

## Overview

The TeachSpark worksheet generator uses OpenAI's API to generate educational content. This guide explains how to configure and use the LLM integration.

## Configuration

### 1. API Key Setup

The OpenAI API key should be configured in `appsettings.Development.json` for development or through user secrets/environment variables for production.

#### Development Configuration

Update `appsettings.Development.json`:

```json
{
  "LlmConfiguration": {
    "ApiKey": "sk-your-actual-openai-api-key-here",
    "DefaultModel": "gpt-4o-mini",
    "MaxTokens": 4000,
    "Temperature": 0.7
  }
}
```

#### Production Configuration

For production, use user secrets or environment variables:

```bash
# Using user secrets (recommended for development)
dotnet user-secrets set "LlmConfiguration:ApiKey" "sk-your-actual-openai-api-key-here"

# Using environment variables (recommended for production)
export LlmConfiguration__ApiKey="sk-your-actual-openai-api-key-here"
```

### 2. Model Configuration

The system supports various OpenAI models:

- **gpt-4o-mini** (default) - Cost-effective for most educational content
- **gpt-4o** - Higher quality but more expensive
- **gpt-3.5-turbo** - Legacy support, lower cost

### 3. Cost Management

Configure cost limits to prevent unexpected charges:

```json
{
  "LlmConfiguration": {
    "MaxCostPerRequest": 1.0,
    "DailyCostLimit": 50.0,
    "RequestsPerMinute": 20,
    "RequestsPerDay": 1000
  }
}
```

## Features

### Worksheet Generation

The LLM service can generate various types of educational worksheets:

1. **Reading Comprehension** - Questions based on provided text passages
2. **Vocabulary** - Word exercises with context clues and definitions
3. **Grammar** - Sentence structure and grammar rule exercises
4. **Creative Writing** - Writing prompts and story starters
5. **Literary Analysis** - Character, theme, and literary device questions

### Content Quality

- All generated content is aligned with educational standards
- Questions span multiple Bloom's Taxonomy levels
- Content is automatically validated for appropriateness
- Answer keys are generated when requested

### Bootstrap 5 Styling

The worksheet generator UI is built around Bootstrap 5 components and utility classes, with existing stylesheet customizations still present in the application:

- Cards and containers for layout
- Form controls with proper validation
- Responsive design for all screen sizes
- Accessible color schemes and contrast
- New UI work should continue to prefer Bootstrap 5 classes over additional custom styling

## Usage

### Basic Workflow

1. Navigate to the Worksheet Generator
2. Choose worksheet type and difficulty level
3. Paste source text (for reading comprehension, vocabulary, etc.)
4. Configure generation options (number of questions, Common Core standards)
5. Select AI model (optional - auto-selects best option)
6. Generate worksheet
7. View, copy, or print the generated content

### API Endpoints

The system provides REST API endpoints for programmatic access:

- `GET /api/models` - Get available AI models
- `GET /api/models/recommend?worksheetType={type}` - Get model recommendations
- `POST /api/models/estimate-cost` - Estimate generation cost

### Output Formats

Generated worksheets are available in multiple formats:

- **Formatted HTML** - Ready for web display with proper styling
- **Markdown** - Source format for editing and version control
- **Print Version** - Student-friendly version without answer keys

## Troubleshooting

### Common Issues

1. **API Key Not Found**
   - Verify the API key is properly configured
   - Check user secrets or environment variables
   - Ensure the key has proper permissions

2. **Generation Fails**
   - Check internet connectivity
   - Verify OpenAI service status
   - Review cost limits and usage quotas

3. **Poor Quality Output**
   - Try a higher-quality model (gpt-4o instead of gpt-4o-mini)
   - Provide more detailed source text
   - Add specific instructions in the custom instructions field

### Error Messages

- **"Configuration validation failed"** - API key or model configuration issue
- **"Failed to generate worksheet"** - OpenAI service error or rate limiting
- **"Cost limit exceeded"** - Daily or per-request cost limits reached

## Best Practices

### Content Generation

- Provide clear, well-structured source text
- Use appropriate difficulty levels for target grade
- Include specific instructions for unique requirements
- Review generated content before distributing to students

### Performance

- Enable caching for repeated similar requests
- Use cost-effective models for simple worksheets
- Monitor usage and costs regularly
- Set appropriate timeout values for your network

### Security

- Never commit API keys to version control
- Use user secrets or secure environment variables
- Rotate API keys regularly
- Monitor API usage for unexpected activity

## Support

For issues with the LLM integration:

1. Check the application logs for detailed error messages
2. Verify OpenAI service status at status.openai.com
3. Review your OpenAI account usage and billing
4. Contact the development team for application-specific issues
