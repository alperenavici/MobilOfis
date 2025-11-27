using System;
using System.Collections.Generic;

namespace MobilOfis.Web.Models.DTOs;

public class BulkApproveDto
{
    public List<Guid> LeaveIds { get; set; } = new();
}
