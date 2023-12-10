using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GQNA.Models
{
    public class ProjectView
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal TotalContributed { get; set; }
        public decimal? TotalAmount { get; set; }
        public object PercentContributed { get; set; }
        public int RemainingDays { get; set; }
        public int TotalSL { get; set; }
        public string back { get; internal set; }
    }
}