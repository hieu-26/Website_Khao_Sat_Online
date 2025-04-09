
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SurveyWebsite.Models;
using SurveyWebsite.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;



namespace SurveyWebsite.Controllers
{
    [Authorize]
    //[AllowAnonymous]

    public class SurveyController : Controller
    {
        private readonly SurveyDbContext _context;

        public SurveyController(SurveyDbContext context)
        {
            _context = context;
        }

        // GET: Survey/Create
        public IActionResult Create()
        {
            var model = new SurveyCreateViewModel
            {
                Questions = new List<QuestionViewModel>
                {
                    new QuestionViewModel
                    {
                        Options = new List<OptionViewModel> { new OptionViewModel(), new OptionViewModel() }
                    }
                }
            };
            return View(model);
        }

        // POST: Survey/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SurveyCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                foreach (var error in errors)
                {
                    Console.WriteLine(error); // hoặc log lỗi
                }
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var survey = new Survey
            {
                Title = model.Title,
                Description = model.Description,
                IsPublic = model.IsPublic,
                CreatorUserId = userId,
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now
            };

            _context.Surveys.Add(survey);
            await _context.SaveChangesAsync(); // Để lấy được SurveyID

            var setting = new SurveySetting
            {
                SurveyId = survey.SurveyId,
                AllowMultipleResponses = model.AllowMultipleResponses,
                RequireLogin = model.RequireLogin,
                StartDate = model.StartDate,
                EndDate = model.EndDate
            };
            _context.SurveySettings.Add(setting);

            foreach (var qvm in model.Questions)
            {
                var question = new Question
                {
                    SurveyId = survey.SurveyId,
                    QuestionText = qvm.QuestionText,
                    QuestionType = qvm.QuestionType,
                    IsRequired = qvm.IsRequired
                };
                _context.Questions.Add(question);
                await _context.SaveChangesAsync(); // Để lấy QuestionID

                foreach (var opt in qvm.Options)
                {
                    var option = new Option
                    {
                        QuestionId = question.QuestionId,
                        OptionText = opt.OptionText
                    };
                    _context.Options.Add(option);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("MySurveys");
        }

        // GET: Survey/MySurveys
        public async Task<IActionResult> MySurveys()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var surveys = await _context.Surveys
                .Include(s => s.SurveySetting)
                .Where(s => s.CreatorUserId == userId)
                .ToListAsync();

            return View(surveys);
        }

        // GET: Survey/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var survey = await _context.Surveys
                .Include(s => s.SurveySetting)
                .Include(s => s.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(s => s.SurveyId == id);

            if (survey == null) return NotFound();

            var model = new SurveyCreateViewModel
            {
                Title = survey.Title,
                Description = survey.Description,
                IsPublic = survey.IsPublic,
                AllowMultipleResponses = survey.SurveySetting.AllowMultipleResponses,
                RequireLogin = survey.SurveySetting.RequireLogin,
                StartDate = survey.SurveySetting.StartDate,
                EndDate = survey.SurveySetting.EndDate,
                Questions = survey.Questions.Select(q => new QuestionViewModel
                {
                    QuestionText = q.QuestionText,
                    QuestionType = q.QuestionType,
                    IsRequired = q.IsRequired,
                    Options = q.Options.Select(o => new OptionViewModel
                    {
                        OptionText = o.OptionText
                    }).ToList()
                }).ToList()
            };

            ViewBag.SurveyId = survey.SurveyId;
            return View(model);
        }
        // POST: Survey/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SurveyCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.SurveyId = id;
                return View(model);
            }

            var survey = await _context.Surveys
                .Include(s => s.SurveySetting)
                .Include(s => s.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(s => s.SurveyId == id);

            if (survey == null)
                return NotFound();

            // Cập nhật survey
            survey.Title = model.Title;
            survey.Description = model.Description;
            survey.IsPublic = model.IsPublic;
            survey.LastModifiedDate = DateTime.Now;

            // Cập nhật setting
            survey.SurveySetting.AllowMultipleResponses = model.AllowMultipleResponses;
            survey.SurveySetting.RequireLogin = model.RequireLogin;
            survey.SurveySetting.StartDate = model.StartDate;
            survey.SurveySetting.EndDate = model.EndDate;

            // Xóa câu hỏi + đáp án cũ
            _context.Options.RemoveRange(survey.Questions.SelectMany(q => q.Options));
            _context.Questions.RemoveRange(survey.Questions);
            await _context.SaveChangesAsync();

            // Thêm lại các câu hỏi và đáp án từ model
            foreach (var qvm in model.Questions)
            {
                if (string.IsNullOrWhiteSpace(qvm.QuestionText))
                    continue;

                var question = new Question
                {
                    SurveyId = id,
                    QuestionText = qvm.QuestionText,
                    QuestionType = qvm.QuestionType,
                    IsRequired = qvm.IsRequired,
                    Options = new List<Option>()
                };

                foreach (var opt in qvm.Options)
                {
                    if (!string.IsNullOrWhiteSpace(opt.OptionText))
                    {
                        question.Options.Add(new Option
                        {
                            OptionText = opt.OptionText
                        });
                    }
                }

                _context.Questions.Add(question);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("MySurveys");
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var survey = await _context.Surveys
                .Include(s => s.Questions)
                .FirstOrDefaultAsync(m => m.SurveyId == id);

            if (survey == null) return NotFound();

            return View(survey); // View này cần model là Survey
        }
        // POST: Survey/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var survey = await _context.Surveys
                .Include(s => s.Questions)
                .ThenInclude(q => q.Options)
                .Include(s => s.SurveySetting)
                .FirstOrDefaultAsync(s => s.SurveyId == id);

            if (survey != null)
            {
                _context.Options.RemoveRange(survey.Questions.SelectMany(q => q.Options));
                _context.Questions.RemoveRange(survey.Questions);
                _context.SurveySettings.Remove(survey.SurveySetting);
                _context.Surveys.Remove(survey);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("MySurveys");
        }
        // GET: Survey/Results/5
        public async Task<IActionResult> Results(int? id)
        {
            if (id == null) return NotFound();

            var survey = await _context.Surveys
                .Include(s => s.Questions)
                    .ThenInclude(q => q.Options)
                .Include(s => s.Questions)
                    .ThenInclude(q => q.Answers)
                        .ThenInclude(a => a.Option)
                .FirstOrDefaultAsync(s => s.SurveyId == id);

            if (survey == null) return NotFound();

            return View(survey); // Trả về model là Survey
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
        public async Task<IActionResult> TakeSurvey(int id)
        {
            var currentUser = await GetCurrentUserAsync();
            var survey = await _context.Surveys
                .Include(s => s.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(s => s.SurveyId == id);

            if (survey == null)
                return NotFound();

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

        [HttpGet]
        public IActionResult ThankYou()
        {
            return View();
        }


        //Chức năng tìm kiếm
        public IActionResult Search(string? keyword, string? sortOrder, int page = 1)
        {
            int pageSize = 9;
            var userId = HttpContext.Session.GetInt32("UserId");

            var surveys = _context.Surveys
                .Include(s => s.Participations)
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
