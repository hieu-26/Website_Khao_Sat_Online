using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SurveyWebsite.Models;
using SurveyWebsite.Models.ViewModels;
using System.Security.Claims;

namespace SurveyWebsite.Controllers
{
    [Authorize]
    public class SurveyController : Controller
    {
        private readonly SurveyDbContext _context;

        public SurveyController(SurveyDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        public IActionResult Create()
        {
            var model = new SurveyCreateViewModel
            {
                AllUsers = _context.Users.ToList(),
                Questions = new List<SurveyCreateViewModel.QuestionViewModel>()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SurveyCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AllUsers = _context.Users.ToList();
                return View(model);
            }

            var userId = GetCurrentUserId();

            var survey = new Survey
            {
                Title = model.Title,
                Description = model.Description,
                IsPublic = model.IsPublic,
                CreatedDate = DateTime.UtcNow,
                CreatorUserId = userId,
                SurveySetting = new SurveySetting
                {
                    AllowMultipleResponses = model.AllowMultipleResponses,
                    RequireLogin = model.RequireLogin,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate
                },
                Questions = model.Questions.Select(q => new Question
                {
                    QuestionText = q.QuestionText,
                    QuestionType = q.QuestionType,
                    IsRequired = q.IsRequired,
                    Options = q.Options?.Select(o => new Option
                    {
                        OptionText = o.OptionText
                    }).ToList() ?? new List<Option>()
                }).ToList()
            };

            _context.Surveys.Add(survey);
            await _context.SaveChangesAsync();

            if (!model.IsPublic && model.AllowedUserIds != null && model.AllowedUserIds.Any())
            {
                var allowed = model.AllowedUserIds.Select(uid => new SurveyAllowedUser
                {
                    SurveyId = survey.SurveyId,
                    UserId = uid,
                    AddedDate = DateTime.UtcNow
                });
                _context.SurveyAllowedUsers.AddRange(allowed);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("MySurveys");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetCurrentUserId();
            var survey = await _context.Surveys
                .Include(s => s.Questions).ThenInclude(q => q.Options)
                .Include(s => s.SurveySetting)
                .Include(s => s.SurveyAllowedUsers)
                .FirstOrDefaultAsync(s => s.SurveyId == id && s.CreatorUserId == userId);

            if (survey == null) return NotFound();

            var model = new SurveyCreateViewModel
            {
                SurveyId = survey.SurveyId,
                Title = survey.Title,
                Description = survey.Description,
                IsPublic = survey.IsPublic,
                AllowMultipleResponses = survey.SurveySetting?.AllowMultipleResponses ?? false,
                RequireLogin = survey.SurveySetting?.RequireLogin ?? false,
                StartDate = survey.SurveySetting?.StartDate,
                EndDate = survey.SurveySetting?.EndDate,
                Questions = survey.Questions.Select(q => new SurveyCreateViewModel.QuestionViewModel
                {
                    QuestionText = q.QuestionText,
                    QuestionType = q.QuestionType,
                    IsRequired = q.IsRequired,
                    Options = q.Options.Select(o => new SurveyCreateViewModel.OptionViewModel
                    {
                        OptionText = o.OptionText
                    }).ToList()
                }).ToList(),
                AllowedUserIds = survey.SurveyAllowedUsers?.Select(a => a.UserId).ToList() ?? new(),
                AllUsers = await _context.Users.ToListAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SurveyCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AllUsers = await _context.Users.ToListAsync();
                return View(model);
            }

            var userId = GetCurrentUserId();
            var survey = await _context.Surveys
                .Include(s => s.Questions).ThenInclude(q => q.Options)
                .Include(s => s.SurveySetting)
                .Include(s => s.SurveyAllowedUsers)
                .FirstOrDefaultAsync(s => s.SurveyId == id && s.CreatorUserId == userId);

            if (survey == null) return NotFound();

            // Cập nhật thông tin chung
            survey.Title = model.Title;
            survey.Description = model.Description;
            survey.IsPublic = model.IsPublic;

            survey.SurveySetting ??= new SurveySetting();
            survey.SurveySetting.AllowMultipleResponses = model.AllowMultipleResponses;
            survey.SurveySetting.RequireLogin = model.RequireLogin;
            survey.SurveySetting.StartDate = model.StartDate;
            survey.SurveySetting.EndDate = model.EndDate;

            // Xóa câu hỏi và option cũ
            _context.Options.RemoveRange(survey.Questions.SelectMany(q => q.Options));
            _context.Questions.RemoveRange(survey.Questions);

            // Thêm câu hỏi mới
            survey.Questions = model.Questions.Select(q => new Question
            {
                QuestionText = q.QuestionText,
                QuestionType = q.QuestionType,
                IsRequired = q.IsRequired,
                Options = q.Options?.Select(o => new Option
                {
                    OptionText = o.OptionText
                }).ToList() ?? new List<Option>()
            }).ToList();

            // Cập nhật danh sách user được phép tham gia
            _context.SurveyAllowedUsers.RemoveRange(
                _context.SurveyAllowedUsers.Where(x => x.SurveyId == survey.SurveyId)
            );

            if (!model.IsPublic && model.AllowedUserIds != null && model.AllowedUserIds.Any())
            {
                _context.SurveyAllowedUsers.AddRange(model.AllowedUserIds.Select(uid => new SurveyAllowedUser
                {
                    SurveyId = survey.SurveyId,
                    UserId = uid,
                    AddedDate = DateTime.UtcNow
                }));
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("MySurveys");
        }

        public async Task<IActionResult> MySurveys()
        {
            var userId = GetCurrentUserId();
            var surveys = await _context.Surveys
                .Where(s => s.CreatorUserId == userId)
                .ToListAsync();

            return View(surveys);
        }

        public async Task<IActionResult> Results(int id)
        {
            var userId = GetCurrentUserId();
            var survey = await _context.Surveys
                .Include(s => s.Questions).ThenInclude(q => q.Answers)
                .Include(s => s.Participations)
                .FirstOrDefaultAsync(s => s.SurveyId == id && s.CreatorUserId == userId);

            if (survey == null) return NotFound();

            return View(survey);
        }
    }
}
