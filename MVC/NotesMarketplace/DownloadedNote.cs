//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NotesMarketplace
{
    using System;
    using System.Collections.Generic;
    
    public partial class DownloadedNote
    {
        public int ID { get; set; }
        public int F_K_Note { get; set; }
        public int F_K_User_Seller { get; set; }
        public int F_K_User_Buyer { get; set; }
        public bool IsApproved { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<int> ModifedBy { get; set; }
        public bool IsActive { get; set; }
        public int SellPrice { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string Attachment { get; set; }
    
        public virtual User User { get; set; }
        public virtual User User1 { get; set; }
    }
}
