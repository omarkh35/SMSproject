using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class User
{
    public int UserId { get; set; }

    public int PersonId { get; set; }

    public int UserRoleId { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public string? Email { get; set; }

    public string? HashPassword { get; set; }

    public string? AccountNumber { get; set; }

    public virtual Person Person { get; set; } = null!;

    public virtual ICollection<UserRefreshToken> UserRefreshTokens { get; set; } = new List<UserRefreshToken>();

    public virtual Role UserRole { get; set; } = null!;
}
