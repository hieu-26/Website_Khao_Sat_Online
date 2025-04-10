using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SurveyWebsite.Models;
using SurveyWebsite.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;
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

            // Validate options for non-text questions
            foreach (var question in model.Questions)
            {
                if (question.QuestionType != "Text")
                {
                    // Lọc bỏ các option trống
                    question.Options = question.Options?
                        .Where(o => !string.IsNullOrWhiteSpace(o.OptionText))
                        .ToList();

                    if (question.Options == null || !question.Options.Any())
                    {
                        ModelState.AddModelError("", $"Câu hỏi '{question.QuestionText}' cần ít nhất một tùy chọn hợp lệ");
                        model.AllUsers = _context.Users.ToList();
                        return View(model);
                    }
                }
            }

            var survey = new Survey
            {
                Title = model.Title,
                Description = model.Description,
                IsPublic = model.IsPublic,
                CreatedDate = DateTime.UtcNow,
                CreatorUserId = GetCurrentUserId(),
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
                    Options = q.QuestionType == "Text"
                        ? new List<Option>()
                        : q.Options.Select(o => new Option
                        {
                            OptionText = o.OptionText?.Trim() ?? string.Empty
                        }).ToList()
                }).ToList()
            };

            _context.Surveys.Add(survey);
            await _context.SaveChangesAsync();

            // Xử lý survey không công khai
            if (!model.IsPublic && model.AllowedUserIds != null)
            {
                var allowedUsers = model.AllowedUserIds
                    .Select(uid => new SurveyAllowedUser
                    {
                        SurveyId = survey.SurveyId,
                        UserId = uid,
                        AddedDate = DateTime.UtcNow
                    });
                _context.SurveyAllowedUsers.AddRange(allowedUsers);
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
                    QuestionId = q.QuestionId == 0 ? 0 : q.QuestionId,
                    QuestionText = q.QuestionText,
                    QuestionType = q.QuestionType,
                    IsRequired = q.IsRequired,
                    Options = q.Options.Select(o => new SurveyCreateViewModel.OptionViewModel
                    {
                        OptionId = o.OptionId,
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

            var survey = await _context.Surveys
                .Include(s => s.Questions)
                    .ThenInclude(q => q.Options)
                .Include(s => s.SurveySetting)
                .Include(s => s.SurveyAllowedUsers)
                .FirstOrDefaultAsync(s => s.SurveyId == id);

            if (survey == null) return NotFound();

            // Cập nhật thông tin cơ bản
            survey.Title = model.Title;
            survey.Description = model.Description;
            survey.IsPublic = model.IsPublic;
            survey.LastModifiedDate = DateTime.UtcNow;

            // Cập nhật cài đặt
            survey.SurveySetting ??= new SurveySetting();
            survey.SurveySetting.AllowMultipleResponses = model.AllowMultipleResponses;
            survey.SurveySetting.RequireLogin = model.RequireLogin;
            survey.SurveySetting.StartDate = model.StartDate;
            survey.SurveySetting.EndDate = model.EndDate;

            // Xóa các option cũ trước
            var optionsToDelete = survey.Questions.SelectMany(q => q.Options).ToList();
            _context.Options.RemoveRange(optionsToDelete);

            // Xóa câu hỏi cũ và thêm mới
            _context.Questions.RemoveRange(survey.Questions);
            await _context.SaveChangesAsync(); // Cập nhật xóa

            survey.Questions = model.Questions.Select(q => new Question
            {
                SurveyId = survey.SurveyId,
                QuestionText = q.QuestionText,
                QuestionType = q.QuestionType,
                IsRequired = q.IsRequired,
                Options = q.QuestionType == "Text"
                    ? new List<Option>()
                    : q.Options?
                        .Where(o => !string.IsNullOrWhiteSpace(o.OptionText))
                        .Select(o => new Option
                        {
                            OptionText = o.OptionText.Trim()
                        })
                        .ToList() ?? new List<Option>()
            }).ToList();

            // Cập nhật danh sách user được phép
            _context.SurveyAllowedUsers.RemoveRange(survey.SurveyAllowedUsers);

            if (!model.IsPublic && model.AllowedUserIds != null)
            {
                survey.SurveyAllowedUsers = model.AllowedUserIds
                    .Select(uid => new SurveyAllowedUser
                    {
                        SurveyId = survey.SurveyId,
                        UserId = uid,
                        AddedDate = DateTime.UtcNow
                    }).ToList();
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitSurvey(int surveyId, IFormCollection form)
        {
            var currentUser = await GetCurrentUserAsync();

            var participation = new Participation
            {
                SurveyId = surveyId,
                UserId = currentUser?.UserId,
                SubmittedAt = DateTime.Now
            };

            _context.Participations.Add(participation);
            await _context.SaveChangesAsync(); // Lưu để có ParticipationId

            foreach (var key in form.Keys)
            {
                if (!key.StartsWith("question_")) continue;

                var questionId = int.Parse(key.Split('_')[1]);
                var values = form[key]; // hỗ trợ cả radio/text và checkbox (nhiều giá trị)

                foreach (var value in values)
                {
                    if (string.IsNullOrWhiteSpace(value)) continue; // bỏ qua câu trống

                    var answer = new Answer
                    {
                        ParticipationId = participation.ParticipationId,
                        QuestionId = questionId
                    };

                    if (int.TryParse(value, out int optionId))
                    {
                        answer.OptionId = optionId;
                    }
                    else
                    {
                        answer.AnswerText = value;
                    }

                    _context.Answers.Add(answer);
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("ThankYou");
        }

        private async Task<User?> GetCurrentUserAsync()
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                var username = User.Identity.Name;
                return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            }
            return null;
        }

        [HttpGet]
        public IActionResult ThankYou()
        {
            return View();
        }

        //PublicSurveys
        [Authorize] // Yêu cầu người dùng đăng nhập
        public async Task<IActionResult> PublicSurveys()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int currentUserId))
            {
                return RedirectToAction("Login", "Account");
            }

            var publicOrSharedSurveys = await _context.Surveys
                .Include(s => s.CreatorUser)
                .Where(s => s.IsPublic || s.SurveyAllowedUsers.Any(u => u.UserId == currentUserId))
                .ToListAsync();

            return View(publicOrSharedSurveys);
        }

        //TakeSurvey
        [HttpGet]
        public async Task<IActionResult> ViewSurvey(int id)
        {
            var survey = await _context.Surveys
                .Include(s => s.CreatorUser)
                .FirstOrDefaultAsync(s => s.SurveyId == id);

            if (survey == null)
            {
                return NotFound();
            }

            var currentUser = await GetCurrentUserAsync();

            // Nếu là người tạo khảo sát → chuyển sang trang kết quả
            if (currentUser != null && survey.CreatorUserId == currentUser.UserId)
            {
                return RedirectToAction("Results", new { id = survey.SurveyId });
            }

            // Nếu không phải người tạo → chuyển sang trang tham gia khảo sát
            return RedirectToAction("TakeSurvey", new { id = survey.SurveyId });
        }


        [HttpGet]
        public async Task<IActionResult> TakeSurvey(int id)
        {
            var currentUser = await GetCurrentUserAsync();
            var survey = await _context.Surveys
                .Include(s => s.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(s => s.SurveyId == id);

            if (survey == null)
                return NotFound();

            foreach (var q in survey.Questions)
            {
                Console.WriteLine($"Câu hỏi: {q.QuestionText}, Số lựa chọn: {q.Options?.Count}");
            }


            // Kiểm tra nếu người dùng đã tham gia khảo sát trước đó
            if (currentUser != null)
            {
                var existed = await _context.Participations
                    .AnyAsync(p => p.SurveyId == id && p.UserId == currentUser.UserId);
                if (existed)
                {
                    return RedirectToAction("ThankYou");
                }
            }

            return View(survey);
        }


        //Chức năng tìm kiếm
        public IActionResult Search(string? keyword, string? sortOrder, int page = 1)
        {
            int pageSize = 9;
            var userId = HttpContext.Session.GetInt32("UserId");

            var surveys = _context.Surveys
                .Include(s => s.Participations)
                .Include(s => s.CreatorUser)
                .Where(s => s.IsPublic || s.SurveyAllowedUsers.Any(au => au.UserId == userId));

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.ToLower();
                surveys = surveys.Where(s =>
                    s.Title.ToLower().Contains(keyword) ||
                    s.Description.ToLower().Contains(keyword));
            }

            // Sắp xếp
            surveys = sortOrder switch
            {
                "date_asc" => surveys.OrderBy(s => s.CreatedDate),
                "participants_desc" => surveys.OrderByDescending(s => s.Participations.Count),
                "participants_asc" => surveys.OrderBy(s => s.Participations.Count),
                _ => surveys.OrderByDescending(s => s.CreatedDate), // Mặc định: mới nhất
            };

            int totalSurveys = surveys.Count();
            var pagedSurveys = surveys
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var viewModel = new SearchSurveyViewModel
            {
                Keyword = keyword,
                SortOrder = sortOrder,
                Page = page,
                PageSize = pageSize,
                Surveys = pagedSurveys,
                TotalPages = (int)Math.Ceiling((double)totalSurveys / pageSize)
            };

            return View(viewModel);
        }

    }
}
