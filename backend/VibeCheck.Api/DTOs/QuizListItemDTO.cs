namespace VibeCheck.Api.DTOs;

// Ett quiz så som det visas på /quiz-sidan.
public class QuizListItemDTO
{
    public int QuizId { get; set; }

    public string QuizName { get; set; } = string.Empty;

    public string QuizDescription { get; set; } = string.Empty;

    // "Easy", "Medium" eller "Hard".
    public string Difficulty { get; set; } = string.Empty;

    public int QuestionCount { get; set; }

    // Högsta resultatet användaren nått, i procent.
    // null betyder att quizet aldrig slutförts – inte att resultatet var 0.
    // Därför int? och inte int: "aldrig försökt" och "fick 0%" är olika saker.
    public int? BestScore { get; set; }

    public bool IsUnlocked { get; set; }

    // Namnet på quizet som måste klaras först. null för det första quizet.
    // Skickas med så att frontend kan skriva "Klara Internet Slang Basics
    // för att låsa upp" utan att själv veta i vilken ordning quizen ligger.
    public string? UnlockedBy { get; set; }

    // Gränsen i procent. Ligger här i stället för hårdkodat i React,
    // så att regeln bara finns på ett ställe.
    public int RequiredScore { get; set; }
}