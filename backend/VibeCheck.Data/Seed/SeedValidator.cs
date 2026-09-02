namespace VibeCheck.Data.Data;

public static class SeedValidator
{
    public static void ValidateQuestions(
        List<QuestionSeed> questions,
        List<QuestionAlternativeSeed> alternatives)
    {
        var errors = new List<string>();

        foreach (var question in questions)
        {
            var questionAlternatives = alternatives
                .Where(a => a.Question == question.QuestionDesc)
                .ToList();

            // Check that the question has alternatives
            if (questionAlternatives.Count == 0)
            {
                errors.Add(
                    $"Question '{question.QuestionDesc}' has no alternatives.");

                continue;
            }

            // Check that the question has exactly one correct alternative
            var correctCount = questionAlternatives
                .Count(a => a.IsCorrect);

            if (correctCount == 0)
            {
                errors.Add(
                    $"Question '{question.QuestionDesc}' has no correct alternative.");
            }
            else if (correctCount > 1)
            {
                errors.Add(
                    $"Question '{question.QuestionDesc}' has {correctCount} correct alternatives.");
            }
        }

        // Check that every alternative references an existing question
        foreach (var alternative in alternatives)
        {
            if (!questions.Any(q => q.QuestionDesc == alternative.Question))
            {
                errors.Add(
                    $"Alternative '{alternative.AlternativeText}' references unknown question '{alternative.Question}'.");
            }
        }

        // Check for duplicate alternatives within the same question
        var duplicateAlternatives = alternatives
            .GroupBy(a => new
            {
                a.Question,
                a.AlternativeText
            })
            .Where(g => g.Count() > 1);

        foreach (var duplicate in duplicateAlternatives)
        {
            errors.Add(
                $"Question '{duplicate.Key.Question}' contains duplicate alternative '{duplicate.Key.AlternativeText}'.");
        }

        // Check for empty question values
        foreach (var question in questions)
        {
            if (string.IsNullOrWhiteSpace(question.QuestionDesc))
            {
                errors.Add(
                    "A question has an empty QuestionDesc.");
            }

            if (string.IsNullOrWhiteSpace(question.QuestionType))
            {
                errors.Add(
                    $"Question '{question.QuestionDesc}' has an empty QuestionType.");
            }

            if (string.IsNullOrWhiteSpace(question.Difficulty))
            {
                errors.Add(
                    $"Question '{question.QuestionDesc}' has an empty Difficulty.");
            }
        }

        // Check for empty alternative values
        foreach (var alternative in alternatives)
        {
            if (string.IsNullOrWhiteSpace(alternative.Question))
            {
                errors.Add(
                    "An alternative has an empty Question reference.");
            }

            if (string.IsNullOrWhiteSpace(alternative.AlternativeText))
            {
                errors.Add(
                    $"Alternative for question '{alternative.Question}' has empty AlternativeText.");
            }
        }

        // Stop seeding if any validation errors were found
        if (errors.Count > 0)
        {
            var message =
                "Seed validation failed:\n\n" +
                string.Join("\n", errors.Select(error => $"- {error}"));

            throw new Exception(message);
        }
    }
}