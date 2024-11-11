using DATN.Models;
using DATN.Models.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace DATN.Controllers
{
    public class ArticleController : Controller
    {
        private readonly DATNDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        public ArticleController(DATNDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            return View(_context.Articles.ToList());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Article article)
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;
            string fileName = Path.GetFileNameWithoutExtension(article.ImageFile.FileName);
            string extension = Path.GetExtension(article.ImageFile.FileName);
            article.Image = fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
            string path = Path.Combine(wwwRootPath + "/Images/Article/", fileName);
            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                await article.ImageFile.CopyToAsync(fileStream);
            }
            article.CreatedDate = DateTime.Now;
            _context.Add(article);
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

            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }
            return View(article);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Article article)
        {
            if (id != article.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy thông tin bài viết từ cơ sở dữ liệu để truy cập ảnh cũ
                    var existingArticle = await _context.Articles.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
                    if (existingArticle == null)
                    {
                        return NotFound();
                    }

                    string wwwRootPath = _hostEnvironment.WebRootPath;

                    // Kiểm tra xem có ảnh mới được tải lên không
                    if (article.ImageFile != null)
                    {
                        // Đường dẫn của ảnh cũ
                        string oldImagePath = Path.Combine(wwwRootPath + "/Images/Article/", existingArticle.Image);

                        // Xóa ảnh cũ nếu tồn tại
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }

                        // Tạo tên file mới cho ảnh
                        string fileName = Path.GetFileNameWithoutExtension(article.ImageFile.FileName);
                        string extension = Path.GetExtension(article.ImageFile.FileName);
                        article.Image = fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                        string newPath = Path.Combine(wwwRootPath + "/Images/Article/", fileName);

                        // Lưu ảnh mới vào thư mục
                        using (var fileStream = new FileStream(newPath, FileMode.Create))
                        {
                            await article.ImageFile.CopyToAsync(fileStream);
                        }
                    }
                    else
                    {
                        // Nếu không có ảnh mới, giữ lại tên ảnh cũ
                        article.Image = existingArticle.Image;
                    }

                    // Cập nhật bài viết vào cơ sở dữ liệu
                    article.CreatedDate = existingArticle.CreatedDate; // Giữ lại ngày tạo ban đầu
                    _context.Update(article);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!Exists(article.Id))
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
            return View(article);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.Articles
                .FirstOrDefaultAsync(m => m.Id == id);
            if (article == null)
            {
                return NotFound();
            }

            return View(article);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var article = await _context.Articles.FindAsync(id);

            if (article != null)
            {
                // Xác định đường dẫn ảnh trong thư mục wwwroot
                string wwwRootPath = _hostEnvironment.WebRootPath;
                string imagePath = Path.Combine(wwwRootPath, "Images/Article", article.Image);

                // Kiểm tra xem file ảnh có tồn tại không, nếu có thì xóa
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                // Xóa đường dẫn ảnh trong bài viết
                article.Image = null; // Hoặc có thể sử dụng article.Image = string.Empty;

                // Xóa bài viết khỏi cơ sở dữ liệu
                _context.Articles.Remove(article);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }



        private bool Exists(int id)
        {
            return _context.Articles.Any(e => e.Id == id);
        }
    }
}
