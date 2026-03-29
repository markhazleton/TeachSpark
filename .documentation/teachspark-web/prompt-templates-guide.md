# Worksheet Template Prompt Templates Guide

## Overview

Worksheet templates now support System and User prompt templates that can be used with AI models to generate dynamic worksheet content. These templates support substitution tokens that get replaced with actual values during the worksheet generation process.

## Available Substitution Tokens

The following tokens can be used in both System and User prompt templates:

### Core Educational Tokens

- **`{{BLOOM_LEVEL}}`** - The selected Bloom's taxonomy level (e.g., "Remember", "Understand", "Apply", "Analyze", "Evaluate", "Create")
- **`{{COMMON_CORE_STANDARD}}`** - The selected Common Core standard code and description
- **`{{GRADE_LEVEL}}`** - The target grade level for the worksheet (e.g., "3rd Grade", "High School")
- **`{{TEMPLATE_TYPE}}`** - The type of this template (e.g., "reading-comprehension", "vocabulary", "math-practice")

### Content Tokens

- **`{{INPUT_TEXT}}`** - User-provided input text or reading passage
- **`{{WORKSHEET_INSTRUCTIONS}}`** - Specific instructions provided for the worksheet generation

### Utility Tokens

- **`{{STUDENT_NAME}}`** - Placeholder for student name (useful for personalization)
- **`{{DATE}}`** - Current date in a readable format

## Usage Examples

### System Prompt Template Example

```
You are an expert educational content creator specializing in {{TEMPLATE_TYPE}} worksheets. 
Create engaging, age-appropriate content for {{GRADE_LEVEL}} students that aligns with the 
{{BLOOM_LEVEL}} level of Bloom's taxonomy. Ensure all content meets the requirements of 
Common Core Standard: {{COMMON_CORE_STANDARD}}.

Focus on creating clear, structured questions and activities that promote critical thinking 
and learning. Use vocabulary and concepts appropriate for the target grade level.
```

### User Prompt Template Example

```
Create a {{TEMPLATE_TYPE}} worksheet with the following specifications:

Instructions: {{WORKSHEET_INSTRUCTIONS}}
Grade Level: {{GRADE_LEVEL}}
Bloom's Level: {{BLOOM_LEVEL}}
Common Core Standard: {{COMMON_CORE_STANDARD}}

Input Text/Passage:
{{INPUT_TEXT}}

Generate 8-10 questions or activities that progressively increase in difficulty and 
thoroughly assess student understanding. Include a mix of question types appropriate 
for the {{BLOOM_LEVEL}} level.

Format the output as a structured worksheet with:
1. Title and student information section
2. Clear instructions
3. Numbered questions/activities
4. Adequate space for answers
5. Optional extension activities
```

## Best Practices

### System Prompt Templates

1. **Set the Context**: Clearly define the AI's role as an educational content creator
2. **Specify Quality Standards**: Mention grade-appropriate language, educational objectives
3. **Include Guidelines**: Reference educational frameworks like Bloom's taxonomy and Common Core
4. **Keep it Consistent**: Use similar structure across different template types

### User Prompt Templates

1. **Be Specific**: Provide clear instructions about format, length, and expectations
2. **Use All Relevant Tokens**: Include all tokens that add value to the generation
3. **Structure Requests**: Break down requirements into numbered or bulleted lists
4. **Provide Format Guidelines**: Specify how the output should be structured

### Token Usage Guidelines

1. **Always Use Double Braces**: `{{TOKEN_NAME}}` format is required
2. **Case Sensitive**: Use exact token names as specified above
3. **Graceful Fallbacks**: Consider what happens if a token is empty or null
4. **Context Awareness**: Ensure tokens make sense in the context of your template

## Template Management

### Default Prompt Templates

The system provides intelligent default prompt templates that are automatically applied based on the template type:

- **Automatic Population**: When creating or editing templates, default prompts are automatically loaded for empty fields when you select a template type
- **Template-Specific Defaults**: Each template type (reading-comprehension, vocabulary, grammar, math-practice, writing-prompt) has customized default prompts
- **Manual Loading**: Use the "Load Default Prompts for Selected Type" button to manually populate or replace prompt content
- **Smart Overwrite Protection**: The system asks for confirmation before overwriting existing prompt content

#### Available Default Template Types

- **Reading Comprehension**: Focused on literal, inferential, and critical thinking questions
- **Vocabulary**: Emphasizes word relationships, context clues, and practical application
- **Grammar**: Covers rule explanation, guided practice, and real-world application
- **Math Practice**: Includes computational problems, word problems, and multi-step reasoning
- **Writing Prompt**: Provides structured writing process guidance and assessment rubrics
- **Generic**: Flexible default for other template types

### Creating New Templates

1. Navigate to Admin → Worksheet Templates → Create New
2. Fill in basic template information (Name, Type, Description)
3. Select a template type to automatically load default prompts
4. Define the Layout JSON structure
5. Customize the auto-populated System and User prompt templates as needed
6. Use the "Load Default Prompts" button if you need to reset or load prompts manually
7. Set visibility and system flags as needed

### Editing Existing Templates

1. Navigate to Admin → Worksheet Templates
2. Click Edit on the desired template
3. Existing prompt templates are automatically loaded with defaults applied if they were previously empty
4. Modify prompt templates as needed
5. Use the "Load Default Prompts" button to reset to defaults (with confirmation prompt)
6. Use the collapsible token reference for guidance
7. Save changes

**Note**: When editing templates, the system will only auto-populate empty prompt fields. Existing content is preserved unless manually overwritten.

### Token Reference

The admin interface includes a collapsible token reference section that provides:

- Complete list of available tokens
- Brief description of each token
- Usage guidelines
- Examples of token replacement

## Integration with Worksheet Generation

When a worksheet is generated using a template with prompt templates:

1. The system retrieves the template's System and User prompt templates
2. All substitution tokens are replaced with actual values from the generation request
3. The processed prompts are sent to the configured AI model
4. The AI response is used to populate the worksheet content
5. The final worksheet is rendered using the template's Layout JSON structure

## Troubleshooting

### Common Issues

- **Missing Tokens**: If a token doesn't get replaced, check the exact spelling and case
- **Empty Values**: Some tokens may be empty if not provided in the generation request
- **Format Issues**: Ensure prompts are well-formed and don't break when tokens are replaced

### Testing Templates

- Use the Details view to preview how prompts will look
- Test with sample data to ensure token replacement works correctly
- Verify that generated content meets educational objectives

## Future Enhancements

Planned improvements to the prompt template system:

- Additional educational framework tokens (e.g., state standards)
- Conditional token logic
- Template inheritance and composition
- Advanced formatting options
- Preview mode with sample token replacement
