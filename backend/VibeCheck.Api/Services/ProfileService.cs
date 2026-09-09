using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VibeCheck.Api.DTOs;
using VibeCheck.Data.Data;
using VibeCheck.Data.Models;

namespace VibeCheck.Api.Services
{
    public class ProfileService
    {
        private readonly VibeCheckDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly HomeService _homeService;

        public ProfileService(
            VibeCheckDbContext context,
            UserManager<User> userManager,
            HomeService homeService)
        {
            _context = context;
            _userManager = userManager;
            _homeService = homeService;
        }

        public async Task<ProfileDTO?> GetProfileAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                return null;
            }

            var summary = await _homeService.GetSummaryAsync(userId);

            var quizHistory = await _context.QuizAttempts
                .AsNoTracking()
                .Where(attempt =>
                    attempt.UserID == userId &&
                    attempt.CompletedAt != null)
                .OrderByDescending(attempt => attempt.CompletedAt)
                .Select(attempt => new ProfileQuizHistoryItemDTO
                {
                    QuizName = attempt.Quiz.QuizName,
                    Topic = attempt.Quiz.Difficulty.DifficultyDesc,
                    Score = attempt.Score ?? 0,
                    CompletedAt = attempt.CompletedAt!.Value
                })
                .ToListAsync();

            return new ProfileDTO
            {
                UserName = user.UserName ?? "",
                BestScore = summary.BestScore,
                CompletedQuizCount = summary.CompletedQuizCount,
                CurrentStreak = summary.CurrentStreak,
                QuizHistory = quizHistory
            };
        }

        public async Task<IdentityResult> UpdateUserNameAsync(int userId, string newUserName)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return IdentityResult.Failed(
                    new IdentityError { Description = "Användaren hittades inte." });
            }

            if (string.IsNullOrWhiteSpace(newUserName))
            {
                return IdentityResult.Failed(
                    new IdentityError { Description = "Användarnamnet kan inte vara tomt." });
            }

            var existingUser = await _userManager.FindByNameAsync(newUserName);

            if (existingUser != null && existingUser.Id != user.Id)
            {
                return IdentityResult.Failed(
                    new IdentityError { Description = "Användarnamnet är redan taget." });
            }

            return await _userManager.SetUserNameAsync(user, newUserName);

        }

        public async Task<IdentityResult> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null) 
            {
                return IdentityResult.Failed(
                    new IdentityError { Description = "Användaren hittades inte." });
            }

            return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        }
    }
}
