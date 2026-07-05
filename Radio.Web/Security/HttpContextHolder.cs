using Microsoft.AspNetCore.Http;
using System.Threading;

namespace Radio.Web.Security
{
    /// <summary>
    /// موفر لحفظ سياق الطلب الحالي (HttpContext) في خيط المعالجة المحلي لتسهيل استدعائه من الميثودز الاستاتيكية.
    /// </summary>
    public static class HttpContextHolder
    {
        private static readonly AsyncLocal<HttpContext?> _current = new();

        public static HttpContext? Current
        {
            get => _current.Value;
            set => _current.Value = value;
        }
    }
}
