//------------------------------------------------------------------------------
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
    
    public partial class tbl_DT
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_DT()
        {
            this.Projects = new HashSet<Project>();
        }
    
        public int id_DT { get; set; }
        public string name_DT { get; set; }
        public string avatar_DT { get; set; }
        public string background_DT { get; set; }
        public string back_slide { get; set; }
        public string txt_DT { get; set; }
        public string des_DT { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Project> Projects { get; set; }
    }
}
