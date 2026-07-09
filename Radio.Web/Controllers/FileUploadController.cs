using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Radio.Web.Services;

namespace Radio.Web.Controllers;

[Authorize]
[Route("Upload")]
public class FileUploadController(IEpisodeAttachmentService attachments, ICurrentUserService user) : Controller
{
    [HttpPost]
    [Route("Episode/{episodeId}")]
    public async Task<IActionResult> Upload(int episodeId, List<IFormFile> files)
    {
        var uploadedBy = user.ToUserSession()?.FullName ?? "غير معروف";
        var results = new List<EpisodeAttachmentInfo>();

        foreach (var file in files)
        {
            var info = await attachments.UploadAsync(episodeId, file, uploadedBy, HttpContext.RequestAborted);
            results.Add(info);
        }

        TempData["Success"] = $"تم رفع {results.Count} ملف بنجاح";
        return Redirect(Request.Headers["Referer"].ToString());
    }

    [HttpGet]
    [Route("Episode/{episodeId}")]
    public IActionResult Attachments(int episodeId)
    {
        var list = attachments.GetAttachments(episodeId);
        return Json(list);
    }

    [HttpPost]
    [Route("Delete/{episodeId}/{storedName}")]
    public IActionResult Delete(int episodeId, string storedName)
    {
        attachments.DeleteAsync(episodeId, storedName);
        TempData["Success"] = "تم حذف الملف";
        return Redirect(Request.Headers["Referer"].ToString());
    }
}
