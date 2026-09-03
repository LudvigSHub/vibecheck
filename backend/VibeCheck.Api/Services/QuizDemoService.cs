using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using VibeCheck.Api.DTOs;
using VibeCheck.Data.Data;

namespace VibeCheck.Api.Services
{
    public class QuizDemoService
    {
        private readonly VibeCheckDbContext _context;

        // Antal svarsalternativ per fråga (1 rätt + 3 fel).
        private const int OptionsPerQuestion = 4;

        private static readonly string[] Letters = { "A", "B", "C", "D" };

        public QuizDemoService(VibeCheckDbContext context)
        {
            _context = context;
        }

        public async Task<List<QuizDemoQuestionDTO>> GetQuestionsAsync(int count)
        {
            // Ett ord utan exempelmening går inte att bygga en lucktext av.
            var words = await _context.Words
                .Include(w => w.Meaning)
                .Include(w => w.WordExamples)
                .Where(w => w.WordExamples.Any())
                .ToListAsync();

            // Behöver minst 4 ord totalt för att kunna fylla en enda fråga
            // (1 rätt + 3 fel). Räcker inte -> tomt resultat, ingen krasch.
            if (words.Count < OptionsPerQuestion)
            {
                return new List<QuizDemoQuestionDTO>();
            }

            var allWordTexts = words.Select(w => w.WordDesc).ToList();

            var chosenWords = words
                .OrderBy(_ => Random.Shared.Next())
                .Take(count)
                .ToList();

            var questions = new List<QuizDemoQuestionDTO>();

            foreach (var word in chosenWords)
            {
                var example = word.WordExamples
                    .OrderBy(_ => Random.Shared.Next())
                    .First();

                var quote = Regex.Replace(
                    example.ExampleText,
                    $@"\b{Regex.Escape(word.WordDesc)}\b",
                    "___",
                    RegexOptions.IgnoreCase);

                var distractors = allWordTexts
                    .Where(text => !string.Equals(text, word.WordDesc, StringComparison.OrdinalIgnoreCase))
                    .Distinct()
                    .OrderBy(_ => Random.Shared.Next())
                    .Take(OptionsPerQuestion - 1);

                var optionTexts = distractors
                    .Append(word.WordDesc)
                    .OrderBy(_ => Random.Shared.Next())
                    .ToList();

                var options = new List<QuizDemoOptionDTO>();
                var correctId = Letters[0];

                for (var i = 0; i < optionTexts.Count; i++)
                {
                    options.Add(new QuizDemoOptionDTO { Id = Letters[i], Text = optionTexts[i] });

                    if (string.Equals(optionTexts[i], word.WordDesc, StringComparison.OrdinalIgnoreCase))
                    {
                        correctId = Letters[i];
                    }
                }

                questions.Add(new QuizDemoQuestionDTO
                {
                    WordId = word.WordID,
                    Question = "Vilket slangord passar in i följande mening?",
                    Quote = quote,
                    Options = options,
                    CorrectId = correctId,
                    Explanation = word.Meaning.MeaningText
                });
            }

            return questions;

        }
    }
}
