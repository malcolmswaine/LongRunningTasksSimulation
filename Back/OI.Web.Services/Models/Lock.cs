using System;
using System.Collections.Generic;

namespace OI.Web.Services.Models;

public partial class Lock
{
    public string Resource { get; set; }

    public int Updatecount { get; set; }

    public DateTime? Acquired { get; set; }
}
