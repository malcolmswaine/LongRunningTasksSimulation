﻿using System;
using System.Collections.Generic;

namespace OI.JobProcessing.Models;

public partial class Server
{
    public string Id { get; set; }

    public string Data { get; set; }

    public DateTime Lastheartbeat { get; set; }

    public int Updatecount { get; set; }
}
