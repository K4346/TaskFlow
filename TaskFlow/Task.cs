using System;
using System.Collections.Generic;

namespace TaskFlow;

public partial class Task
{
    public int Taskid { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int Statusid { get; set; }

    public int Categoryid { get; set; }

    public DateTime? Createdat { get; set; }

    public DateOnly? Deadline { get; set; }

    public int? Assignedto { get; set; }

    public int? Priority { get; set; }

    public virtual User? AssignedtoNavigation { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual Priority? PriorityNavigation { get; set; }

    public virtual Status Status { get; set; } = null!;
}
