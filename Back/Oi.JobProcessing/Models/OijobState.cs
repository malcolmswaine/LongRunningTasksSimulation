﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OI.JobProcessing.Models;

public partial class OijobState
{
    [Key]
    public int JobStateId { get; set; }

    [Required] 
    public string JobStateName { get; set; }
}
