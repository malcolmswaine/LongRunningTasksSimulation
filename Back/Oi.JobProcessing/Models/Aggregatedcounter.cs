﻿using System;
using System.Collections.Generic;

namespace OI.JobProcessing.Models;

public partial class Aggregatedcounter
{
    public long Id { get; set; }

    public string Key { get; set; }

    public long Value { get; set; }

    public DateTime? Expireat { get; set; }
}
