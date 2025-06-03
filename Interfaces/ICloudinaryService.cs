using System;

namespace DummyProject.Interfaces;

public interface ICloudinaryService
{
    Task<string> UploadImageAsync(IFormFile file);

}