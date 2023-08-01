using ProjetFinEtude.Constants;
using ProjetFinEtude.Data;
using ProjetFinEtude.Models;
using ProjetFinEtude.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetFinEtude.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public AdminController(UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var students = await _context.Students.ToListAsync();
            ViewData["AllStudents"] = students.Count;
            ViewData["AllParents"] = _context.Parents.CountAsync().Result;
            ViewData["AllTeachers"] = _context.Teachers.CountAsync().Result;
            ViewData["AllClasses"] = _context.Classes.CountAsync().Result;
            ViewData["Male"] = students.Where(s => s.Gender == 'M').Count();
            ViewData["Female"] = students.Where(s => s.Gender == 'F').Count();

            var currentUserId = _userManager.GetUserId(User);
            var viewModel = new AdminHomeViewModel
            {
                Notice = await _context.Notices.OrderByDescending(e => e.PostDateTime.Date).Take(10).ToListAsync(),
                Chats = await _context.Chats.OrderByDescending(o => o.SendDate).Where(c => c.FromId == currentUserId).Take(10).ToListAsync()
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Profile(string edit)
        {
            if (edit != null) ViewData["Edit"] = "Edit";

            var user = await _userManager.GetUserAsync(User);
            return View(user);
        }
        [HttpPost]
        public async Task<IActionResult> Profile(string id, ApplicationUser model)
        {
            if (id != model.Id) return NotFound();

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            if (model.UserName != user.UserName)
                if (await _userManager.FindByNameAsync(model.UserName) != null)
                {
                    ModelState.AddModelError("UserName", "Username Is Already Exist");
                    ViewData["Edit"] = "Edit";
                    return View(model);
                }
            if (user.Email != model.Email)
            {
                if (await _userManager.FindByEmailAsync(model.Email) != null)
                {
                    ModelState.AddModelError("Email", "Email Is Already Exist");
                    ViewData["Edit"] = "Edit";
                    return View(model);
                }
            }
            if (user.PhoneNumber != model.PhoneNumber)
            {
                if (_userManager.Users.Any(e => e.PhoneNumber == model.PhoneNumber))
                {
                    ModelState.AddModelError("PhoneNumber", "Phone Number Is Already Exist");
                    ViewData["Edit"] = "Edit";
                    return View(model);
                }
            }

            user.UserName = model.UserName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded) return RedirectToAction(nameof(Profile));
            else { ViewData["Edit"] = "Edit"; return View(model); }

        }



        //Chat 
        [HttpGet]
        public async Task<IActionResult> Chat()
        {
            var currentUserId = _userManager.GetUserId(User);
            var users = await _userManager.Users.Where(u => u.Id != currentUserId).ToListAsync();
            ViewBag.userToList = new SelectList(users.Select(u => new DropDownList
            {
                AccountId = u.Id,
                DisplayValue = u.UserName
            }), "AccountId", "DisplayValue", currentUserId);
            ViewBag.userId = currentUserId;
            return View();
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Chat(Chat chat)
        {
            var currentUserId = _userManager.GetUserId(User);

            if (!ModelState.IsValid)
            {
                var users = await _userManager.Users.Where(u => u.Id != currentUserId).ToListAsync();
                ViewBag.userToList = new SelectList(users.Select(u => new DropDownList
                {
                    AccountId = u.Id,
                    DisplayValue = u.UserName
                }), "AccountId", "DisplayValue", currentUserId);
                return View(chat);
            }
            chat.SendDate = DateTime.Now;
            chat.FromId = currentUserId;
            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Chat));
        }

        [AllowAnonymous]
        public IActionResult GetEvents()
        {
            var events = _context.Events.Select(e => new
            {
                id = e.Id,
                title = e.Title,
                description = e.Description ?? "",
                start = e.Start.ToString("yyyy-MM-dd"),
                end = e.End.HasValue ?
                Convert.ToDateTime(e.End).ToString("yyyy-MM-dd") : null
            }).ToList();

            return new JsonResult(events);
        }

    }
}