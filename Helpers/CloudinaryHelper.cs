using System;
using System.Configuration;
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
            string cloudName = ConfigurationManager.AppSettings["Cloudinary:CloudName"];
            string apiKey = ConfigurationManager.AppSettings["Cloudinary:ApiKey"];
            string apiSecret = ConfigurationManager.AppSettings["Cloudinary:ApiSecret"];

            Account account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true; 
        }

        public string UploadImage(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
                return null;

            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, file.InputStream),
                Folder = "PandoraWeb/Products",
                UseFilename = true,
                UniqueFilename = true
            };

            var uploadResult = _cloudinary.Upload(uploadParams);

            if (uploadResult.Error != null)
            {
                throw new Exception(uploadResult.Error.Message);
            }

            return uploadResult.SecureUrl.ToString();
        }
    }
}
