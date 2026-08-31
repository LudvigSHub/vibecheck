using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VibeCheck.Data.Models;

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
        await SetCorrectAlternativesAsync(context);
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

    private static async Task SeedUsersAsync(
        UserManager<User> userManager)
    {
        // Admin user
        if (await userManager.FindByNameAsync("admin") == null)
        {
            var admin = new User
            {
                UserName = "admin",
                Email = "admin@vibecheck.local"
            };

            var result = await userManager.CreateAsync(
                admin,
                "Admin123!");

            if (!result.Succeeded)
            {
                throw new Exception(
                    "Failed to create admin user: " +
                    string.Join(
                        ", ",
                        result.Errors.Select(e => e.Description)));
            }

            await userManager.AddToRoleAsync(admin, "Admin");
        }

        // Normal user
        if (await userManager.FindByNameAsync("user") == null)
        {
            var user = new User
            {
                UserName = "user",
                Email = "user@vibecheck.local",
            };

            var result = await userManager.CreateAsync(
                user,
                "User123!");

            if (!result.Succeeded)
            {
                throw new Exception(
                    "Failed to create normal user: " +
                    string.Join(
                        ", ",
                        result.Errors.Select(e => e.Description)));
            }
            await userManager.AddToRoleAsync(user, "User");
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

        var meanings = new List<Meaning>
        {
            new()
            {
                MeaningText = "Very good or impressive"
            },

            new()
            {
                MeaningText = "Annoying or frustrating"
            },

            new()
            {
                MeaningText = "Suspicious or questionable"
            },

            new()
            {
                MeaningText = "Something embarrassing"
            },

            new()
            {
                MeaningText = "Something extremely funny"
            }
        };

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

        // Get meanings by their text so we don't rely
        // on hardcoded MeaningID values.
        var meanings = await context.Meanings
            .ToDictionaryAsync(m => m.MeaningText);

        var words = new List<Word>
        {
            new()
            {
                WordDesc = "fire",
                MeaningID =
                    meanings["Very good or impressive"].MeaningID
            },

            new()
            {
                WordDesc = "lit",
                MeaningID =
                    meanings["Very good or impressive"].MeaningID
            },

            new()
            {
                WordDesc = "cringe",
                MeaningID =
                    meanings["Something embarrassing"].MeaningID
            },

            new()
            {
                WordDesc = "embarrassing",
                MeaningID =
                    meanings["Something embarrassing"].MeaningID
            },

            new()
            {
                WordDesc = "sus",
                MeaningID =
                    meanings["Suspicious or questionable"].MeaningID
            },

            new()
            {
                WordDesc = "sketchy",
                MeaningID =
                    meanings["Suspicious or questionable"].MeaningID
            },

            new()
            {
                WordDesc = "annoying",
                MeaningID =
                    meanings["Annoying or frustrating"].MeaningID
            },

            new()
            {
                WordDesc = "hilarious",
                MeaningID =
                    meanings["Something extremely funny"].MeaningID
            }
        };



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

        var words = await context.Words
            .ToDictionaryAsync(w => w.WordDesc);

        var examples = new List<WordExample>
    {
        new()
        {
            WordID = words["fire"].WordID,
            ExampleText = "That new song is fire."
        },

        new()
        {
            WordID = words["fire"].WordID,
            ExampleText = "Your new outfit is absolutely fire."
        },

        new()
        {
            WordID = words["lit"].WordID,
            ExampleText = "The party was lit last night."
        },

        new()
        {
            WordID = words["cringe"].WordID,
            ExampleText = "That video was so cringe."
        },

        new()
        {
            WordID = words["sus"].WordID,
            ExampleText = "That guy is acting kinda sus."
        },

        new()
        {
            WordID = words["sus"].WordID,
            ExampleText = "This website looks pretty sus."
        },

        new()
        {
            WordID = words["hilarious"].WordID,
            ExampleText = "That was absolutely hilarious."
        }
    };

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

        var tags = new List<Tag>
    {
        new()
        {
            TagName = "positive"
        },

        new()
        {
            TagName = "negative"
        },

        new()
        {
            TagName = "popular"
        },

        new()
        {
            TagName = "funny"
        },

        new()
        {
            TagName = "suspicious"
        }
    };

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

        var words = await context.Words
            .ToDictionaryAsync(w => w.WordDesc);

        var tags = await context.Tags
            .ToDictionaryAsync(t => t.TagName);

        var wordTags = new List<WordTag>
    {
        // fire
        new()
        {
            WordID = words["fire"].WordID,
            TagID = tags["positive"].TagID
        },

        new()
        {
            WordID = words["fire"].WordID,
            TagID = tags["popular"].TagID
        },

        // lit
        new()
        {
            WordID = words["lit"].WordID,
            TagID = tags["positive"].TagID
        },

        new()
        {
            WordID = words["lit"].WordID,
            TagID = tags["popular"].TagID
        },

        // cringe
        new()
        {
            WordID = words["cringe"].WordID,
            TagID = tags["negative"].TagID
        },

        new()
        {
            WordID = words["cringe"].WordID,
            TagID = tags["funny"].TagID
        },

        // sus
        new()
        {
            WordID = words["sus"].WordID,
            TagID = tags["suspicious"].TagID
        },

        new()
        {
            WordID = words["sus"].WordID,
            TagID = tags["negative"].TagID
        },

        // sketchy
        new()
        {
            WordID = words["sketchy"].WordID,
            TagID = tags["suspicious"].TagID
        },

        new()
        {
            WordID = words["sketchy"].WordID,
            TagID = tags["negative"].TagID
        },

        // hilarious
        new()
        {
            WordID = words["hilarious"].WordID,
            TagID = tags["funny"].TagID
        }
    };

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
            WordID = words["lit"].WordID,
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
            TypeText = "Multiple Choice"
        },

        new()
        {
            TypeText = "True or False"
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

        var questionTypes = await context.QuestionTypes
            .ToDictionaryAsync(qt => qt.TypeText);

        var difficulties = await context.Difficulties
            .ToDictionaryAsync(d => d.DifficultyDesc);

        var questions = new List<Question>
    {
        new()
        {
            QuestionDesc = "What does the slang term 'fire' usually mean?",
            QuestionTypeID =
                questionTypes["Multiple Choice"].QuestionTypeID,
            DifficultyID =
                difficulties["Easy"].DifficultyID
        },

        new()
        {
            QuestionDesc = "Is 'sus' commonly used to describe something suspicious?",
            QuestionTypeID =
                questionTypes["True or False"].QuestionTypeID,
            DifficultyID =
                difficulties["Easy"].DifficultyID
        },

        new()
        {
            QuestionDesc = "Which slang term is associated with something embarrassing?",
            QuestionTypeID =
                questionTypes["Multiple Choice"].QuestionTypeID,
            DifficultyID =
                difficulties["Medium"].DifficultyID
        },

        new()
        {
            QuestionDesc = "Which slang term can describe something suspicious or questionable?",
            QuestionTypeID =
                questionTypes["Multiple Choice"].QuestionTypeID,
            DifficultyID =
                difficulties["Medium"].DifficultyID
        },

        new()
        {
            QuestionDesc = "Which slang term would best describe an extremely funny situation?",
            QuestionTypeID =
                questionTypes["Multiple Choice"].QuestionTypeID,
            DifficultyID =
                difficulties["Hard"].DifficultyID
        }
    };

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

        var questions = await context.Questions
            .ToDictionaryAsync(q => q.QuestionDesc);

        var alternatives = new List<QuestionAlternative>
    {
        // Question 1
        new()
        {
            QuestionID = questions[
                "What does the slang term 'fire' usually mean?"
            ].QuestionID,
            AlternativeText = "Amazing or very good"
        },
        new()
        {
            QuestionID = questions[
                "What does the slang term 'fire' usually mean?"
            ].QuestionID,
            AlternativeText = "Something dangerous"
        },
        new()
        {
            QuestionID = questions[
                "What does the slang term 'fire' usually mean?"
            ].QuestionID,
            AlternativeText = "Something old-fashioned"
        },
        new()
        {
            QuestionID = questions[
                "What does the slang term 'fire' usually mean?"
            ].QuestionID,
            AlternativeText = "Something boring"
        },

        // Question 2 - True / False
        new()
        {
            QuestionID = questions[
                "Is 'sus' commonly used to describe something suspicious?"
            ].QuestionID,
            AlternativeText = "True"
        },
        new()
        {
            QuestionID = questions[
                "Is 'sus' commonly used to describe something suspicious?"
            ].QuestionID,
            AlternativeText = "False"
        },

        // Question 3
        new()
        {
            QuestionID = questions[
                "Which slang term is associated with something embarrassing?"
            ].QuestionID,
            AlternativeText = "Cringe"
        },
        new()
        {
            QuestionID = questions[
                "Which slang term is associated with something embarrassing?"
            ].QuestionID,
            AlternativeText = "Fire"
        },
        new()
        {
            QuestionID = questions[
                "Which slang term is associated with something embarrassing?"
            ].QuestionID,
            AlternativeText = "Based"
        },
        new()
        {
            QuestionID = questions[
                "Which slang term is associated with something embarrassing?"
            ].QuestionID,
            AlternativeText = "W"
        },

        // Question 4
        new()
        {
            QuestionID = questions[
                "Which slang term can describe something suspicious or questionable?"
            ].QuestionID,
            AlternativeText = "Sus"
        },
        new()
        {
            QuestionID = questions[
                "Which slang term can describe something suspicious or questionable?"
            ].QuestionID,
            AlternativeText = "Fire"
        },
        new()
        {
            QuestionID = questions[
                "Which slang term can describe something suspicious or questionable?"
            ].QuestionID,
            AlternativeText = "Based"
        },
        new()
        {
            QuestionID = questions[
                "Which slang term can describe something suspicious or questionable?"
            ].QuestionID,
            AlternativeText = "W"
        },

        // Question 5
        new()
        {
            QuestionID = questions[
                "Which slang term would best describe an extremely funny situation?"
            ].QuestionID,
            AlternativeText = "Hilarious"
        },
        new()
        {
            QuestionID = questions[
                "Which slang term would best describe an extremely funny situation?"
            ].QuestionID,
            AlternativeText = "Cringe"
        },
        new()
        {
            QuestionID = questions[
                "Which slang term would best describe an extremely funny situation?"
            ].QuestionID,
            AlternativeText = "Sus"
        },
        new()
        {
            QuestionID = questions[
                "Which slang term would best describe an extremely funny situation?"
            ].QuestionID,
            AlternativeText = "L"
        }
    };

        await context.QuestionAlternatives.AddRangeAsync(alternatives);
        await context.SaveChangesAsync();
    }

    private static async Task SetCorrectAlternativesAsync(
    VibeCheckDbContext context)
    {
        var questions = await context.Questions
            .ToDictionaryAsync(q => q.QuestionDesc);

        var alternatives = await context.QuestionAlternatives
            .ToListAsync();

        SetCorrectAnswer(
            questions["What does the slang term 'fire' usually mean?"],
            "Amazing or very good",
            alternatives);

        SetCorrectAnswer(
            questions["Is 'sus' commonly used to describe something suspicious?"],
            "True",
            alternatives);

        SetCorrectAnswer(
            questions["Which slang term is associated with something embarrassing?"],
            "Cringe",
            alternatives);

        SetCorrectAnswer(
            questions["Which slang term can describe something suspicious or questionable?"],
            "Sus",
            alternatives);

        SetCorrectAnswer(
            questions["Which slang term would best describe an extremely funny situation?"],
            "Hilarious",
            alternatives);

        await context.SaveChangesAsync();
    }

    private static void SetCorrectAnswer(
        Question question,
        string correctAnswer,
        List<QuestionAlternative> alternatives)
    {
        question.CorrectAlternativeID = alternatives
            .Single(a =>
                a.QuestionID == question.QuestionID &&
                a.AlternativeText == correctAnswer)
            .AlternativeID;
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
            QuizName = "Internet Slang Basics",
            QuizDescription =
                "Test your knowledge of common internet slang.",
            DifficultyID =
                difficulties["Easy"].DifficultyID
        },

        new()
        {
            QuizName = "Internet Slang Challenge",
            QuizDescription =
                "Put your knowledge of modern slang to the test.",
            DifficultyID =
                difficulties["Medium"].DifficultyID
        },

        new()
        {
            QuizName = "Advanced Slang Knowledge",
            QuizDescription =
                "A challenging quiz for experienced slang users.",
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
        // Internet Slang Basics - Easy
        // ============================================================

        new()
        {
            QuizID = quizzes["Internet Slang Basics"].QuizID,
            QuestionID = questions[1].QuestionID
        },

        new()
        {
            QuizID = quizzes["Internet Slang Basics"].QuizID,
            QuestionID = questions[2].QuestionID
        },


        // ============================================================
        // Internet Slang Challenge - Medium
        // ============================================================

        new()
        {
            QuizID = quizzes["Internet Slang Challenge"].QuizID,
            QuestionID = questions[2].QuestionID
        },

        new()
        {
            QuizID = quizzes["Internet Slang Challenge"].QuizID,
            QuestionID = questions[3].QuestionID
        },

        new()
        {
            QuizID = quizzes["Internet Slang Challenge"].QuizID,
            QuestionID = questions[4].QuestionID
        },


        // ============================================================
        // Advanced Slang Knowledge - Hard
        // ============================================================

        new()
        {
            QuizID = quizzes["Advanced Slang Knowledge"].QuizID,
            QuestionID = questions[3].QuestionID
        },

        new()
        {
            QuizID = quizzes["Advanced Slang Knowledge"].QuizID,
            QuestionID = questions[4].QuestionID
        },

        new()
        {
            QuizID = quizzes["Advanced Slang Knowledge"].QuizID,
            QuestionID = questions[5].QuestionID
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
        new()
        {
            UserID = user.Id,
            QuizID = quizzes["Internet Slang Basics"].QuizID,
            Score = 100,
            QuizPassed = true,
            AttemptDate = DateTime.UtcNow.AddDays(-2),
            CompletedAt = DateTime.UtcNow.AddDays(-2).AddMinutes(5)
        },

        new()
        {
            UserID = user.Id,
            QuizID = quizzes["Internet Slang Challenge"].QuizID,
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

        QuestionAlternative GetAlternative(
            int questionId,
            string alternativeText)
        {
            var alternative = alternatives.FirstOrDefault(a =>
                a.QuestionID == questionId &&
                a.AlternativeText == alternativeText);

            if (alternative == null)
            {
                throw new Exception(
                    $"Could not find alternative '{alternativeText}' " +
                    $"for question {questionId}.");
            }

            return alternative;
        }

        var basicAttempt = attempts.First(a =>
            a.Quiz.QuizName == "Internet Slang Basics");

        var challengeAttempt = attempts.First(a =>
            a.Quiz.QuizName == "Internet Slang Challenge");

        var answers = new List<QuizAttemptAnswer>
    {
        // ============================================================
        // Internet Slang Basics - Attempt
        // ============================================================

        // Q1 - Correct
        new()
        {
            QuizAttemptID = basicAttempt.QuizAttemptID,
            QuestionID = questions[1].QuestionID,
            SelectedAlternativeID =
                GetAlternative(1, "Amazing or very good").AlternativeID,
            IsCorrect = true
        },

        // Q2 - Correct
        new()
        {
            QuizAttemptID = basicAttempt.QuizAttemptID,
            QuestionID = questions[2].QuestionID,
            SelectedAlternativeID =
                GetAlternative(2, "True").AlternativeID,
            IsCorrect = true
        },


        // ============================================================
        // Internet Slang Challenge - Attempt
        // ============================================================

        // Q2 - Correct
        new()
        {
            QuizAttemptID = challengeAttempt.QuizAttemptID,
            QuestionID = questions[2].QuestionID,
            SelectedAlternativeID =
                GetAlternative(2, "True").AlternativeID,
            IsCorrect = true
        },

        // Q3 - Correct
        new()
        {
            QuizAttemptID = challengeAttempt.QuizAttemptID,
            QuestionID = questions[3].QuestionID,
            SelectedAlternativeID =
                GetAlternative(3, "Cringe").AlternativeID,
            IsCorrect = true
        },

        // Q4 - Wrong
        new()
        {
            QuizAttemptID = challengeAttempt.QuizAttemptID,
            QuestionID = questions[4].QuestionID,
            SelectedAlternativeID =
                GetAlternative(4, "Fire").AlternativeID,
            IsCorrect = false
        }
    };

        await context.QuizAttemptAnswers.AddRangeAsync(answers);
        await context.SaveChangesAsync();
    }
}