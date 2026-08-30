using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VibeCheck.Data.Models;

namespace VibeCheck.Data.Data;

public class VibeCheckDbContext
    : IdentityDbContext<User, IdentityRole<int>, int>
{
    public VibeCheckDbContext(
        DbContextOptions<VibeCheckDbContext> options)
        : base(options)
    {
    }

    // Word system
    public DbSet<Word> Words => Set<Word>();
    public DbSet<Meaning> Meanings => Set<Meaning>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<WordExample> WordExamples => Set<WordExample>();
    public DbSet<WordVote> WordVotes => Set<WordVote>();
    public DbSet<WordTag> WordTags => Set<WordTag>();

    // Quiz system
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<QuestionAlternative> QuestionAlternatives => Set<QuestionAlternative>();
    public DbSet<QuestionType> QuestionTypes => Set<QuestionType>();
    public DbSet<Difficulty> Difficulties => Set<Difficulty>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();
    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
    public DbSet<QuizAttemptAnswer> QuizAttemptAnswers => Set<QuizAttemptAnswer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ============================================================
        // WORD SYSTEM
        // ============================================================

        // Word -> Meaning
        modelBuilder.Entity<Word>()
            .HasOne(w => w.Meaning)
            .WithMany(m => m.Words)
            .HasForeignKey(w => w.MeaningID)
            .OnDelete(DeleteBehavior.Restrict);

        // WordExample primary key
        modelBuilder.Entity<WordExample>()
            .HasKey(e => e.ExampleID);

        // WordExample -> Word
        modelBuilder.Entity<WordExample>()
            .HasOne(e => e.Word)
            .WithMany(w => w.WordExamples)
            .HasForeignKey(e => e.WordID)
            .OnDelete(DeleteBehavior.Cascade);

        // WordVote -> Word
        modelBuilder.Entity<WordVote>()
            .HasOne(v => v.Word)
            .WithMany(w => w.WordVotes)
            .HasForeignKey(v => v.WordID)
            .OnDelete(DeleteBehavior.Cascade);

        // WordVote -> User
        modelBuilder.Entity<WordVote>()
            .HasOne(v => v.User)
            .WithMany(u => u.WordVotes)
            .HasForeignKey(v => v.UserID)
            .OnDelete(DeleteBehavior.Cascade);

        // One vote per user per word
        modelBuilder.Entity<WordVote>()
            .HasIndex(v => new { v.UserID, v.WordID })
            .IsUnique();

        // WordTag -> Word
        modelBuilder.Entity<WordTag>()
            .HasOne(wt => wt.Word)
            .WithMany(w => w.WordTags)
            .HasForeignKey(wt => wt.WordID)
            .OnDelete(DeleteBehavior.Cascade);

        // WordTag -> Tag
        modelBuilder.Entity<WordTag>()
            .HasOne(wt => wt.Tag)
            .WithMany(t => t.WordTags)
            .HasForeignKey(wt => wt.TagID)
            .OnDelete(DeleteBehavior.Cascade);

        // WordTag composite primary key
        modelBuilder.Entity<WordTag>()
            .HasKey(wt => new { wt.WordID, wt.TagID });

        // Unique fields
        modelBuilder.Entity<Word>()
            .HasIndex(w => w.WordDesc)
            .IsUnique();

        modelBuilder.Entity<Meaning>()
            .HasIndex(m => m.MeaningText)
            .IsUnique();

        modelBuilder.Entity<Tag>()
            .HasIndex(t => t.TagName)
            .IsUnique();


        // ============================================================
        // QUESTION SYSTEM
        // ============================================================

        // Question -> QuestionType
        modelBuilder.Entity<Question>()
            .HasOne(q => q.QuestionType)
            .WithMany(qt => qt.Questions)
            .HasForeignKey(q => q.QuestionTypeID)
            .OnDelete(DeleteBehavior.Restrict);

        // Question -> Difficulty
        modelBuilder.Entity<Question>()
            .HasOne(q => q.Difficulty)
            .WithMany(d => d.Questions)
            .HasForeignKey(q => q.DifficultyID)
            .OnDelete(DeleteBehavior.Restrict);

        // QuestionAlternative primary key
        modelBuilder.Entity<QuestionAlternative>()
            .HasKey(qa => qa.AlternativeID);

        // QuestionAlternative -> Question
        modelBuilder.Entity<QuestionAlternative>()
            .HasOne(qa => qa.Question)
            .WithMany(q => q.QuestionAlternatives)
            .HasForeignKey(qa => qa.QuestionID)
            .OnDelete(DeleteBehavior.Cascade);

        // QuestionAlternative -> Word
        modelBuilder.Entity<QuestionAlternative>()
            .HasOne(qa => qa.Word)
            .WithMany()
            .HasForeignKey(qa => qa.WordID)
            .OnDelete(DeleteBehavior.Restrict);

        // One AlternativeID per QuestionID
        modelBuilder.Entity<QuestionAlternative>()
            .HasIndex(qa => new { qa.QuestionID, qa.AlternativeID })
            .IsUnique();

        // QuestionType TypeText unique
        modelBuilder.Entity<QuestionType>()
            .HasIndex(qt => qt.TypeText)
            .IsUnique();

        // Difficulty DifficultyDesc unique
        modelBuilder.Entity<Difficulty>()
            .HasIndex(d => d.DifficultyDesc)
            .IsUnique();


        // ============================================================
        // QUIZ SYSTEM
        // ============================================================

        // Quiz -> Difficulty
        modelBuilder.Entity<Quiz>()
            .HasOne(q => q.Difficulty)
            .WithMany(d => d.Quizzes)
            .HasForeignKey(q => q.DifficultyID)
            .OnDelete(DeleteBehavior.Restrict);

        // QuizQuestion -> Quiz
        modelBuilder.Entity<QuizQuestion>()
            .HasOne(qq => qq.Quiz)
            .WithMany(q => q.QuizQuestions)
            .HasForeignKey(qq => qq.QuizID)
            .OnDelete(DeleteBehavior.Cascade);

        // QuizQuestion -> Question
        modelBuilder.Entity<QuizQuestion>()
            .HasOne(qq => qq.Question)
            .WithMany(q => q.QuizQuestions)
            .HasForeignKey(qq => qq.QuestionID)
            .OnDelete(DeleteBehavior.Cascade);

        // A question can only appear once in the same quiz
        modelBuilder.Entity<QuizQuestion>()
            .HasIndex(qq => new { qq.QuizID, qq.QuestionID })
            .IsUnique();

        // QuizName unique
        modelBuilder.Entity<Quiz>()
            .HasIndex(q => q.QuizName)
            .IsUnique();


        // ============================================================
        // QUIZ ATTEMPTS
        // ============================================================

        // QuizAttemptAnswer primary key
        modelBuilder.Entity<QuizAttemptAnswer>()
            .HasKey(aaa => aaa.AttemptAnswerID);

        // QuizAttempt -> User
        modelBuilder.Entity<QuizAttempt>()
            .HasOne(qa => qa.User)
            .WithMany(u => u.QuizAttempts)
            .HasForeignKey(qa => qa.UserID)
            .OnDelete(DeleteBehavior.Cascade);

        // QuizAttempt -> Quiz
        modelBuilder.Entity<QuizAttempt>()
            .HasOne(qa => qa.Quiz)
            .WithMany(q => q.QuizAttempts)
            .HasForeignKey(qa => qa.QuizID)
            .OnDelete(DeleteBehavior.Cascade);

        // QuizAttemptAnswer -> QuizAttempt
        modelBuilder.Entity<QuizAttemptAnswer>()
            .HasOne(aaa => aaa.QuizAttempt)
            .WithMany(qa => qa.QuizAttemptAnswers)
            .HasForeignKey(aaa => aaa.QuizAttemptID)
            .OnDelete(DeleteBehavior.Cascade);

        // QuizAttemptAnswer -> Question
        modelBuilder.Entity<QuizAttemptAnswer>()
            .HasOne(aaa => aaa.Question)
            .WithMany(q => q.QuizAttemptAnswers)
            .HasForeignKey(aaa => aaa.QuestionID)
            .OnDelete(DeleteBehavior.Restrict);

        // QuizAttemptAnswer -> QuestionAlternative
        modelBuilder.Entity<QuizAttemptAnswer>()
            .HasOne(aaa => aaa.SelectedAlternative)
            .WithMany(qa => qa.QuizAttemptAnswers)
            .HasForeignKey(aaa => aaa.SelectedAlternativeID)
            .OnDelete(DeleteBehavior.Restrict);

        // One answer per question in each attempt
        modelBuilder.Entity<QuizAttemptAnswer>()
            .HasIndex(aaa => new { aaa.QuizAttemptID, aaa.QuestionID })
            .IsUnique();


        // ============================================================
        // COMPOSITE FOREIGN KEYS
        // ============================================================

        // Question -> Correct QuestionAlternative
        //
        // Ensures that CorrectAlternativeID actually belongs
        // to the Question.
        modelBuilder.Entity<Question>()
            .HasOne<QuestionAlternative>()
            .WithMany()
            .HasForeignKey(q => new
            {
                q.QuestionID,
                q.CorrectAlternativeID
            })
            .HasPrincipalKey(qa => new
            {
                qa.QuestionID,
                qa.AlternativeID
            })
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // QuizAttemptAnswer -> Selected QuestionAlternative
        //
        // Ensures that the selected alternative actually belongs
        // to the question being answered.
        modelBuilder.Entity<QuizAttemptAnswer>()
            .HasOne(aaa => aaa.SelectedAlternative)
            .WithMany(qa => qa.QuizAttemptAnswers)
            .HasForeignKey(aaa => new
            {
                aaa.QuestionID,
                aaa.SelectedAlternativeID
            })
            .HasPrincipalKey(qa => new
            {
                qa.QuestionID,
                qa.AlternativeID
            })
            .OnDelete(DeleteBehavior.Restrict);
    }
}