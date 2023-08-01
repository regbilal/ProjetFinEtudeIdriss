using ProjetFinEtude.Models;
using ProjetFinEtude.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjetFinEtude.Data;
using Microsoft.AspNetCore.Authorization;
using ProjetFinEtude.ViewModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;

namespace ProjetFinEtude.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public StudentsController(UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AllStudents(string searchString, int? pageNumber)
        {
            ViewData["CurrentFilter"] = searchString;

            var viewModel = _context.Users
               .Join(
               _context.Students,
               users => users.StudentId,
               students => students.Id,
               (users, students) => new
               {
                   Id = users.Id,
                   userId = students.Id,
                   UserName = users.UserName,

                   FirstName = students.FirstName,
                   LastName = students.LastName,
                   MidName = students.MidName,
                   Gender = students.Gender,
                   DateBirth = students.DateBirth,
                   NationalId = students.NationalId,
                   ClassId = students.ClassId
               }
               ).Join(
               _context.Classes,
               student => student.ClassId,
               classRoom => classRoom.Id,
               (student, classRoom) => new
               {
                   Id = student.Id,
                   userId = student.userId,
                   UserName = student.UserName,

                   FirstName = student.FirstName,
                   LastName = student.LastName,
                   MidName = student.MidName,
                   Gender = student.Gender,
                   DateBirth = student.DateBirth,
                   NationalId = student.NationalId,
                   ClassId = student.ClassId,
                   ClassName = classRoom.Name,
               }
               )
               .Select(
               u => new UserProfileViewModel
               {
                   UserId = u.userId,
                   Id = u.Id,
                   UserName = u.UserName,

                   FirstName = u.FirstName,
                   LastName = u.LastName,
                   MidName = u.MidName,
                   DateBirth = u.DateBirth,
                   Gender = u.Gender,
                   NationalId = u.NationalId,
                   ClassName = u.ClassName
               }
               );

            int pageSize = 20;

            if (!String.IsNullOrEmpty(searchString))
            {
                viewModel = viewModel.Where(s => s.LastName.Contains(searchString)
                                       || s.FirstName.Contains(searchString)
                                       || s.UserName.StartsWith(searchString)
                                       || s.NationalId.StartsWith(searchString)
                                       || s.ClassName.StartsWith(searchString)

                                       );
                return View(await PaginatedList<UserProfileViewModel>
                    .CreateAsync(viewModel.AsNoTracking(), pageNumber ?? 1, pageSize));
            }

            return View(await PaginatedList<UserProfileViewModel>
               .CreateAsync(viewModel.AsNoTracking(), pageNumber ?? 1, pageSize));

        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ViewStudent(int? userId)
        {
            if (userId == null) return NotFound();

            var student = await _context.Students.Where(s => s.Id == userId)
                .Include(c => c.Class).Include(a => a.Address).Include(p => p.Parent)
                .FirstOrDefaultAsync();

            var ViewModel = await _userManager.Users.Where(u => u.StudentId == userId)
                .Select(u => new UserProfileViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    Phone = u.PhoneNumber,
                    UserId = student.Id,
                    FirstName = student.FirstName,
                    LastName = student.LastName,
                    MidName = student.MidName,
                    Gender = student.Gender,
                    DateBirth = student.DateBirth,
                    NationalId = student.NationalId,
                    Address = student.Address,
                    Class = student.Class,
                    ParentId = student.ParentId,
                    Parent = student.Parent
                }).FirstOrDefaultAsync();

            if (student == null || ViewModel == null) return NotFound();

            return View(ViewModel);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> EditStudent(int? userId)
        {
            if (userId == null) return NotFound();

            var student = await _userManager.Users.Where(u => u.StudentId == userId)
                .Include(s => s.Student).ThenInclude(a => a.Address)
                .Select(u => new UserProfileViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    Phone = u.PhoneNumber,
                    UserId = u.Student.Id,
                    FirstName = u.Student.FirstName,
                    LastName = u.Student.LastName,
                    MidName = u.Student.MidName,
                    Gender = u.Student.Gender,
                    DateBirth = u.Student.DateBirth,
                    NationalId = u.Student.NationalId,
                    Address = u.Student.Address,
                    AddressId = u.Student.AddressId,
                    ClassId = (int)u.Student.ClassId,
                    ParentId = u.Student.ParentId
                }).FirstOrDefaultAsync();

            if (student == null) return NotFound();


            ViewData["ClassList"] = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name");
            ViewData["parents"] = new SelectList(await _context.Parents.Select(p => new DropDownList
            {
                Id = p.Id,
                DisplayValue = $"{p.NationalId}-{p.FirstName} {p.MidName} {p.LastName}"
            }).ToListAsync(), "Id", "DisplayValue");


            return View(student);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStudent(int userId, UserProfileViewModel model)
        {
            if (userId != model.UserId) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["ClassList"] = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name");
                ViewData["parents"] = new SelectList(await _context.Parents.Select(p => new DropDownList
                {
                    Id = p.Id,
                    DisplayValue = $"{p.NationalId}-{p.FirstName} {p.MidName} {p.LastName}"
                }).ToListAsync(), "Id", "DisplayValue");

                return View(model);
            }

            var user = await _userManager.Users.Include(u => u.Student).ThenInclude(a => a.Address)
                .Where(u => u.Id == model.Id).FirstOrDefaultAsync();
            if (user == null) return NotFound();


            if (user.Email != model.Email)
                if (await _userManager.FindByEmailAsync(model.Email) != null)
                {
                    ModelState.AddModelError("Email", "Email is Already Exists");
                    ViewData["ClassList"] = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name");
                    ViewData["parents"] = new SelectList(await _context.Parents.Select(p => new DropDownList
                    {
                        Id = p.Id,
                        DisplayValue = $"{p.NationalId}-{p.FirstName} {p.MidName} {p.LastName}"
                    }).ToListAsync(), "Id", "DisplayValue");

                    return View(model);
                }

            if (user.UserName != model.UserName)
                if (await _userManager.FindByNameAsync(model.UserName) != null)
                {
                    ModelState.AddModelError("Username", "Username is Already Exists");
                    ViewData["ClassList"] = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name");
                    ViewData["parents"] = new SelectList(await _context.Parents.Select(p => new DropDownList
                    {
                        Id = p.Id,
                        DisplayValue = $"{p.NationalId}-{p.FirstName} {p.MidName} {p.LastName}"
                    }).ToListAsync(), "Id", "DisplayValue");

                    return View(model);
                }
            if (user.PhoneNumber != model.Phone)
                if (await _userManager.Users.AnyAsync(u => u.PhoneNumber == model.Phone))
                {
                    ModelState.AddModelError("Phone", "Phone Number is Already Exists");
                    ViewData["ClassList"] = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name");
                    ViewData["parents"] = new SelectList(await _context.Parents.Select(p => new DropDownList
                    {
                        Id = p.Id,
                        DisplayValue = $"{p.NationalId}-{p.FirstName} {p.MidName} {p.LastName}"
                    }).ToListAsync(), "Id", "DisplayValue");

                    return View(model);
                }
            if (user.Student.NationalId != model.NationalId)
                if (await _userManager.Users.AnyAsync(u => u.Student.NationalId == model.NationalId))
                {
                    ModelState.AddModelError("NationalId", "NationalId Number is Already Exists");
                    ViewData["ClassList"] = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name");
                    ViewData["parents"] = new SelectList(await _context.Parents.Select(p => new DropDownList
                    {
                        Id = p.Id,
                        DisplayValue = $"{p.NationalId}-{p.FirstName} {p.MidName} {p.LastName}"
                    }).ToListAsync(), "Id", "DisplayValue");

                    return View(model);
                }

            if (model.Password != null)
            {
                if (model.Password == model.ConfirmPassword)
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    await _userManager.ResetPasswordAsync(user, token, model.Password);
                }
                else
                {
                    ModelState.AddModelError("Password", "Password Not match");
                    ViewData["ClassList"] = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name");
                    ViewData["parents"] = new SelectList(await _context.Parents.Select(p => new DropDownList
                    {
                        Id = p.Id,
                        DisplayValue = $"{p.NationalId}-{p.FirstName} {p.MidName} {p.LastName}"
                    }).ToListAsync(), "Id", "DisplayValue");

                    return View(model);
                }
            }
            user.UserName = model.UserName;
            user.Email = model.Email;
            user.PhoneNumber = model.Phone;

            user.Student.FirstName = model.FirstName;
            user.Student.LastName = model.LastName;
            user.Student.MidName = model.MidName;
            user.Student.Gender = model.Gender;
            user.Student.DateBirth = model.DateBirth.Date;
            user.Student.NationalId = model.NationalId;
            user.Student.ParentId = model.ParentId;

            user.Student.ClassId = model.ClassId;
            user.Student.Address.Address1 = model.Address.Address1;
            user.Student.Address.Address2 = model.Address.Address2;
            user.Student.Address.District = model.Address.District;
            user.Student.Address.Location = model.Address.Location;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
                return RedirectToAction(nameof(AllStudents));
            else
            {
                ViewData["ClassList"] = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name");
                ViewData["parents"] = new SelectList(await _context.Parents.Select(p => new DropDownList
                {
                    Id = p.Id,
                    DisplayValue = $"{p.NationalId}-{p.FirstName} {p.MidName} {p.LastName}"
                }).ToListAsync(), "Id", "DisplayValue");

                return View(model);

            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> AddStudent()
        {
            var parents = await _context.Parents.Select(e => new
            {
                Id = e.Id,
                Name = $"{e.NationalId}-{e.FirstName}  {e.LastName}"
            }).ToListAsync();
            ViewData["parents"] = new SelectList(parents, "Id", "Name");
            ViewData["ClassList"] = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name");
            return View();
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStudent(UserProfileViewModel user)
        {
            if (!ModelState.IsValid)
            {
                var parents = await _context.Parents.Select(e => new
                {
                    Id = e.Id,
                    Name = $"{e.NationalId}-{e.FirstName}  {e.LastName}"
                }).ToListAsync();
                ViewData["parents"] = new SelectList(parents, "Id", "Name");
                ViewData["ClassList"] = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name");
                return View(user);
            }
            if (user.Email != null)
                if (await _userManager.FindByEmailAsync(user.Email) != null)
                {
                    var parents = await _context.Parents.Select(e => new
                    {
                        Id = e.Id,
                        Name = $"{e.NationalId}-{e.FirstName}  {e.LastName}"
                    }).ToListAsync();
                    ViewData["parents"] = new SelectList(parents, "Id", "Name");
                    ViewData["ClassList"] = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name");
                    ModelState.AddModelError("Email", "Email is Already Exists");
                    return View(user);
                }

            if (await _userManager.FindByNameAsync(user.UserName) != null)
            {
                var parents = await _context.Parents.Select(e => new
                {
                    Id = e.Id,
                    Name = $"{e.NationalId}-{e.FirstName}  {e.LastName}"
                }).ToListAsync();
                ViewData["parents"] = new SelectList(parents, "Id", "Name");
                ViewData["ClassList"] = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name");
                ModelState.AddModelError("Username", "Username is Already Exists");
                return View(user);
            }
            if (user.Phone != null)
                if (await _userManager.Users.AnyAsync(u => u.PhoneNumber == user.Phone))
                {
                    var parents = await _context.Parents.Select(e => new
                    {
                        Id = e.Id,
                        Name = $"{e.NationalId}-{e.FirstName}  {e.LastName}"
                    }).ToListAsync();
                    ViewData["parents"] = new SelectList(parents, "Id", "Name");
                    ViewData["ClassList"] = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name");
                    ModelState.AddModelError("Phone", "Phone Number is Already Exists");
                    return View(user);
                }
            if (await _userManager.Users.Include(s => s.Student).AnyAsync(u => u.Student.NationalId == user.NationalId))
            {
                var parents = await _context.Parents.Select(e => new
                {
                    Id = e.Id,
                    Name = $"{e.NationalId}-{e.FirstName}  {e.LastName}"
                }).ToListAsync();
                ViewData["parents"] = new SelectList(parents, "Id", "Name");
                ViewData["ClassList"] = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name");
                ModelState.AddModelError("NationalId", "NationalId Number is Already Exists");
                return View(user);
            }
            if (user.Password != user.ConfirmPassword || user.Password == null)
            {
                var parents = await _context.Parents.Select(e => new
                {
                    Id = e.Id,
                    Name = $"{e.NationalId}-{e.FirstName}  {e.LastName}"
                }).ToListAsync();
                ViewData["parents"] = new SelectList(parents, "Id", "Name");
                ViewData["ClassList"] = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name");
                ModelState.AddModelError("Password", "Invlid or not match Password");
                return View(user);
            }


            var newUser = new ApplicationUser
            {
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.Phone
            };

            var newUserStudent = new Student
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                MidName = user.MidName,
                Gender = user.Gender,
                DateBirth = user.DateBirth,
                NationalId = user.NationalId,
                Address = user.Address,
                ClassId = user.ClassId,
                ParentId = user.ParentId
            };
            newUser.Student = newUserStudent;

            var result = await _userManager.CreateAsync(newUser, user.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(newUser, Roles.Student.ToString());
                return RedirectToAction(nameof(AllStudents));
            }
            else return View(user);
        }


    }
}
