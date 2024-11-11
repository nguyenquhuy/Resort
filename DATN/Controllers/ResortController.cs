using DATN.Migrations;
using DATN.Models;
using DATN.Models.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DATN.Controllers
{
	public class ResortController : Controller
	{
		private readonly DATNDbContext _context;
		private readonly IWebHostEnvironment _hostEnvironment;
		public ResortController(DATNDbContext context, IWebHostEnvironment hostEnvironment)
		{
			_context = context;
			_hostEnvironment = hostEnvironment;
		}

		public IActionResult Index()
		{
			return View(_context.Resort.ToList());
		}

		[HttpGet]
		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Create(Resort resort)
		{
			string wwwRootPath = _hostEnvironment.WebRootPath;
			string fileName = Path.GetFileNameWithoutExtension(resort.IFormFile.FileName);
			string extension = Path.GetExtension(resort.IFormFile.FileName);
			resort.Image = fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
			string path = Path.Combine(wwwRootPath + "/Images/Resort/", fileName);
			using (var fileStream = new FileStream(path, FileMode.Create))
			{
				await resort.IFormFile.CopyToAsync(fileStream);
			}
			_context.Add(resort);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var resort = await _context.Resort.FindAsync(id);
            if (resort == null)
            {
                return NotFound();
            }
            return View(resort);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Resort resort)
        {
            if (id != resort.Id)
            {
                return NotFound();
            }

            try
            {
                // Lấy thông tin bài viết từ cơ sở dữ liệu để truy cập ảnh cũ
                var existingresort = await _context.Resort.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
                if (existingresort == null)
                {
                    return NotFound();
                }

                string wwwRootPath = _hostEnvironment.WebRootPath;

                // Kiểm tra xem có ảnh mới được tải lên không
                if (resort.IFormFile != null)
                {
                    // Đường dẫn của ảnh cũ
                    string oldImagePath = Path.Combine(wwwRootPath + "/Images/resort/", existingresort.Image);

                    // Xóa ảnh cũ nếu tồn tại
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }

                    // Tạo tên file mới cho ảnh
                    string fileName = Path.GetFileNameWithoutExtension(resort.IFormFile.FileName);
                    string extension = Path.GetExtension(resort.IFormFile.FileName);
                    resort.Image = fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                    string newPath = Path.Combine(wwwRootPath + "/Images/resort/", fileName);

                    // Lưu ảnh mới vào thư mục
                    using (var fileStream = new FileStream(newPath, FileMode.Create))
                    {
                        await resort.IFormFile.CopyToAsync(fileStream);
                    }
                }
                else
                {
                    // Nếu không có ảnh mới, giữ lại tên ảnh cũ
                    resort.Image = existingresort.Image;
                }
                _context.Update(resort);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Exists(resort.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }

        private bool Exists(int id)
        {
            return _context.Resort.Any(e => e.Id == id);
        }
    }
}
