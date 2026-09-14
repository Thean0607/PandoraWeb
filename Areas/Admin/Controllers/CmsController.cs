using System.Web.Mvc;
using PandoraWeb.Filters;
using PandoraWeb.Models;
using PandoraWeb.Models.Data;
using System.Linq;
using System.Data.Entity;
using System;

namespace PandoraWeb.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class CmsController : Controller
    {
        private PandoraDbContext db = new PandoraDbContext();

        [AdminAuthorize(Permission = "manage_cms")]
        public ActionResult Pages()
        {
            ViewBag.ActiveMenu = "CMS";
            ViewBag.ActiveSubMenu = "Pages";
            ViewBag.Title = "Trang Tĩnh";
            var pages = db.Pages.OrderByDescending(p => p.CreatedAt).ToList();
            return View(pages);
        }
        
        [AdminAuthorize(Permission = "manage_cms")]
        public ActionResult Blog()
        {
            ViewBag.ActiveMenu = "CMS";
            ViewBag.ActiveSubMenu = "Blog";
            ViewBag.Title = "Bài Viết (Blog)";
            var posts = db.BlogPosts.OrderByDescending(p => p.PublishedDate).ToList();
            return View(posts);
        }

        [AdminAuthorize(Permission = "manage_cms")]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SaveBlog(int? postId, string title, string author, bool isPublished, string content, System.Web.HttpPostedFileBase imageFile)
        {
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(content))
            {
                TempData["Error"] = "Tiêu đề và Nội dung không được để trống!";
                return RedirectToAction("Blog");
            }

            BlogPost post;
            if (postId.HasValue && postId.Value > 0)
            {
                post = db.BlogPosts.Find(postId.Value);
                if (post == null)
                {
                    TempData["Error"] = "Không tìm thấy bài viết!";
                    return RedirectToAction("Blog");
                }
            }
            else
            {
                post = new BlogPost();
                post.PublishedDate = DateTime.Now;
                db.BlogPosts.Add(post);
            }

            post.Title = title.Trim();
            post.Author = string.IsNullOrWhiteSpace(author) ? "Admin" : author.Trim();
            post.IsPublished = isPublished;
            post.Content = content;

            if (imageFile != null && imageFile.ContentLength > 0)
            {
                try
                {
                    var cloudinaryHelper = new PandoraWeb.Helpers.CloudinaryHelper();
                    string uploadedUrl = cloudinaryHelper.UploadImage(imageFile);
                    if (!string.IsNullOrEmpty(uploadedUrl))
                    {
                        post.ImageUrl = uploadedUrl;
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Lỗi khi tải ảnh lên Cloudinary: " + ex.Message;
                    return RedirectToAction("Blog");
                }
            }

            db.SaveChanges();
            TempData["Success"] = "Đã lưu bài viết thành công!";
            return RedirectToAction("Blog");
        }

        [AdminAuthorize(Permission = "manage_cms")]
        [HttpPost]
        public JsonResult DeleteBlog(int id)
        {
            var post = db.BlogPosts.Find(id);
            if (post != null)
            {
                db.BlogPosts.Remove(post);
                db.SaveChanges();
                return Json(new { success = true, message = "Xóa bài viết thành công!" });
            }
            return Json(new { success = false, message = "Không tìm thấy bài viết." });
        }
        
        [AdminAuthorize(Permission = "manage_cms")]
        public ActionResult FAQ()
        {
            ViewBag.ActiveMenu = "CMS";
            ViewBag.ActiveSubMenu = "FAQ";
            ViewBag.Title = "Câu Hỏi Thường Gặp";
            var faqs = db.Faqs.OrderBy(f => f.DisplayOrder).ToList();
            return View(faqs);
        }

        [AdminAuthorize(Permission = "manage_cms")]
        [HttpPost]
        public ActionResult SaveFaq(int? id, string question, string answer, int displayOrder, bool isActive)
        {
            if (string.IsNullOrEmpty(question) || string.IsNullOrEmpty(answer))
                return Json(new { success = false, message = "Thiếu thông tin" });

            try
            {
                if (id.HasValue && id.Value > 0)
                {
                    var faq = db.Faqs.Find(id.Value);
                    if (faq != null)
                    {
                        faq.Question = question;
                        faq.Answer = answer;
                        faq.DisplayOrder = displayOrder;
                        faq.IsActive = isActive;
                    }
                }
                else
                {
                    db.Faqs.Add(new Faq
                    {
                        Question = question,
                        Answer = answer,
                        DisplayOrder = displayOrder,
                        IsActive = isActive
                    });
                }
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [AdminAuthorize(Permission = "manage_cms")]
        [HttpPost]
        public ActionResult DeleteFaq(int id)
        {
            try
            {
                var faq = db.Faqs.Find(id);
                if (faq != null)
                {
                    db.Faqs.Remove(faq);
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "Không tìm thấy" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
