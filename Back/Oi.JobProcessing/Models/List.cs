﻿using System;
using System.Collections.Generic;

namespace OI.JobProcessing.Models;

public partial class List
{
    public long Id { get; set; }

    public string Key { get; set; }

    public string Value { get; set; }

    public DateTime? Expireat { get; set; }

    public int Updatecount { get; set; }
}
