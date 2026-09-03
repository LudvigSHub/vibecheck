using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VibeCheck.Data.Models;
using System.Text.Json;
using VibeCheck.Data.Seed;

namespace VibeCheck.Data.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(
        VibeCheckDbContext context,
        UserManager<User> userManager,
        RoleManager<IdentityRole<int>> roleManager)
    {

        // Seed data
        await SeedRolesAsync(roleManager);
        await SeedUsersAsync(userManager);
        await SeedMeaningsAsync(context);
        await SeedWordsAsync(context);
        await SeedWordExamplesAsync(context);
        await SeedTagsAsync(context);
        await SeedWordTagsAsync(context);
        await SeedWordVotesAsync(context);
        await SeedQuestionTypesAsync(context);
        await SeedDifficultiesAsync(context);
        await SeedQuestionsAsync(context);
        await SeedQuestionAlternativesAsync(context);
        await SeedQuizzesAsync(context);
        await SeedQuizQuestionsAsync(context);
        await SeedQuizAttemptsAsync(context, userManager);
        await SeedQuizAttemptAnswersAsync(context);
    }

    // ============================================================
    // USERS
    // ============================================================

    //Skapar "admin" och "user" roller (om de inte redan finns i RoleManager)
    private static async Task SeedRolesAsync(RoleManager<IdentityRole<int>> roleManager)
    {
        foreach(var role in new[] { "admin", "user" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<int>(role));
            }
        }
    }

    private static async Task SeedUsersAsync(UserManager<User> userManager)
    {
        // Admin user
        var admin = await userManager.FindByNameAsync("admin");
        if (admin == null)
        {
            admin = new User
            {
                UserName = "admin",
                Email = "admin@vibecheck.local"
            };
            var result = await userManager.CreateAsync(admin, "Admin123!");
            if (!result.Succeeded)
            {
                throw new Exception(
                    "Failed to create admin user: " +
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        if (!await userManager.IsInRoleAsync(admin, "admin"))
        {
            await userManager.AddToRoleAsync(admin, "admin");
        }

        // Normal user
        var user = await userManager.FindByNameAsync("user");
        if (user == null)
        {
            user = new User
            {
                UserName = "user",
                Email = "user@vibecheck.local",
            };
            var result = await userManager.CreateAsync(user, "User123!");
            if (!result.Succeeded)
            {
                throw new Exception(
                    "Failed to create normal user: " +
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        if (!await userManager.IsInRoleAsync(user, "user"))
        {
            await userManager.AddToRoleAsync(user, "user");
        }
    }

    // ============================================================
    // MEANINGS
    // ============================================================

    private static async Task SeedMeaningsAsync(
        VibeCheckDbContext context)
    {
        // Only seed if the table is empty
        if (await context.Meanings.AnyAsync())
            return;

        var json = await File.ReadAllTextAsync(
            Path.Combine(AppContext.BaseDirectory, "Seed", "meanings.json"));

        var seedMeanings = JsonSerializer.Deserialize<List<MeaningSeed>>(json)
            ?? throw new Exception("Failed to deserialize meanings.json.");

        var meanings = seedMeanings.Select(seed => new Meaning
        {
            MeaningText = seed.MeaningText
        }).ToList();

        await context.Meanings.AddRangeAsync(meanings);
        await context.SaveChangesAsync();
    }

    // ============================================================
    // WORDS
    // ============================================================


    private static async Task SeedWordsAsync(
        VibeCheckDbContext context)
    {
        // Only seed if the table is empty
        if (await context.Words.AnyAsync())
            return;

        var json = await File.ReadAllTextAsync(
            Path.Combine(AppContext.BaseDirectory, "Seed", "words.json"));

        var seedWords = JsonSerializer.Deserialize<List<WordSeed>>(json)
            ?? throw new Exception("Failed to deserialize words.json.");

        // Get meanings by their text so we don't rely
        // on hardcoded MeaningID values.
        var meanings = await context.Meanings
            .ToDictionaryAsync(m => m.MeaningText);

        var words = new List<Word>();

        foreach (var seedWord in seedWords)
        {
            if (!meanings.TryGetValue(seedWord.Meaning, out var meaning))
            {
                throw new Exception(
                    $"Meaning '{seedWord.Meaning}' referenced by word " +
                    $"'{seedWord.WordDesc}' was not found.");
            }

            words.Add(new Word
            {
                WordDesc = seedWord.WordDesc,
                MeaningID = meaning.MeaningID
            });
        }

        await context.Words.AddRangeAsync(words);
        await context.SaveChangesAsync();
    }


    // ============================================================
    // WORDEXAMPLES
    // ============================================================
    private static async Task SeedWordExamplesAsync(
        VibeCheckDbContext context)
    {
        if (await context.WordExamples.AnyAsync())
            return;

        var jsonPath = Path.Combine(
            AppContext.BaseDirectory,
            "Seed",
            "wordexamples.json");

        var json = await File.ReadAllTextAsync(jsonPath);

        var seedExamples =
            JsonSerializer.Deserialize<List<WordExampleSeed>>(json)
            ?? throw new Exception(
                "Failed to deserialize wordexamples.json.");

        var words = await context.Words
            .ToDictionaryAsync(w => w.WordDesc);

        var examples = new List<WordExample>();

        foreach (var seed in seedExamples)
        {
            if (!words.TryGetValue(seed.Word, out var word))
            {
                throw new Exception(
                    $"Word '{seed.Word}' referenced in wordexamples.json was not found.");
            }

            examples.Add(new WordExample
            {
                WordID = word.WordID,
                ExampleText = seed.ExampleText
            });
        }

        await context.WordExamples.AddRangeAsync(examples);
        await context.SaveChangesAsync();
    }


    // ============================================================
    // TAGS
    // ============================================================
    private static async Task SeedTagsAsync(
        VibeCheckDbContext context)
    {
        if (await context.Tags.AnyAsync())
            return;

        var jsonPath = Path.Combine(
            AppContext.BaseDirectory,
            "Seed",
            "tags.json");

        var json = await File.ReadAllTextAsync(jsonPath);

        var seedTags =
            JsonSerializer.Deserialize<List<TagSeed>>(json)
            ?? throw new Exception(
                "Failed to deserialize tags.json.");

        var tags = seedTags.Select(seed => new Tag
        {
            TagName = seed.TagName
        }).ToList();

        await context.Tags.AddRangeAsync(tags);
        await context.SaveChangesAsync();
    }



    // ============================================================
    // WORDTAGS
    // ============================================================
    private static async Task SeedWordTagsAsync(
        VibeCheckDbContext context)
    {
        if (await context.WordTags.AnyAsync())
            return;

        var jsonPath = Path.Combine(
            AppContext.BaseDirectory,
            "Seed",
            "wordtags.json");

        var json = await File.ReadAllTextAsync(jsonPath);

        var seedWordTags =
            JsonSerializer.Deserialize<List<WordTagSeed>>(json)
            ?? throw new Exception(
                "Failed to deserialize wordtags.json.");

        var words = await context.Words
            .ToDictionaryAsync(w => w.WordDesc);

        var tags = await context.Tags
            .ToDictionaryAsync(t => t.TagName);

        var wordTags = new List<WordTag>();

        foreach (var seed in seedWordTags)
        {
            if (!words.TryGetValue(seed.Word, out var word))
            {
                throw new Exception(
                    $"Word '{seed.Word}' referenced in wordtags.json was not found.");
            }

            if (!tags.TryGetValue(seed.Tag, out var tag))
            {
                throw new Exception(
                    $"Tag '{seed.Tag}' referenced in wordtags.json was not found.");
            }

            wordTags.Add(new WordTag
            {
                WordID = word.WordID,
                TagID = tag.TagID
            });
        }

        await context.WordTags.AddRangeAsync(wordTags);
        await context.SaveChangesAsync();
    }

    // ============================================================
    // WORDVOTES
    // ============================================================
    private static async Task SeedWordVotesAsync(
    VibeCheckDbContext context)
    {
        if (await context.WordVotes.AnyAsync())
            return;

        var users = await context.Users
            .ToDictionaryAsync(u => u.UserName!);

        var words = await context.Words
            .ToDictionaryAsync(w => w.WordDesc);

        var wordVotes = new List<WordVote>
    {
        // Admin votes
        new()
        {
            UserID = users["admin"].Id,
            WordID = words["fire"].WordID,
            IsPositive = true
        },

        new()
        {
            UserID = users["admin"].Id,
            WordID = words["cringe"].WordID,
            IsPositive = false
        },

        new()
        {
            UserID = users["admin"].Id,
            WordID = words["sus"].WordID,
            IsPositive = false
        },

        // Normal user votes
        new()
        {
            UserID = users["user"].Id,
            WordID = words["fire"].WordID,
            IsPositive = true
        },

        new()
        {
            UserID = users["user"].Id,
            WordID = words["sus"].WordID,
            IsPositive = false
        }
    };

        await context.WordVotes.AddRangeAsync(wordVotes);
        await context.SaveChangesAsync();
    }

    // ============================================================
    // QUESTIONTYPES
    // ============================================================

    private static async Task SeedQuestionTypesAsync(
        VibeCheckDbContext context)
    {
        if (await context.QuestionTypes.AnyAsync())
            return;

        var questionTypes = new List<QuestionType>
    {
        new()
        {
            TypeText = "Multiple Choice",
            Description = "Vilket slangord passar bäst för att beskriva denna situation/person?"
        },

        new()
        {
            TypeText = "True or False",
            Description = "Är påståendet sant eller falskt?"
        }
    };

        await context.QuestionTypes.AddRangeAsync(questionTypes);
        await context.SaveChangesAsync();
    }

    // ============================================================
    // DIFFICULTIES
    // ============================================================

    private static async Task SeedDifficultiesAsync(
    VibeCheckDbContext context)
    {
        if (await context.Difficulties.AnyAsync())
            return;

        var difficulties = new List<Difficulty>
    {
        new()
        {
            DifficultyDesc = "Easy"
        },

        new()
        {
            DifficultyDesc = "Medium"
        },

        new()
        {
            DifficultyDesc = "Hard"
        }
    };

        await context.Difficulties.AddRangeAsync(difficulties);
        await context.SaveChangesAsync();
    }

    // ============================================================
    // QUESTIONS
    // ============================================================

    private static async Task SeedQuestionsAsync(
    VibeCheckDbContext context)
    {
        if (await context.Questions.AnyAsync())
            return;

        var jsonPath = Path.Combine(
            AppContext.BaseDirectory,
            "Seed",
            "questions.json");

        var json = await File.ReadAllTextAsync(jsonPath);

        var seedQuestions =
            JsonSerializer.Deserialize<List<QuestionSeed>>(json)
            ?? throw new Exception(
                "Failed to deserialize questions.json.");

        var questionTypes = await context.QuestionTypes
            .ToDictionaryAsync(qt => qt.TypeText);

        var difficulties = await context.Difficulties
            .ToDictionaryAsync(d => d.DifficultyDesc);

        var words = await context.Words
            .ToDictionaryAsync(w => w.WordDesc);

        var questions = new List<Question>();

        foreach (var seed in seedQuestions)
        {
            if (!questionTypes.TryGetValue(
                    seed.QuestionType,
                    out var questionType))
            {
                throw new Exception(
                    $"Question type '{seed.QuestionType}' referenced in questions.json was not found.");
            }

            if (!difficulties.TryGetValue(
                    seed.Difficulty,
                    out var difficulty))
            {
                throw new Exception(
                    $"Difficulty '{seed.Difficulty}' referenced in questions.json was not found.");
            }

            if (!words.TryGetValue(seed.Word, out var word))
            {
                throw new Exception(
                    $"Word '{seed.Word}' referenced by question '{seed.QuestionDesc}' was not found.");
            }

            questions.Add(new Question
            {
                QuestionDesc = seed.QuestionDesc,
                QuestionTypeID = questionType.QuestionTypeID,
                DifficultyID = difficulty.DifficultyID,
                WordID = word.WordID
            });
        }

        await context.Questions.AddRangeAsync(questions);
        await context.SaveChangesAsync();
    }

    // ============================================================
    // QUESTIONALTERNATIVES
    // ============================================================

    private static async Task SeedQuestionAlternativesAsync(
        VibeCheckDbContext context)
    {
        if (await context.QuestionAlternatives.AnyAsync())
            return;

        // Load question alternatives from JSON

        var jsonPath = Path.Combine(
            AppContext.BaseDirectory,
            "Seed",
            "questionalternatives.json");

        var json = await File.ReadAllTextAsync(jsonPath);

        var seedAlternatives =
            JsonSerializer.Deserialize<List<QuestionAlternativeSeed>>(json)
            ?? throw new Exception(
                "Failed to deserialize questionalternatives.json.");

        // Load questions from JSON for validation

        var questionsJsonPath = Path.Combine(
            AppContext.BaseDirectory,
            "Seed",
            "questions.json");

        var questionsJson =
            await File.ReadAllTextAsync(questionsJsonPath);

        var seedQuestions =
            JsonSerializer.Deserialize<List<QuestionSeed>>(questionsJson)
            ?? throw new Exception(
                "Failed to deserialize questions.json.");

        // Validate the JSON seed data before inserting anything
        SeedValidator.ValidateQuestions(
            seedQuestions,
            seedAlternatives);

        // Get questions from database

        var questions = await context.Questions
            .ToDictionaryAsync(q => q.QuestionDesc);

        var alternatives = new List<QuestionAlternative>();

        // Keep track of the actual QuestionAlternative object
        // marked as correct for each question.
        var correctAlternatives =
            new Dictionary<int, QuestionAlternative>();

        // Create QuestionAlternative entities

        foreach (var seed in seedAlternatives)
        {
            if (!questions.TryGetValue(
                    seed.Question,
                    out var question))
            {
                throw new Exception(
                    $"Question '{seed.Question}' referenced in questionalternatives.json was not found.");
            }

            var alternative = new QuestionAlternative
            {
                QuestionID = question.QuestionID,
                AlternativeText = seed.AlternativeText
            };

            alternatives.Add(alternative);

            if (seed.IsCorrect)
            {
                if (correctAlternatives.ContainsKey(question.QuestionID))
                {
                    throw new Exception(
                        $"Question '{seed.Question}' has more than one correct alternative.");
                }

                correctAlternatives[question.QuestionID] = alternative;
            }
        }

        // Make sure every question has exactly one correct alternative

        foreach (var question in questions.Values)
        {
            if (!correctAlternatives.ContainsKey(question.QuestionID))
            {
                throw new Exception(
                    $"Question '{question.QuestionDesc}' has no correct alternative.");
            }
        }

        await context.QuestionAlternatives.AddRangeAsync(alternatives);
        await context.SaveChangesAsync();

        // The QuestionAlternative objects in correctAlternatives
        // now contain their database-generated AlternativeIDs.
        foreach (var question in questions.Values)
        {
            var correctAlternative =
                correctAlternatives[question.QuestionID];

            question.CorrectAlternativeID =
                correctAlternative.AlternativeID;
        }

        // Save the generated CorrectAlternativeID values
        await context.SaveChangesAsync();
    }


    // ============================================================
    // QUIZZES
    // ============================================================

    private static async Task SeedQuizzesAsync(
    VibeCheckDbContext context)
    {
        if (await context.Quizzes.AnyAsync())
            return;

        var difficulties = await context.Difficulties
            .ToDictionaryAsync(d => d.DifficultyDesc);

        var quizzes = new List<Quiz>
    {
        new()
        {
            QuizName = "Slang för nybörjare",
            QuizDescription =
                "Testa dina kunskaper om de vanligaste slangorden.",
            DifficultyID =
                difficulties["Easy"].DifficultyID
        },

        new()
        {
            QuizName = "Slangutmaningen",
            QuizDescription =
                "Sätt dina kunskaper om modern slang på prov",
            DifficultyID =
                difficulties["Medium"].DifficultyID
        },

        new()
        {
            QuizName = "Avancerad slang",
            QuizDescription =
                "Ett tuffare quiz för dig som redan kan ditt lingo.",
            DifficultyID =
                difficulties["Hard"].DifficultyID
        }
    };

        await context.Quizzes.AddRangeAsync(quizzes);
        await context.SaveChangesAsync();
    }


    // ============================================================
    // QUIZQUESTIONS
    // ============================================================

    private static async Task SeedQuizQuestionsAsync(
    VibeCheckDbContext context)
    {
        if (await context.QuizQuestions.AnyAsync())
            return;

        var quizzes = await context.Quizzes
            .ToDictionaryAsync(q => q.QuizName);

        var questions = await context.Questions
            .ToDictionaryAsync(q => q.QuestionID);

        var quizQuestions = new List<QuizQuestion>
    {
        // ============================================================
        // Slang för nybörjare - Easy
        // Questions 1-10
        // ============================================================

        new()
        {
            QuizID = quizzes["Slang för nybörjare"].QuizID,
            QuestionID = questions[1].QuestionID
        },

        new()
        {
            QuizID = quizzes["Slang för nybörjare"].QuizID,
            QuestionID = questions[2].QuestionID
        },

        new()
        {
            QuizID = quizzes["Slang för nybörjare"].QuizID,
            QuestionID = questions[3].QuestionID
        },

        new()
        {
            QuizID = quizzes["Slang för nybörjare"].QuizID,
            QuestionID = questions[4].QuestionID
        },

        new()
        {
            QuizID = quizzes["Slang för nybörjare"].QuizID,
            QuestionID = questions[5].QuestionID
        },

        new()
        {
            QuizID = quizzes["Slang för nybörjare"].QuizID,
            QuestionID = questions[6].QuestionID
        },

        new()
        {
            QuizID = quizzes["Slang för nybörjare"].QuizID,
            QuestionID = questions[7].QuestionID
        },

        new()
        {
            QuizID = quizzes["Slang för nybörjare"].QuizID,
            QuestionID = questions[8].QuestionID
        },

        new()
        {
            QuizID = quizzes["Slang för nybörjare"].QuizID,
            QuestionID = questions[9].QuestionID
        },

        new()
        {
            QuizID = quizzes["Slang för nybörjare"].QuizID,
            QuestionID = questions[10].QuestionID
        },


        // ============================================================
        // Slangutmaningen - Medium
        // Questions 11-20
        // ============================================================

        new()
        {
            QuizID = quizzes["Slangutmaningen"].QuizID,
            QuestionID = questions[11].QuestionID
        },

        new()
        {
            QuizID = quizzes["Slangutmaningen"].QuizID,
            QuestionID = questions[12].QuestionID
        },

        new()
        {
            QuizID = quizzes["Slangutmaningen"].QuizID,
            QuestionID = questions[13].QuestionID
        },

        new()
        {
            QuizID = quizzes["Slangutmaningen"].QuizID,
            QuestionID = questions[14].QuestionID
        },

        new()
        {
            QuizID = quizzes["Slangutmaningen"].QuizID,
            QuestionID = questions[15].QuestionID
        },

        new()
        {
            QuizID = quizzes["Slangutmaningen"].QuizID,
            QuestionID = questions[16].QuestionID
        },

        new()
        {
            QuizID = quizzes["Slangutmaningen"].QuizID,
            QuestionID = questions[17].QuestionID
        },

        new()
        {
            QuizID = quizzes["Slangutmaningen"].QuizID,
            QuestionID = questions[18].QuestionID
        },

        new()
        {
            QuizID = quizzes["Slangutmaningen"].QuizID,
            QuestionID = questions[19].QuestionID
        },

        new()
        {
            QuizID = quizzes["Slangutmaningen"].QuizID,
            QuestionID = questions[20].QuestionID
        },


        // ============================================================
        // Avancerad slang - Hard
        // Questions 21-25
        // ============================================================

        new()
        {
            QuizID = quizzes["Avancerad slang"].QuizID,
            QuestionID = questions[21].QuestionID
        },

        new()
        {
            QuizID = quizzes["Avancerad slang"].QuizID,
            QuestionID = questions[22].QuestionID
        },

        new()
        {
            QuizID = quizzes["Avancerad slang"].QuizID,
            QuestionID = questions[23].QuestionID
        },

        new()
        {
            QuizID = quizzes["Avancerad slang"].QuizID,
            QuestionID = questions[24].QuestionID
        },

        new()
        {
            QuizID = quizzes["Avancerad slang"].QuizID,
            QuestionID = questions[25].QuestionID
        }
    };

        await context.QuizQuestions.AddRangeAsync(quizQuestions);
        await context.SaveChangesAsync();
    }

    // ============================================================
    // QUIZATTEMPTS
    // ============================================================

    private static async Task SeedQuizAttemptsAsync(
     VibeCheckDbContext context,
     UserManager<User> userManager)
    {
        if (await context.QuizAttempts.AnyAsync())
            return;

        var user = await userManager.FindByNameAsync("user");

        if (user == null)
            throw new Exception("Seed user 'user' was not found.");

        var quizzes = await context.Quizzes
            .ToDictionaryAsync(q => q.QuizName);

        var attempts = new List<QuizAttempt>
    {
        // ============================================================
        // Slang för nybörjare
        // ============================================================

        new()
        {
            UserID = user.Id,
            QuizID = quizzes["Slang för nybörjare"].QuizID,
            Score = 100,
            QuizPassed = true,
            AttemptDate = DateTime.UtcNow.AddDays(-2),
            CompletedAt = DateTime.UtcNow.AddDays(-2).AddMinutes(5)
        },

        // ============================================================
        // Slangutmaningen
        // ============================================================

        new()
        {
            UserID = user.Id,
            QuizID = quizzes["Slangutmaningen"].QuizID,
            Score = 67,
            QuizPassed = false,
            AttemptDate = DateTime.UtcNow.AddDays(-1),
            CompletedAt = DateTime.UtcNow.AddDays(-1).AddMinutes(8)
        }
    };

        await context.QuizAttempts.AddRangeAsync(attempts);
        await context.SaveChangesAsync();
    }

    // ============================================================
    // QUIZATTEMPTSANSWERS
    // ============================================================

    private static async Task SeedQuizAttemptAnswersAsync(
    VibeCheckDbContext context)
    {
        if (await context.QuizAttemptAnswers.AnyAsync())
            return;

        var attempts = await context.QuizAttempts
            .Include(a => a.Quiz)
            .ToListAsync();

        var questions = await context.Questions
            .ToDictionaryAsync(q => q.QuestionID);

        var alternatives = await context.QuestionAlternatives
            .ToListAsync();

        // ============================================================
        // Helper methods
        // ============================================================

        QuestionAlternative GetCorrectAlternative(int questionId)
        {
            var question = questions[questionId];

            var alternative = alternatives.FirstOrDefault(a =>
                a.AlternativeID == question.CorrectAlternativeID);

            if (alternative == null)
            {
                throw new Exception(
                    $"Could not find correct alternative for question {questionId}.");
            }

            return alternative;
        }

        QuestionAlternative GetWrongAlternative(int questionId)
        {
            var question = questions[questionId];

            var alternative = alternatives.FirstOrDefault(a =>
                a.QuestionID == questionId &&
                a.AlternativeID != question.CorrectAlternativeID);

            if (alternative == null)
            {
                throw new Exception(
                    $"Could not find wrong alternative for question {questionId}.");
            }

            return alternative;
        }

        var basicAttempt = attempts.First(a =>
            a.Quiz.QuizName == "Slang för nybörjare");

        var challengeAttempt = attempts.First(a =>
            a.Quiz.QuizName == "Slangutmaningen");

        var answers = new List<QuizAttemptAnswer>
    {
        // ============================================================
        // Slang för nybörjare - Attempt
        // ============================================================

        // Q1 - Correct
        new()
        {
            QuizAttemptID = basicAttempt.QuizAttemptID,
            QuestionID = questions[1].QuestionID,
            SelectedAlternativeID =
                GetCorrectAlternative(1).AlternativeID,
            IsCorrect = true
        },

        // Q2 - Correct
        new()
        {
            QuizAttemptID = basicAttempt.QuizAttemptID,
            QuestionID = questions[2].QuestionID,
            SelectedAlternativeID =
                GetCorrectAlternative(2).AlternativeID,
            IsCorrect = true
        },

        // ============================================================
        // Slangutmaningen - Attempt
        // ============================================================

        // Q11 - Correct
        new()
        {
            QuizAttemptID = challengeAttempt.QuizAttemptID,
            QuestionID = questions[11].QuestionID,
            SelectedAlternativeID =
                GetCorrectAlternative(11).AlternativeID,
            IsCorrect = true
        },

        // Q12 - Correct
        new()
        {
            QuizAttemptID = challengeAttempt.QuizAttemptID,
            QuestionID = questions[12].QuestionID,
            SelectedAlternativeID =
                GetCorrectAlternative(12).AlternativeID,
            IsCorrect = true
        },

        // Q13 - Wrong
        new()
        {
            QuizAttemptID = challengeAttempt.QuizAttemptID,
            QuestionID = questions[13].QuestionID,
            SelectedAlternativeID =
                GetWrongAlternative(13).AlternativeID,
            IsCorrect = false
        }
    };

        await context.QuizAttemptAnswers.AddRangeAsync(answers);
        await context.SaveChangesAsync();
    }
}