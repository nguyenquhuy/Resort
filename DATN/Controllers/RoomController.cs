using DATN.Migrations;
using DATN.Models;
using DATN.Models.Context;
using DATN.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace DATN.Controllers
{
    public class RoomController : Controller
    {
        private readonly DATNDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public RoomController(DATNDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var rooms = await _context.Rooms.Include(r => r.RoomType).ToListAsync();
            ViewBag.Amenities = new SelectList(_context.Amenity, "ID", "Name");
            return View(rooms);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            RoomGal rg = new RoomGal();
            // Populate RoomTypes for dropdown
            ViewBag.RoomTypes = new SelectList(await _context.RoomTypes.ToListAsync(), "Id", "Name");
            ViewBag.Amenities = new SelectList(await _context.Amenity.ToListAsync(), "ID", "Name");
            return View(rg);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoomGal rg, List<int> SelectedAmenities)
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;
            string fileName = Path.GetFileNameWithoutExtension(rg.Room.ImageFile.FileName);
            string extension = Path.GetExtension(rg.Room.ImageFile.FileName);
            rg.Room.Image = fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
            string path = Path.Combine(wwwRootPath + "/Images/Room/", fileName);
            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                await rg.Room.ImageFile.CopyToAsync(fileStream);
            }
            rg.Room.Status = false;
            _context.Add(rg.Room);
            await _context.SaveChangesAsync();
            // Add selected amenities to RoomAmenities
            if (SelectedAmenities != null && SelectedAmenities.Any())
            {
                foreach (var amenityId in SelectedAmenities)
                {
                    _context.RoomAmenity.Add(new RoomAmenity
                    {
                        RoomId = rg.Room.ID,
                        AmenityId = amenityId
                    });
                }
                await _context.SaveChangesAsync();
            }

            if (rg.Images.Count > 0)
            {
                foreach (var item in rg.Images)
                {
                    string stringFileName = UploadFile(item);
                    var roomgal = new GalleryRooms
                    {
                        Image = stringFileName,
                        RoomId = rg.Room.ID
                    };
                    _context.Add(roomgal);
                }
                await _context.SaveChangesAsync();
            }            
            return RedirectToAction(nameof(Index));
        }

        private string UploadFile(IFormFile file)
        {
            string filename = null;
            if(file != null)
            {
                string uploadDir = Path.Combine(_hostEnvironment.WebRootPath, "RoomGallery");
                filename = Guid.NewGuid().ToString() + "-" + file.FileName;
                string filePath = Path.Combine(uploadDir, filename);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }
            }
            return filename;
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var room = await _context.Rooms.Include(r => r.GalleryRooms).FirstOrDefaultAsync(r => r.ID == id);
            var selectedAmenity = _context.RoomAmenity.Where(a=>a.RoomId==id).Select(a=>a.AmenityId).ToList();
            MultiSelectList multiAmenityList = new MultiSelectList(_context.Amenity.AsQueryable(), "ID", "Name", selectedAmenity);
            room.MultiCategoryList = multiAmenityList;
            if (room == null)
            {
                return NotFound();
            }

            var viewModel = new RoomGal
            {
                Room = room,
                Images = new List<IFormFile>(),
                
            };
            
            ViewBag.RoomTypes = new SelectList(await _context.RoomTypes.ToListAsync(), "Id", "Name", room.RoomTypeId);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RoomGal viewModel)
        {
            if (id != viewModel.Room.ID)
            {
                return NotFound();
            }

            // Retrieve the Room entity including GalleryRooms from the database
            var room = await _context.Rooms.Include(r => r.GalleryRooms).FirstOrDefaultAsync(r => r.ID == id);
            if (room == null)
            {
                return NotFound();
            }

            string wwwRootPath = _hostEnvironment.WebRootPath;

            // Update Room properties
            room.Name = viewModel.Room.Name;
            room.Description = viewModel.Room.Description;
            room.Price = viewModel.Room.Price;
            room.RoomTypeId = viewModel.Room.RoomTypeId;
            room.MetaDescription = viewModel.Room.MetaDescription;
            var amenityRemove = _context.RoomAmenity.Where(ra => ra.RoomId == id && !viewModel.SelectedAmenities.Contains(ra.AmenityId));
            _context.RoomAmenity.RemoveRange(amenityRemove);
            foreach(var amenity in viewModel.SelectedAmenities)
            {
                if(!_context.RoomAmenity.Any(ra=>ra.RoomId==id && ra.AmenityId == amenity))
                {
                    _context.RoomAmenity.Add(new RoomAmenity { RoomId = id, AmenityId = amenity });
                }
            }
            // Handle main room image update
            if (viewModel.Room.ImageFile != null)
            {
                // Delete the old image if it exists
                string oldImagePath = Path.Combine(wwwRootPath + "/Images/Room/", room.Image);
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }

                // Save the new image
                string fileName = Path.GetFileNameWithoutExtension(viewModel.Room.ImageFile.FileName);
                string extension = Path.GetExtension(viewModel.Room.ImageFile.FileName);
                room.Image = fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                string newPath = Path.Combine(wwwRootPath + "/Images/Room/", fileName);

                using (var fileStream = new FileStream(newPath, FileMode.Create))
                {
                    await viewModel.Room.ImageFile.CopyToAsync(fileStream);
                }
            }

            foreach (var file in Request.Form.Files)
            {
                var imageId = Request.Form["ReplacementImageIds"].FirstOrDefault(id => file.Name.EndsWith($"_{id}"));
                if (imageId != null && int.TryParse(imageId, out int parsedImageId))
                {
                    var galleryImage = room.GalleryRooms.FirstOrDefault(g => g.ID == parsedImageId);
                    if (galleryImage != null)
                    {
                        // Delete old image file if it exists
                        string oldImagePath = Path.Combine(wwwRootPath, "RoomGallery", galleryImage.Image);
                        if (System.IO.File.Exists(oldImagePath)) System.IO.File.Delete(oldImagePath);

                        // Save new image
                        string newFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                        string newPath = Path.Combine(wwwRootPath, "RoomGallery", newFileName);
                        using (var stream = new FileStream(newPath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        // Update the database entry
                        galleryImage.Image = newFileName;
                    }
                }
            }
            // Add New Images to GalleryRooms
            if (viewModel.Images != null && viewModel.Images.Count > 0)
            {
                foreach (var imageFile in viewModel.Images)
                {
                    string fileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    string path = Path.Combine(_hostEnvironment.WebRootPath, "RoomGallery", fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    room.GalleryRooms.Add(new GalleryRooms { Image = fileName, RoomId = room.ID });
                }
            }
            // Save changes to the database
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }




        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            var room = await _context.Rooms.Include(r => r.GalleryRooms).FirstOrDefaultAsync(r => r.ID == id);
            var selectedAmenity = _context.RoomAmenity.Where(a => a.RoomId == id).Select(a => a.AmenityId).ToList();
            MultiSelectList multiAmenityList = new MultiSelectList(_context.Amenity.AsQueryable(), "ID", "Name", selectedAmenity);
            room.MultiCategoryList = multiAmenityList;
            if (room == null)
            {
                return NotFound();
            }

            var viewModel = new RoomGal
            {
                Room = room,
                Images = new List<IFormFile>()
            };

            ViewBag.RoomTypes = new SelectList(await _context.RoomTypes.ToListAsync(), "Id", "Name", room.RoomTypeId);
            return View(viewModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var room = await _context.Rooms
                                     .Include(r => r.GalleryRooms) // Load associated GalleryRooms
                                     .FirstOrDefaultAsync(r => r.ID == id);

            if (room != null)
            {
                // Define wwwroot path for images
                string wwwRootPath = _hostEnvironment.WebRootPath;

                // Delete main room image if it exists
                if (!string.IsNullOrEmpty(room.Image))
                {
                    string mainImagePath = Path.Combine(wwwRootPath, "Images/Room", room.Image);
                    if (System.IO.File.Exists(mainImagePath))
                    {
                        System.IO.File.Delete(mainImagePath);
                    }
                }

                // Delete each gallery image file and remove the record from the database
                foreach (var galleryImage in room.GalleryRooms)
                {
                    string galleryImagePath = Path.Combine(wwwRootPath, "RoomGallery", galleryImage.Image);
                    if (System.IO.File.Exists(galleryImagePath))
                    {
                        System.IO.File.Delete(galleryImagePath);
                    }

                    _context.GalleryRooms.Remove(galleryImage); // Remove gallery image record
                }
                var roomAme = _context.RoomAmenity.Where(r => r.RoomId == id);
                foreach(var roomAmes in roomAme)
                {
                    _context.Remove(roomAmes);
                }
                _context.Rooms.Remove(room); // Remove room record
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult DeleteDetailedImage(int id)
        {
            // Find the image in the database
            var galleryImage = _context.GalleryRooms.Find(id);
            if (galleryImage != null)
            {
                // Optionally, delete the image file from the server if required
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "RoomGallery", galleryImage.Image);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                // Remove from the database
                _context.GalleryRooms.Remove(galleryImage);
                _context.SaveChanges();
            }

            return Json(new { success = true });
        }

        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.ID == id);
        }
    }
}
