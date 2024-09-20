using System;
using System.Collections.Generic;

namespace OI.Web.Services.Models;

public partial class JobState
{
    public int JobStateId { get; set; }

    public string JobStateName { get; set; } = string.Empty;
}
