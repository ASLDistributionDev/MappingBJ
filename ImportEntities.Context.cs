﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MappingBJ
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class ImportEntities : DbContext
    {
        public ImportEntities()
            : base("name=ImportEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<mmdestination> mmdestinations { get; set; }
        public virtual DbSet<mmraw> mmraws { get; set; }
        public virtual DbSet<mmref> mmrefs { get; set; }
        public virtual DbSet<Log> Logs { get; set; }
    }
}
