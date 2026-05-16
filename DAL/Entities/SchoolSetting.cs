using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class SchoolSetting
{
    public int SettingId { get; set; }

    public string SchoolName { get; set; } = null!;

    public string? SchoolLogo { get; set; }

    public DateTime? LastUpdated { get; set; }
}
