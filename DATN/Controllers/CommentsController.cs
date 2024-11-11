using DATN.Models;
using DATN.Models.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DATN.Controllers
{
    public class CommentsController : Controller
    {
        private readonly DATNDbContext _context;

        public CommentsController(DATNDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var comments = _context.Comments
                                    .Include(c => c.Room)  // Include Room entity
                                    .Include(c => c.Account)  // Include Account entity
                                    .ToList();
            return View(comments);
        }

        // GET: RoomTypes/Edit/5
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }
            return View(comment);
        }

        // POST: RoomTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, Comment comment)
        {
            if (id != comment.Id)
            {
                return NotFound();
            }

            try
            {
                var existingComment = await _context.Comments.FindAsync(id);
                if (existingComment == null)
                {
                    return NotFound();
                }

                // Giữ nguyên giá trị của Created khi cập nhật
                existingComment.Content = comment.Content;
                existingComment.Status = comment.Status;

                _context.Update(existingComment);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommentExists(comment.Id))
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

        private bool CommentExists(int id)
        {
            return _context.Comments.Any(e => e.Id == id);
        }




        // ĐÂY LÀ THÊM SỬA XÓA BÊN USER
        //Thêm bình luận
        [HttpPost]
        public async Task<IActionResult> PostComment(int roomId, string content)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập mới được thêm bình luận." });
            }

            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

            var comment = new Comment
            {
                AccountId = userId,
                RoomId = roomId,
                Content = content,
                Created = DateTime.Now,
                Status = true
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return Json(new { success = true});
        }

        //Sửa bình luận
        [HttpPost]
        [ValidateAntiForgeryToken] // Ensure anti-forgery token validation
        public async Task<IActionResult> Edit(int id, string content)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return Json(new { success = false, message = "Không tìm thấy bình luận." });
            }

            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Json(new { success = false, message = "Bạn không phải chủ bình luận này." });
            }

            if (comment.AccountId != userId)
            {
                return Json(new { success = false, message = "Bạn không được phép sửa bình luận này." });
            }

            // Update the comment content
            comment.Content = content;

            // Save changes to the database
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();

            // Return success response with updated comment data
            return Json(new { success = true, comment = comment });
        }

        //Xóa bình luận
        [HttpDelete]
        [ValidateAntiForgeryToken] // Ensure anti-forgery token validation
        public async Task<IActionResult> Delete(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return Json(new { success = false, message = "Không tìm thấy bình luận." });
            }

            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return Json(new { success = false, message = "Bạn không phải chủ bình luận này." });
            }

            if (comment.AccountId != userId)
            {
                return Json(new { success = false, message = "Bạn không được phép sửa bình luận này." });
                }

            // Delete the comment
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            // Return success response
            return Json(new { success = true });
        }

    }
}
