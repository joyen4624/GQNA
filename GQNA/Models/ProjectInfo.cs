using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GQNA.Models
{
    public class ProjectInfo
    {
        public int Id { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal TotalContributed { get; set; }
        public int PercentContributed { get; set; }
        public int RemainingDays { get; set; }
    }
}