using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CStat.Models
{
    public partial class CStatContext : DbContext
    {
        public CStatContext()
        {
        }

        public CStatContext(DbContextOptions<CStatContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Account> Account { get; set; }
        public virtual DbSet<Address> Address { get; set; }
        public virtual DbSet<Attendance> Attendance { get; set; }
        public virtual DbSet<Business> Business { get; set; }
        public virtual DbSet<Church> Church { get; set; }
        public virtual DbSet<Event> Event { get; set; }
        public virtual DbSet<Incident> Incident { get; set; }
        public virtual DbSet<Inventory> Inventory { get; set; }
        public virtual DbSet<InventoryItem> InventoryItem { get; set; }
        public virtual DbSet<Item> Item { get; set; }
        public virtual DbSet<Manufacturer> Manufacturer { get; set; }
        public virtual DbSet<Medical> Medical { get; set; }
        public virtual DbSet<Operations> Operations { get; set; }
        public virtual DbSet<Person> Person { get; set; }
        public virtual DbSet<Position> Position { get; set; }
        public virtual DbSet<Registration> Registration { get; set; }
        public virtual DbSet<Task> Task { get; set; }
        public virtual DbSet<Transaction> Transaction { get; set; }
        public virtual DbSet<TransactionItems> TransactionItems { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Data Source=RONI7;Initial Catalog=CCA;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasOne(d => d.Business)
                    .WithMany(p => p.Account)
                    .HasForeignKey(d => d.BusinessId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Account_Business");
            });

            modelBuilder.Entity<Address>(entity =>
            {
                entity.HasComment("Address");

                entity.Property(e => e.Country).IsFixedLength();
            });

            modelBuilder.Entity<Attendance>(entity =>
            {
                entity.HasOne(d => d.Event)
                    .WithMany(p => p.Attendance)
                    .HasForeignKey(d => d.EventId)
                    .HasConstraintName("FK_Attendance_Event");

                entity.HasOne(d => d.Medical)
                    .WithMany(p => p.Attendance)
                    .HasForeignKey(d => d.MedicalId)
                    .HasConstraintName("FK_Attendance_Medical");

                entity.HasOne(d => d.Person)
                    .WithMany(p => p.Attendance)
                    .HasForeignKey(d => d.PersonId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Attendance_Person");

                entity.HasOne(d => d.Registration)
                    .WithMany(p => p.Attendance)
                    .HasForeignKey(d => d.RegistrationId)
                    .HasConstraintName("FK_Attendance_Registration");

                entity.HasOne(d => d.Transaction)
                    .WithMany(p => p.Attendance)
                    .HasForeignKey(d => d.TransactionId)
                    .HasConstraintName("FK_Attendance_Transaction");
            });

            modelBuilder.Entity<Business>(entity =>
            {
                entity.Property(e => e.ApiLink).IsFixedLength();

                entity.Property(e => e.UserLink).IsFixedLength();

                entity.HasOne(d => d.Address)
                    .WithMany(p => p.Business)
                    .HasForeignKey(d => d.AddressId)
                    .HasConstraintName("FK_Business_Address");

                entity.HasOne(d => d.Poc)
                    .WithMany(p => p.Business)
                    .HasForeignKey(d => d.PocId)
                    .HasConstraintName("FK_Business_Person");
            });

            modelBuilder.Entity<Church>(entity =>
            {
                entity.HasOne(d => d.Address)
                    .WithMany(p => p.Church)
                    .HasForeignKey(d => d.AddressId)
                    .HasConstraintName("FK_Church_Address");

                entity.HasOne(d => d.Alternate1)
                    .WithMany(p => p.ChurchAlternate1)
                    .HasForeignKey(d => d.Alternate1Id)
                    .HasConstraintName("FK_Church_Person5");

                entity.HasOne(d => d.Alternate2)
                    .WithMany(p => p.ChurchAlternate2)
                    .HasForeignKey(d => d.Alternate2Id)
                    .HasConstraintName("FK_Church_Person6");

                entity.HasOne(d => d.Alternate3)
                    .WithMany(p => p.ChurchAlternate3)
                    .HasForeignKey(d => d.Alternate3Id)
                    .HasConstraintName("FK_Church_Person7");

                entity.HasOne(d => d.Elder1)
                    .WithMany(p => p.ChurchElder1)
                    .HasForeignKey(d => d.Elder1Id)
                    .HasConstraintName("FK_Church_Person8");

                entity.HasOne(d => d.Elder2)
                    .WithMany(p => p.ChurchElder2)
                    .HasForeignKey(d => d.Elder2Id)
                    .HasConstraintName("FK_Church_Person9");

                entity.HasOne(d => d.Elder3)
                    .WithMany(p => p.ChurchElder3)
                    .HasForeignKey(d => d.Elder3Id)
                    .HasConstraintName("FK_Church_Person10");

                entity.HasOne(d => d.Elder4)
                    .WithMany(p => p.ChurchElder4)
                    .HasForeignKey(d => d.Elder4Id)
                    .HasConstraintName("FK_Church_Person11");

                entity.HasOne(d => d.Elder5)
                    .WithMany(p => p.ChurchElder5)
                    .HasForeignKey(d => d.Elder5Id)
                    .HasConstraintName("FK_Church_Person12");

                entity.HasOne(d => d.SeniorMinister)
                    .WithMany(p => p.ChurchSeniorMinister)
                    .HasForeignKey(d => d.SeniorMinisterId)
                    .HasConstraintName("FK_Church_Person");

                entity.HasOne(d => d.Trustee1)
                    .WithMany(p => p.ChurchTrustee1)
                    .HasForeignKey(d => d.Trustee1Id)
                    .HasConstraintName("FK_Church_Person2");

                entity.HasOne(d => d.Trustee2)
                    .WithMany(p => p.ChurchTrustee2)
                    .HasForeignKey(d => d.Trustee2Id)
                    .HasConstraintName("FK_Church_Person3");

                entity.HasOne(d => d.Trustee3)
                    .WithMany(p => p.ChurchTrustee3)
                    .HasForeignKey(d => d.Trustee3Id)
                    .HasConstraintName("FK_Church_Person4");

                entity.HasOne(d => d.YouthMinister)
                    .WithMany(p => p.ChurchYouthMinister)
                    .HasForeignKey(d => d.YouthMinisterId)
                    .HasConstraintName("FK_Church_Person1");
            });

            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasOne(d => d.Church)
                    .WithMany(p => p.Event)
                    .HasForeignKey(d => d.ChurchId)
                    .HasConstraintName("FK_Event_Church");
            });

            modelBuilder.Entity<Incident>(entity =>
            {
                entity.HasOne(d => d.Persion3)
                    .WithMany(p => p.IncidentPersion3)
                    .HasForeignKey(d => d.Persion3Id)
                    .HasConstraintName("FK_Incident_Person2");

                entity.HasOne(d => d.Person1)
                    .WithMany(p => p.IncidentPerson1)
                    .HasForeignKey(d => d.Person1Id)
                    .HasConstraintName("FK_Incident_Person");

                entity.HasOne(d => d.Person2)
                    .WithMany(p => p.IncidentPerson2)
                    .HasForeignKey(d => d.Person2Id)
                    .HasConstraintName("FK_Incident_Person1");

                entity.HasOne(d => d.Person4)
                    .WithMany(p => p.IncidentPerson4)
                    .HasForeignKey(d => d.Person4Id)
                    .HasConstraintName("FK_Incident_Person3");

                entity.HasOne(d => d.Person5)
                    .WithMany(p => p.IncidentPerson5)
                    .HasForeignKey(d => d.Person5Id)
                    .HasConstraintName("FK_Incident_Person4");
            });

            modelBuilder.Entity<Inventory>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name).IsFixedLength();
            });

            modelBuilder.Entity<InventoryItem>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.Inventory)
                    .WithMany(p => p.InventoryItem)
                    .HasForeignKey(d => d.InventoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_InventoryItem_Inventory");

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.InventoryItem)
                    .HasForeignKey(d => d.ItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_InventoryItem_Item");
            });

            modelBuilder.Entity<Item>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name).IsFixedLength();

                entity.Property(e => e.Upc).IsFixedLength();

                entity.HasOne(d => d.Mfg)
                    .WithMany(p => p.Item)
                    .HasForeignKey(d => d.MfgId)
                    .HasConstraintName("FK_Item_Business");
            });

            modelBuilder.Entity<Manufacturer>(entity =>
            {
                entity.HasNoKey();

                entity.Property(e => e.Name).IsFixedLength();
            });

            modelBuilder.Entity<Medical>(entity =>
            {
                entity.HasOne(d => d.Event)
                    .WithMany(p => p.Medical)
                    .HasForeignKey(d => d.EventId)
                    .HasConstraintName("FK_Medical_Event");

                entity.HasOne(d => d.Person)
                    .WithMany(p => p.Medical)
                    .HasForeignKey(d => d.PersonId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Medical_Person");
            });

            modelBuilder.Entity<Operations>(entity =>
            {
                entity.HasOne(d => d.Business)
                    .WithMany(p => p.Operations)
                    .HasForeignKey(d => d.BusinessId)
                    .HasConstraintName("FK_Operations_Business");

                entity.HasOne(d => d.Church)
                    .WithMany(p => p.Operations)
                    .HasForeignKey(d => d.ChurchId)
                    .HasConstraintName("FK_Operations_Church");

                entity.HasOne(d => d.Person)
                    .WithMany(p => p.Operations)
                    .HasForeignKey(d => d.PersonId)
                    .HasConstraintName("FK_Operations_Person");
            });

            modelBuilder.Entity<Person>(entity =>
            {
                entity.HasComment("Person");

                entity.HasOne(d => d.Address)
                    .WithMany(p => p.Person)
                    .HasForeignKey(d => d.AddressId)
                    .HasConstraintName("FK_Person_Address");

                entity.HasOne(d => d.Church)
                    .WithMany(p => p.Person)
                    .HasForeignKey(d => d.ChurchId)
                    .HasConstraintName("FK_Person_Church");

                entity.HasOne(d => d.Pg1Person)
                    .WithMany(p => p.InversePg1Person)
                    .HasForeignKey(d => d.Pg1PersonId)
                    .HasConstraintName("FK_Person_Person1");

                entity.HasOne(d => d.Pg2Person)
                    .WithMany(p => p.InversePg2Person)
                    .HasForeignKey(d => d.Pg2PersonId)
                    .HasConstraintName("FK_Person_Person2");
            });

            modelBuilder.Entity<Position>(entity =>
            {
                entity.HasOne(d => d.Person)
                    .WithMany(p => p.Position)
                    .HasForeignKey(d => d.PersonId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Position_Person");
            });

            modelBuilder.Entity<Registration>(entity =>
            {
                entity.Property(e => e.TShirtSize).IsFixedLength();

                entity.HasOne(d => d.Event)
                    .WithMany(p => p.Registration)
                    .HasForeignKey(d => d.EventId)
                    .HasConstraintName("FK_Registration_Event");

                entity.HasOne(d => d.Person)
                    .WithMany(p => p.Registration)
                    .HasForeignKey(d => d.PersonId)
                    .HasConstraintName("FK_Registration_Person");
            });

            modelBuilder.Entity<Task>(entity =>
            {
                entity.HasOne(d => d.Blocking1)
                    .WithMany(p => p.InverseBlocking1)
                    .HasForeignKey(d => d.Blocking1Id)
                    .HasConstraintName("FK_Task_Task1");

                entity.HasOne(d => d.Blocking2)
                    .WithMany(p => p.InverseBlocking2)
                    .HasForeignKey(d => d.Blocking2Id)
                    .HasConstraintName("FK_Task_Task");

                entity.HasOne(d => d.Church)
                    .WithMany(p => p.Task)
                    .HasForeignKey(d => d.ChurchId)
                    .HasConstraintName("FK_Task_Church");

                entity.HasOne(d => d.Person)
                    .WithMany(p => p.Task)
                    .HasForeignKey(d => d.PersonId)
                    .HasConstraintName("FK_Task_Person");
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.Property(e => e.InvoiceId).IsFixedLength();

                entity.Property(e => e.PaymentNumber).IsFixedLength();

                entity.HasOne(d => d.Business)
                    .WithMany(p => p.Transaction)
                    .HasForeignKey(d => d.BusinessId)
                    .HasConstraintName("FK_Transaction_Business");

                entity.HasOne(d => d.CcaAccount)
                    .WithMany(p => p.Transaction)
                    .HasForeignKey(d => d.CcaAccountId)
                    .HasConstraintName("FK_Transaction_Account");

                entity.HasOne(d => d.CcaPerson)
                    .WithMany(p => p.Transaction)
                    .HasForeignKey(d => d.CcaPersonId)
                    .HasConstraintName("FK_Transaction_Person");

                entity.HasOne(d => d.Church)
                    .WithMany(p => p.Transaction)
                    .HasForeignKey(d => d.ChurchId)
                    .HasConstraintName("FK_Transaction_Church");
            });

            modelBuilder.Entity<TransactionItems>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.IdNavigation)
                    .WithOne(p => p.TransactionItems)
                    .HasForeignKey<TransactionItems>(d => d.Id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TransactionItems_Item");

                entity.HasOne(d => d.Id1)
                    .WithOne(p => p.TransactionItems)
                    .HasForeignKey<TransactionItems>(d => d.Id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TransactionItems_Transaction");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
