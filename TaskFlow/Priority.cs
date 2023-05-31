using System;
using System.Collections.Generic;

namespace TaskFlow;

public partial class Priority
{
    public int Priorityid { get; set; }

    public string Priorityname { get; set; } = null!;

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
