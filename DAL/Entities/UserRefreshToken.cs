using System;
using System.Collections.Generic;

namespace SMS.Entities;

public partial class UserRefreshToken
{
    public int RefreshTokenId { get; set; }

    public int UserId { get; set; }

    public string TokenValue { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    public DateTime? RevokedOn { get; set; }

    public DateTime ExpiresOn { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresOn;
    public bool IsRevoked => RevokedOn != null;
    public bool IsActive => !IsRevoked && !IsExpired;

    public string? ReplacedByToken { get; set; }

    public virtual User User { get; set; } = null!;
}
