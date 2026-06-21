namespace Domain.Models;

public class Role : BaseEntity
{
    public int RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public string? RoleDescription { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}