namespace Domain.Entities;

public partial class Role
{
    public int RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
}
