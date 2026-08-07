using System;
using System.Configuration;
using System.IO;
using System.Web;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace PandoraWeb.Helpers
{
    public class CloudinaryHelper
    {
        private Cloudinary _cloudinary;

        public CloudinaryHelper()
        {
            try
            {
                string cloudName = ConfigurationManager.AppSettings["Cloudinary:CloudName"];
                string apiKey = ConfigurationManager.AppSettings["Cloudinary:ApiKey"];
                string apiSecret = ConfigurationManager.AppSettings["Cloudinary:ApiSecret"];

                if (!string.IsNullOrWhiteSpace(cloudName) && !string.IsNullOrWhiteSpace(apiKey) && !string.IsNullOrWhiteSpace(apiSecret))
                {
                    Account account = new Account(cloudName, apiKey, apiSecret);
                    _cloudinary = new Cloudinary(account);
                    _cloudinary.Api.Secure = true;
                }
            }
            catch
            {
                _cloudinary = null;
            }
        }

        public string UploadImage(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
                return null;

            // 1. Try Cloudinary first if configured
            if (_cloudinary != null)
            {
                try
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.FileName, file.InputStream),
                        Folder = "PandoraWeb/Products",
                        UseFilename = true,
                        UniqueFilename = true
                    };

                    var uploadResult = _cloudinary.Upload(uploadParams);
                    if (uploadResult != null && uploadResult.SecureUrl != null)
                    {
                        return uploadResult.SecureUrl.ToString();
                    }
                }
                catch
                {
                    // Fallback to local upload
                }
            }

            // 2. Local fallback upload supporting all file extensions
            try
            {
                string uploadFolder = HttpContext.Current.Server.MapPath("~/uploads/");
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                string fileExtension = Path.GetExtension(file.FileName);
                if (string.IsNullOrEmpty(fileExtension))
                {
                    fileExtension = ".jpg";
                }

                string uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                string savePath = Path.Combine(uploadFolder, uniqueFileName);

                file.InputStream.Position = 0;
                file.SaveAs(savePath);

                return $"uploads/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi tải hình ảnh lên: " + ex.Message);
            }
        }
    }
}

