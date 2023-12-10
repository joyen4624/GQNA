﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GQNA.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

    public partial class Project
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Project()
        {
            this.tbl_get_CD = new HashSet<tbl_get_CD>();
            this.tbl_img = new HashSet<tbl_img>();
            this.tbl_QG = new HashSet<tbl_QG>();
        }
    
        public int id_project { get; set; }
        public string name_project { get; set; }
        public string img_project { get; set; }
        [AllowHtml]
        public string des_project { get; set; }
        public Nullable<System.DateTime> project_time_start { get; set; }
        public Nullable<System.DateTime> project_time_end { get; set; }
        public Nullable<int> id_DT { get; set; }
        public Nullable<decimal> project_amount { get; set; }
        public string status { get; set; }
    
        public virtual tbl_DT tbl_DT { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_get_CD> tbl_get_CD { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_img> tbl_img { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_QG> tbl_QG { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal TotalContributed { get; set; }
        public int PercentContributed { get; set; }
        public int RemainingDays { get; set; }
        public int totalSL { get; set; }
        [Display(Name = "Chọn chủ đề")]
        public List<int> SelectedTopics { get; set; }
    }
}
