using Microsoft.AspNetCore.Components.Forms;

namespace Radio.Web.Services;

public class EpisodeAttachmentInfo
{
    public int EpisodeId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StoredName { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public string UploadedBy { get; set; } = string.Empty;
}

public interface IEpisodeAttachmentService
{
    Task<EpisodeAttachmentInfo> UploadAsync(int episodeId, IFormFile file, string uploadedBy, CancellationToken ct = default);
    Task<bool> DeleteAsync(int episodeId, string storedName);
    List<EpisodeAttachmentInfo> GetAttachments(int episodeId);
    string? GetFilePath(int episodeId, string storedName);
}

public class EpisodeAttachmentService(IWebHostEnvironment env) : IEpisodeAttachmentService
{
    private const string MetaDir = "App_Data/attachments";

    public async Task<EpisodeAttachmentInfo> UploadAsync(int episodeId, IFormFile file, string uploadedBy, CancellationToken ct = default)
    {
        var uploadsDir = Path.Combine(env.WebRootPath, "uploads", episodeId.ToString());
        Directory.CreateDirectory(uploadsDir);

        var ext = Path.GetExtension(file.FileName);
        var storedName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsDir, storedName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream, ct);

        var info = new EpisodeAttachmentInfo
        {
            EpisodeId = episodeId,
            FileName = file.FileName,
            StoredName = storedName,
            SizeBytes = file.Length,
            ContentType = file.ContentType,
            UploadedAt = DateTime.UtcNow,
            UploadedBy = uploadedBy
        };

        SaveMetadata(episodeId, info);
        return info;
    }

    public Task<bool> DeleteAsync(int episodeId, string storedName)
    {
        var filePath = Path.Combine(env.WebRootPath, "uploads", episodeId.ToString(), storedName);
        if (File.Exists(filePath)) File.Delete(filePath);
        RemoveMetadata(episodeId, storedName);
        return Task.FromResult(true);
    }

    public List<EpisodeAttachmentInfo> GetAttachments(int episodeId)
    {
        var metaFile = GetMetaFile(episodeId);
        if (!File.Exists(metaFile)) return new();
        var json = File.ReadAllText(metaFile);
        return System.Text.Json.JsonSerializer.Deserialize<List<EpisodeAttachmentInfo>>(json) ?? new();
    }

    public string? GetFilePath(int episodeId, string storedName)
    {
        var path = Path.Combine(env.WebRootPath, "uploads", episodeId.ToString(), storedName);
        return File.Exists(path) ? $"/uploads/{episodeId}/{storedName}" : null;
    }

    private string GetMetaFile(int episodeId)
    {
        var dir = Path.Combine(env.ContentRootPath, MetaDir);
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, $"episode-{episodeId}.json");
    }

    private void SaveMetadata(int episodeId, EpisodeAttachmentInfo info)
    {
        var list = GetAttachments(episodeId);
        list.Add(info);
        File.WriteAllText(GetMetaFile(episodeId), System.Text.Json.JsonSerializer.Serialize(list));
    }

    private void RemoveMetadata(int episodeId, string storedName)
    {
        var list = GetAttachments(episodeId);
        list.RemoveAll(a => a.StoredName == storedName);
        File.WriteAllText(GetMetaFile(episodeId), System.Text.Json.JsonSerializer.Serialize(list));
    }
}
