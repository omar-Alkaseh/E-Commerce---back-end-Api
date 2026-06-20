namespace Application.Interfaces.Services
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync (Stream fileStream, string fileName, string contentType,
         string folderName);

        Task DeleteFileAsync(string fileUrl);
        
    }
}