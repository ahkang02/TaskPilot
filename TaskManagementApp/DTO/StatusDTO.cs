using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaskManagementApp.DTO
{
    public class StatusDTO
    {
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}