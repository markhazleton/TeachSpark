namespace TeachSpark.Web.Services.Prompts
{
    /// <summary>
    /// Manages prompt templates for different worksheet types and educational contexts
    /// </summary>
    public static class PromptTemplates
    {
        /// <summary>
        /// Base system prompt that establishes the AI's role as an educational content creator
        /// </summary>
        public static string SystemPrompt => @"
You are an expert educational content creator specializing in middle school English Language Arts (8th grade). 
Your task is to create high-quality, pedagogically sound worksheets that align with Common Core Standards and Bloom's Taxonomy.

Key principles:
- Always align content with specified Common Core Standards
- Create questions at the appropriate Bloom's Taxonomy level
- Ensure age-appropriate content and vocabulary
- Provide clear, engaging instructions
- Include diverse question types to accommodate different learning styles
- Consider accessibility needs when specified

Output format: Always return valid JSON with the structure specified in the user prompt.
";        /// <summary>
          /// Template for reading comprehension worksheets
          /// </summary>
        public static string ReadingComprehensionTemplate => @"
Create a reading comprehension worksheet based on the following text passage.

**Source Text:**
{sourceText}

**Requirements:**
- Grade Level: {gradeLevel}
- Difficulty: {difficultyLevel}
- Common Core Standard: {commonCoreStandard}
- Bloom's Level Focus: {bloomLevel}
- Number of Questions: {maxQuestions}
- Include Answer Key: {includeAnswerKey}

**Accessibility Options:** {accessibilityOptions}

**Output Format: Return ONLY clean Markdown content (no code blocks, no JSON)**

Generate a complete worksheet using this structure:

```
# [Worksheet Title]

**Subject:** English Language Arts  
**Grade Level:** 8th Grade  
**Estimated Time:** 20-30 minutes  
**Standards:** [Common Core Standard Code]

## Instructions

[Clear, student-friendly instructions for completing the worksheet]

## Reading Passage

[The reading passage - original or adapted for grade level]

## Questions

### Part A: Comprehension Questions

1. **[Question Type]** [Question text]
   - A) [Option A]
   - B) [Option B] 
   - C) [Option C]
   - D) [Option D]

2. **[Question Type]** [Short answer question]
   
   *[Space for student response]*

### Part B: Analysis Questions

[Continue with higher-order thinking questions]

### Part C: Extension Activity

[Creative or application-based activity]

## Answer Key

| Question | Answer | Explanation |
|----------|--------|-------------|
| 1 | C | [Brief explanation] |
| 2 | [Sample answer] | [Rubric guidance] |

## Vocabulary

| Word | Definition | Example Sentence |
|------|------------|------------------|
| [word] | [definition] | [usage example] |

## Assessment Rubric

### Short Answer Questions (2-3 points each)
- **Excellent (3):** [Criteria]
- **Good (2):** [Criteria]  
- **Needs Improvement (1):** [Criteria]

---
*This worksheet aligns with Common Core Standard {commonCoreStandard}*
```

Focus on:
- Clear section headers with ##
- Numbered questions with appropriate Bloom's levels
- Tables for answer keys and vocabulary
- Professional formatting for teacher use
- Copy-friendly structure
";

        /// <summary>
        /// Template for vocabulary worksheets
        /// </summary>
        public static string VocabularyTemplate => @"
Create a vocabulary worksheet based on the following text or word list.

**Source Material:**
{sourceText}

**Requirements:**
- Grade Level: {gradeLevel}
- Difficulty: {difficultyLevel}
- Common Core Standard: {commonCoreStandard}
- Bloom's Level Focus: {bloomLevel}
- Number of Words: {maxQuestions}
- Include Answer Key: {includeAnswerKey}

**Vocabulary Activities to Include:**
- Word definitions
- Context clues
- Word usage in sentences
- Synonyms and antonyms
- Word roots/etymology (if appropriate)
- Application exercises

**JSON Output Format:**
```json
{
  ""title"": ""Vocabulary Worksheet Title"",
  ""description"": ""Worksheet description"",
  ""instructions"": ""Student instructions"",
  ""vocabularyWords"": [
    {
      ""word"": ""vocabulary word"",
      ""definition"": ""clear, grade-appropriate definition"",
      ""partOfSpeech"": ""noun/verb/adjective/etc."",
      ""example"": ""example sentence using the word"",
      ""context"": ""original context from source text if applicable""
    }
  ],
  ""activities"": [
    {
      ""type"": ""matching"" | ""fill-blank"" | ""multiple-choice"" | ""sentence-creation"",
      ""instructions"": ""Activity-specific instructions"",
      ""questions"": [
        {
          ""id"": 1,
          ""question"": ""Question text"",
          ""answer"": ""Correct answer"",
          ""options"": [""option1"", ""option2""] // if applicable
        }
      ]
    }
  ],
  ""answerKey"": {
    ""answers"": [
      {
        ""activityType"": ""matching"",
        ""answers"": [""1-A"", ""2-B"", ""3-C""]
      }
    ]
  }
}
```
";

        /// <summary>
        /// Template for grammar worksheets
        /// </summary>
        public static string GrammarTemplate => @"
Create a grammar worksheet focusing on the specified grammatical concepts.

**Grammar Focus:** {grammarFocus}
**Source Text (if applicable):** {sourceText}

**Requirements:**
- Grade Level: {gradeLevel}
- Difficulty: {difficultyLevel}
- Common Core Standard: {commonCoreStandard}
- Bloom's Level Focus: {bloomLevel}
- Number of Questions: {maxQuestions}

**Grammar Activities:**
- Identification exercises
- Correction exercises
- Application in original sentences
- Explanation of rules
- Pattern recognition

**JSON Output Format:**
```json
{
  ""title"": ""Grammar Worksheet Title"",
  ""description"": ""Focus on [grammar concept]"",
  ""grammarConcept"": ""The main grammar concept being taught"",
  ""ruleExplanation"": ""Clear explanation of the grammar rule"",
  ""examples"": [
    ""Example sentence 1"",
    ""Example sentence 2""
  ],
  ""exercises"": [
    {
      ""type"": ""identification"" | ""correction"" | ""completion"" | ""creation"",
      ""instructions"": ""Exercise instructions"",
      ""questions"": [
        {
          ""id"": 1,
          ""text"": ""Sentence or prompt"",
          ""answer"": ""Correct answer"",
          ""explanation"": ""Why this is correct""
        }
      ]
    }
  ]
}
```
";

        /// <summary>
        /// Template for creative writing prompts
        /// </summary>
        public static string CreativeWritingTemplate => @"
Create a creative writing worksheet with engaging prompts and scaffolding.

**Writing Focus:** {writingFocus}
**Inspiration Source:** {sourceText}

**Requirements:**
- Grade Level: {gradeLevel}
- Difficulty: {difficultyLevel}
- Common Core Standard: {commonCoreStandard}
- Bloom's Level Focus: {bloomLevel}
- Writing Type: {writingType} (narrative, expository, persuasive, descriptive)

**JSON Output Format:**
```json
{
  ""title"": ""Creative Writing Worksheet"",
  ""writingType"": ""narrative/expository/persuasive/descriptive"",
  ""mainPrompt"": ""Primary writing prompt"",
  ""preWritingActivities"": [
    {
      ""activity"": ""brainstorming"",
      ""instructions"": ""What to brainstorm"",
      ""template"": ""Graphic organizer or template""
    }
  ],
  ""writingPrompts"": [
    {
      ""prompt"": ""Writing prompt text"",
      ""requirements"": [""requirement1"", ""requirement2""],
      ""wordCount"": ""150-200 words"",
      ""timeLimit"": ""30 minutes""
    }
  ],
  ""scaffolding"": {
    ""vocabularySupport"": [""helpful words""],
    ""sentenceStarters"": [""In my opinion..."", ""First...""],
    ""organizationTips"": [""tip1"", ""tip2""]
  },
  ""rubric"": {
    ""criteria"": [
      {
        ""criterion"": ""Ideas and Content"",
        ""levels"": [""Excellent"", ""Good"", ""Satisfactory"", ""Needs Improvement""],
        ""descriptions"": [""Clear description for each level""]
      }
    ]
  }
}
```
";

        /// <summary>
        /// Gets the appropriate template based on worksheet type
        /// </summary>
        public static string GetTemplate(string worksheetType)
        {
            return worksheetType.ToLower() switch
            {
                "reading-comprehension" => ReadingComprehensionTemplate,
                "vocabulary" => VocabularyTemplate,
                "grammar" => GrammarTemplate,
                "creative-writing" => CreativeWritingTemplate,
                _ => ReadingComprehensionTemplate // default
            };
        }

        /// <summary>
        /// Bloom's Taxonomy level modifiers for prompts
        /// </summary>
        public static Dictionary<string, string> BloomLevelModifiers => new()
        {
            ["remember"] = "Focus on recall and recognition. Ask students to identify, list, name, or describe basic facts and concepts.",
            ["understand"] = "Focus on comprehension. Ask students to explain, summarize, paraphrase, or give examples.",
            ["apply"] = "Focus on using knowledge in new situations. Ask students to solve problems, demonstrate, or use concepts in new contexts.",
            ["analyze"] = "Focus on breaking down information. Ask students to compare, contrast, categorize, or examine relationships.",
            ["evaluate"] = "Focus on making judgments. Ask students to critique, assess, justify, or defend positions.",
            ["create"] = "Focus on producing new work. Ask students to design, compose, plan, or construct something original."
        };

        /// <summary>
        /// Difficulty level modifiers
        /// </summary>
        public static Dictionary<string, string> DifficultyModifiers => new()
        {
            ["simplified"] = "Use simpler vocabulary, shorter sentences, and provide more scaffolding. Include more explicit instructions and examples.",
            ["standard"] = "Use grade-appropriate vocabulary and complexity. Balance challenge with accessibility.",
            ["advanced"] = "Use more sophisticated vocabulary and complex sentence structures. Include higher-order thinking challenges."
        };
    }
}
