using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OI.JobProcessing.Models;

public partial class Oijob
{
    [Key]
    public int JobId { get; set; }

    [Required]
    public string OriginalString { get; set; } = string.Empty;

    [Required]
    public string EncodedString { get; set; } = string.Empty;

    public string ReturnedData { get; set; } = string.Empty;

    [Required]
    public DateTime CreatedDateTime { get; set; }

    [Required]
    public DateTime UpdatedDateTime { get; set; }

    [Required]
    public int JobStateId { get; set; }
}
