using DATN.Migrations;
using DATN.Models;
using DATN.Models.Context;
using DATN.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DATN.Controllers
{
    public class ServiceController : Controller
    {
        private readonly DATNDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        public ServiceController(DATNDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            return View(_context.Services.ToList());
        }

        [HttpGet]
        public IActionResult Create() 
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Service service)
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;
            string fileName = Path.GetFileNameWithoutExtension(service.ImageFile.FileName);
            string extension = Path.GetExtension(service.ImageFile.FileName);
            service.Image = fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
            string path = Path.Combine(wwwRootPath + "/Images/Service/", fileName);
            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                await service.ImageFile.CopyToAsync(fileStream);
            }
            service.CreatedDate = DateTime.Now;
            _context.Add(service);
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

            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }

            return View(service); // This will bind the existing `Content` value to the CKEditor field.
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Service service)
        {
            if (id != service.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy thông tin bài viết từ cơ sở dữ liệu để truy cập ảnh cũ
                    var existingService = await _context.Services.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
                    if (existingService == null)
                    {
                        return NotFound();
                    }

                    string wwwRootPath = _hostEnvironment.WebRootPath;

                    // Kiểm tra xem có ảnh mới được tải lên không
                    if (service.ImageFile != null)
                    {
                        // Đường dẫn của ảnh cũ
                        string oldImagePath = Path.Combine(wwwRootPath + "/Images/Service/", existingService.Image);

                        // Xóa ảnh cũ nếu tồn tại
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }

                        // Tạo tên file mới cho ảnh
                        string fileName = Path.GetFileNameWithoutExtension(service.ImageFile.FileName);
                        string extension = Path.GetExtension(service.ImageFile.FileName);
                        service.Image = fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                        string newPath = Path.Combine(wwwRootPath + "/Images/Service/", fileName);

                        // Lưu ảnh mới vào thư mục
                        using (var fileStream = new FileStream(newPath, FileMode.Create))
                        {
                            await service.ImageFile.CopyToAsync(fileStream);
                        }
                    }
                    else
                    {
                        // Nếu không có ảnh mới, giữ lại tên ảnh cũ
                        service.Image = existingService.Image;
                    }

                    // Cập nhật bài viết vào cơ sở dữ liệu
                    service.CreatedDate = existingService.CreatedDate; // Giữ lại ngày tạo ban đầu
                    _context.Update(service);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!Exists(service.Id))
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
            return View(service);
        }




        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services
                .FirstOrDefaultAsync(m => m.Id == id);
            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var service = await _context.Services.FindAsync(id);

            if (service != null)
            {
                // Xác định đường dẫn ảnh trong thư mục wwwroot
                string wwwRootPath = _hostEnvironment.WebRootPath;
                string imagePath = Path.Combine(wwwRootPath, "Images/Service", service.Image);

                // Kiểm tra xem file ảnh có tồn tại không, nếu có thì xóa
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                // Xóa đường dẫn ảnh trong bài viết
                service.Image = null; // Hoặc có thể sử dụng article.Image = string.Empty;

                // Xóa bài viết khỏi cơ sở dữ liệu
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool Exists(int id)
        {
            return _context.Services.Any(e => e.Id == id);
        }
    }
}
