using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using NBSite.Models.ViewComponents;
using NBSite.Infrastructure;
using System.Security.Claims;

namespace NBSite.Controllers
{
    public class AccountController : Controller
    {
        private readonly NbshopContext _db;
        private readonly IUserService _userService;
        private readonly IAuthService _authService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IEmailSender _emailSender;      
        private readonly AppConfig _appConfig;

        public AccountController(
            NbshopContext db,
            IUserService userService,
            IAuthService authService,
            IPasswordHasher passwordHasher,
            IEmailSender emailSender,                     
            AppConfig appConfig) 
        {
            _db = db;
            _userService = userService;
            _authService = authService;
            _passwordHasher = passwordHasher;
            _emailSender = emailSender;
            _appConfig = appConfig;

        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            // Загружаем активные города для выпадающего списка
            ViewBag.Cities = await _db.ReferencesCities
                .Where(c => c.Active)
                .OrderBy(c => c.Name)
                .ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Проверка существующего email
                var existingUserByEmail = await _userService.GetUserByEmailAsync(model.Email);
                if (existingUserByEmail != null)
                {
                    ModelState.AddModelError("Email", "Пользователь с таким email уже существует");
                    return View(model);
                }

                // Базовый username: либо из модели, либо из email
                string baseUsername;
                if (!string.IsNullOrEmpty(model.Username))
                {
                    baseUsername = model.Username;
                    // Проверяем, не занят ли указанный username
                    var existingUserByUsername = await _userService.GetUserByUsernameAsync(baseUsername);
                    if (existingUserByUsername != null)
                    {
                        ModelState.AddModelError("Username", "Пользователь с таким логином уже существует");
                        return View(model);
                    }
                }
                else
                {
                    // Берём локальную часть email, обрезаем до 150 символов
                    baseUsername = model.Email.Split('@')[0];
                    if (string.IsNullOrEmpty(baseUsername))
                        baseUsername = "user"; // защита от пустой строки
                    if (baseUsername.Length > 150)
                        baseUsername = baseUsername.Substring(0, 150);
                }

                // Генерируем уникальный username, если базовый уже занят
                string finalUsername = baseUsername;
                int suffix = 1;
                while (await _userService.GetUserByUsernameAsync(finalUsername) != null)
                {
                    finalUsername = baseUsername + suffix;
                    suffix++;
                    // Если вдруг превысили лимит длины, обрезаем основу
                    if (finalUsername.Length > 150)
                    {
                        baseUsername = baseUsername.Substring(0, 140); // оставляем место для суффикса
                        finalUsername = baseUsername + suffix;
                    }
                }

                // Создаём пользователя
                var user = new AuthUser
                {
                    Username = finalUsername,
                    Email = model.Email,
                    Password = _passwordHasher.HashPassword(model.Password),
                    IsActive = true,
                    IsStaff = false,
                    IsSuperuser = false,
                    DateJoined = DateTime.UtcNow,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                };

                await _db.AuthUsers.AddAsync(user);
                await _db.SaveChangesAsync();

                // Создаём профиль
                var profile = new AccountsProfile
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Fio = $"{model.LastName} {model.FirstName}".Trim(),
                    Phone = model.Phone,
                    Company = model.Company,
                    CityId = model.CityId,
                    PricesVisible = false
                };

                await _db.AccountsProfiles.AddAsync(profile);
                await _db.SaveChangesAsync();

                // Аутентификация
                await Authenticate(user);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception)
            {
                ViewBag.Cities = await _db.ReferencesCities
                    .Where(c => c.Active)
                    .OrderBy(c => c.Name)
                    .ToListAsync();
                ModelState.AddModelError("", "Произошла ошибка при регистрации. Пожалуйста, попробуйте позже.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Ищем пользователя по email или username
                var user = await _db.AuthUsers
                    .Include(u => u.AccountsProfile)
                    .FirstOrDefaultAsync(u =>
                        (u.Email == model.EmailOrUsername || u.Username == model.EmailOrUsername)
                        && u.IsActive);

                if (user == null)
                {
                    ModelState.AddModelError("", "Пользователь не найден или учетная запись неактивна");
                    return View(model);
                }

                // Проверяем пароль
                var passwordValid = _passwordHasher.VerifyPassword(user.Password, model.Password);
                if (!passwordValid)
                {
                    ModelState.AddModelError("", "Некорректный пароль");
                    return View(model);
                }

                // Обновляем время последнего входа
                user.LastLogin = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                // Аутентифицируем пользователя
                await Authenticate(user);

                // Перенаправляем на returnUrl или на главную
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }
            //TODO: Можно сделать более подробное логгирование ошибок
            catch (Exception)
            {
                ModelState.AddModelError("", "Произошла ошибка при входе. Пожалуйста, попробуйте позже.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userService.GetUserByEmailAsync(model.Email);
            if (user == null)
            {
                // Не сообщаем, что пользователь не существует (безопасность)
                return RedirectToAction("ForgotPasswordConfirmation");
            }

            // Генерируем токен сброса пароля
            var token = Guid.NewGuid().ToString() + DateTime.UtcNow.Ticks.ToString();
            token = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(token))
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", "");

            // Сохраняем токен в профиле
            var profile = await _db.AccountsProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (profile != null)
            {
                profile.ResetPasswordKey = token;
                await _db.SaveChangesAsync();

                // Формируем ссылку для сброса пароля
                var resetLink = Url.Action("ResetPassword", "Account",
                    new { token = token },
                    protocol: HttpContext.Request.Scheme);

                // Отправляем письмо
                var subject = "Восстановление пароля";
                var body = $@"
            <h2>Восстановление пароля</h2>
            <p>Здравствуйте!</p>
            <p>Для восстановления пароля перейдите по ссылке:</p>
            <p><a href='{resetLink}'>{resetLink}</a></p>
            <p>Если вы не запрашивали восстановление пароля, просто проигнорируйте это письмо.</p>
            <p>С уважением, интернет-магазин.</p>
        ";

                await _emailSender.SendEmailAsync(user.Email, subject, body, true);
            }

            return RedirectToAction("ForgotPasswordConfirmation");
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Токен недействителен.");
            }

            var model = new ResetPasswordViewModel { Token = token };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Находим профиль с таким токеном
            var profile = await _db.AccountsProfiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.ResetPasswordKey == model.Token);

            if (profile == null || profile.User == null)
            {
                ModelState.AddModelError("", "Токен недействителен или устарел.");
                return View(model);
            }

            // Обновляем пароль
            var user = profile.User;
            user.Password = _passwordHasher.HashPassword(model.Password);

            // Очищаем токен
            profile.ResetPasswordKey = null;

            await _db.SaveChangesAsync();

            return RedirectToAction("ResetPasswordConfirmation");
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }

            var user = await _db.AuthUsers
                .Include(u => u.AccountsProfile)
                .ThenInclude(p => p!.City)
                .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));

            if (user == null)
            {
                await HttpContext.SignOutAsync();
                return RedirectToAction("Login");
            }

            var model = new UserProfileViewModel
            {
                Email = user.Email,
                Username = user.Username,
                FirstName = user.FirstName!,
                LastName = user.LastName!,
                Phone = user.AccountsProfile?.Phone,
                Company = user.AccountsProfile?.Company,
                CompanyPost = user.AccountsProfile?.CompanyPost,
                Fio = user.AccountsProfile?.Fio,
                CityId = user.AccountsProfile?.CityId,
                PricesVisible = user.AccountsProfile?.PricesVisible ?? false
            };

            // Загружаем список городов для выпадающего списка
            ViewBag.Cities = await _db.ReferencesCities
                .Where(c => c.Active)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(UserProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Cities = await _db.ReferencesCities
                    .Where(c => c.Active)
                    .OrderBy(c => c.Name)
                    .ToListAsync();
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }

            var user = await _db.AuthUsers
                .Include(u => u.AccountsProfile)
                .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));

            if (user == null)
            {
                await HttpContext.SignOutAsync();
                return RedirectToAction("Login");
            }

            // Обновляем данные пользователя
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;

            // Обновляем профиль
            if (user.AccountsProfile == null)
            {
                user.AccountsProfile = new AccountsProfile { UserId = user.Id };
            }

            user.AccountsProfile.Phone = model.Phone;
            user.AccountsProfile.Company = model.Company;
            user.AccountsProfile.CompanyPost = model.CompanyPost;
            user.AccountsProfile.Fio = model.Fio!;
            user.AccountsProfile.CityId = model.CityId;
            user.AccountsProfile.PricesVisible = model.PricesVisible;

            await _db.SaveChangesAsync();

            // Обновляем claims (если изменилось имя)
            await Authenticate(user);

            ViewBag.SuccessMessage = "Профиль успешно обновлен";
            ViewBag.Cities = await _db.ReferencesCities
                .Where(c => c.Active)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(model);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }

            var user = await _db.AuthUsers.FindAsync(long.Parse(userId));
            if (user == null)
            {
                await HttpContext.SignOutAsync();
                return RedirectToAction("Login");
            }

            // Проверяем текущий пароль
            var currentPasswordValid = _passwordHasher.VerifyPassword(user.Password, model.CurrentPassword);
            if (!currentPasswordValid)
            {
                ModelState.AddModelError("CurrentPassword", "Текущий пароль указан неверно");
                return View(model);
            }

            // Обновляем пароль
            user.Password = _passwordHasher.HashPassword(model.NewPassword);
            await _db.SaveChangesAsync();

            ViewBag.SuccessMessage = "Пароль успешно изменен";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private async Task Authenticate(AuthUser user)
        {
            var principal = await _authService.CreateClaimsPrincipalAsync(user);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddDays(7)
                });
        }
    }
}