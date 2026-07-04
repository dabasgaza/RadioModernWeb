// ============================================================
// PermissionViewModel — الصلاحية
// ============================================================
// المسؤولية: تعريف الصلاحية.
// ============================================================
using System.ComponentModel;

namespace DataAccess.DTOs
{
    /// <summary>
    /// صنف الصلاحية.
    /// </summary>
    public class PermissionViewModel : INotifyPropertyChanged
    {
        public int PermissionId { get; set; }
        public string? SystemName { get; set; }
        public required string DisplayName { get; set; }
        public required string Module { get; set; }
        public bool IsAssigned { get; set; } = false;

#pragma warning disable CS0067
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067
    }
}
