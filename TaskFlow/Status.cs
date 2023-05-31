using System;
using System.Collections.Generic;

namespace TaskFlow;

public partial class Status
{
    public int Statusid { get; set; }

    public string Statusname { get; set; } = null!;

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
