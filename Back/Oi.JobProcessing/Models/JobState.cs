using System;
using System.Collections.Generic;

namespace OI.JobProcessing.Models;

public partial class JobState
{
    public int JobStateId { get; set; }

    public string JobStateName { get; set; } = string.Empty;
}
