using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

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
        public virtual DbSet<AspNetRoleClaims> AspNetRoleClaims { get; set; }
        public virtual DbSet<AspNetRoles> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUserClaims> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogins> AspNetUserLogins { get; set; }
        public virtual DbSet<AspNetUserRoles> AspNetUserRoles { get; set; }
        public virtual DbSet<AspNetUserTokens> AspNetUserTokens { get; set; }
        public virtual DbSet<AspNetUsers> AspNetUsers { get; set; }
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
                optionsBuilder.UseSqlServer(Startup.CSConfig.GetConnectionString("DefaultConnection"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:DefaultSchema", "ronripp_CStat");

            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasIndex(e => e.BusinessId);

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

            modelBuilder.Entity<AspNetRoleClaims>(entity =>
            {
                entity.HasIndex(e => e.RoleId);
            });

            modelBuilder.Entity<AspNetRoles>(entity =>
            {
                entity.HasIndex(e => e.NormalizedName)
                    .HasName("RoleNameIndex")
                    .IsUnique()
                    .HasFilter("([NormalizedName] IS NOT NULL)");
            });

            modelBuilder.Entity<AspNetUserClaims>(entity =>
            {
                entity.HasIndex(e => e.UserId);
            });

            modelBuilder.Entity<AspNetUserLogins>(entity =>
            {
                entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

                entity.HasIndex(e => e.UserId);
            });

            modelBuilder.Entity<AspNetUserRoles>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId });

                entity.HasIndex(e => e.RoleId);
            });

            modelBuilder.Entity<AspNetUserTokens>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });
            });

            modelBuilder.Entity<AspNetUsers>(entity =>
            {
                entity.HasIndex(e => e.NormalizedEmail)
                    .HasName("EmailIndex");

                entity.HasIndex(e => e.NormalizedUserName)
                    .HasName("UserNameIndex")
                    .IsUnique()
                    .HasFilter("([NormalizedUserName] IS NOT NULL)");
            });

            modelBuilder.Entity<Attendance>(entity =>
            {
                entity.HasIndex(e => e.EventId);

                entity.HasIndex(e => e.MedicalId);

                entity.HasIndex(e => e.PersonId);

                entity.HasIndex(e => e.RegistrationId);

                entity.HasIndex(e => e.TransactionId);

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
                entity.HasIndex(e => e.AddressId);

                entity.HasIndex(e => e.PocId);

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
                entity.HasIndex(e => e.AddressId);

                entity.HasIndex(e => e.Alternate1Id);

                entity.HasIndex(e => e.Alternate2Id);

                entity.HasIndex(e => e.Alternate3Id);

                entity.HasIndex(e => e.Elder1Id);

                entity.HasIndex(e => e.Elder2Id);

                entity.HasIndex(e => e.Elder3Id);

                entity.HasIndex(e => e.Elder4Id);

                entity.HasIndex(e => e.Elder5Id);

                entity.HasIndex(e => e.SeniorMinisterId);

                entity.HasIndex(e => e.Trustee1Id);

                entity.HasIndex(e => e.Trustee2Id);

                entity.HasIndex(e => e.Trustee3Id);

                entity.HasIndex(e => e.YouthMinisterId);

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
                entity.HasIndex(e => e.ChurchId);

                entity.HasOne(d => d.Church)
                    .WithMany(p => p.Event)
                    .HasForeignKey(d => d.ChurchId)
                    .HasConstraintName("FK_Event_Church");
            });

            modelBuilder.Entity<Incident>(entity =>
            {
                entity.HasIndex(e => e.Persion3Id);

                entity.HasIndex(e => e.Person1Id);

                entity.HasIndex(e => e.Person2Id);

                entity.HasIndex(e => e.Person4Id);

                entity.HasIndex(e => e.Person5Id);

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
                entity.Property(e => e.Name).IsFixedLength();
            });

            modelBuilder.Entity<InventoryItem>(entity =>
            {
                entity.HasIndex(e => e.InventoryId);

                entity.HasIndex(e => e.ItemId);

                entity.HasIndex(e => e.OrderId);

                entity.HasIndex(e => e.PersonId);

                entity.HasOne(d => d.Buy1)
                    .WithMany(p => p.InventoryItemBuy1)
                    .HasForeignKey(d => d.Buy1Id)
                    .HasConstraintName("FK_InventoryItem_Transaction1");

                entity.HasOne(d => d.Buy2)
                    .WithMany(p => p.InventoryItemBuy2)
                    .HasForeignKey(d => d.Buy2Id)
                    .HasConstraintName("FK_InventoryItem_Transaction2");

                entity.HasOne(d => d.Buy3)
                    .WithMany(p => p.InventoryItemBuy3)
                    .HasForeignKey(d => d.Buy3Id)
                    .HasConstraintName("FK_InventoryItem_Transaction3");

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

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.InventoryItemOrder)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("FK_InventoryItem_Transaction");

                entity.HasOne(d => d.Person)
                    .WithMany(p => p.InventoryItem)
                    .HasForeignKey(d => d.PersonId)
                    .HasConstraintName("FK_InventoryItem_Person");
            });

            modelBuilder.Entity<Item>(entity =>
            {
                entity.HasIndex(e => e.MfgId);

                entity.Property(e => e.Name).IsFixedLength();

                entity.Property(e => e.Upc).IsFixedLength();

                entity.HasOne(d => d.Mfg)
                    .WithMany(p => p.Item)
                    .HasForeignKey(d => d.MfgId)
                    .HasConstraintName("FK_Item_Business");

                entity.HasOne(d => d.MfgNavigation)
                    .WithMany(p => p.Item)
                    .HasForeignKey(d => d.MfgId)
                    .HasConstraintName("FK_Item_Manufacturer");
            });

            modelBuilder.Entity<Manufacturer>(entity =>
            {
                entity.Property(e => e.Name).IsFixedLength();
            });

            modelBuilder.Entity<Medical>(entity =>
            {
                entity.HasIndex(e => e.EventId);

                entity.HasIndex(e => e.PersonId);

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
                entity.HasIndex(e => e.BusinessId);

                entity.HasIndex(e => e.ChurchId);

                entity.HasIndex(e => e.PersonId);

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

                entity.HasIndex(e => e.AddressId);

                entity.HasIndex(e => e.ChurchId);

                entity.HasIndex(e => e.Pg1PersonId);

                entity.HasIndex(e => e.Pg2PersonId);

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
                entity.HasIndex(e => e.PersonId);

                entity.HasOne(d => d.Person)
                    .WithMany(p => p.Position)
                    .HasForeignKey(d => d.PersonId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Position_Person");
            });

            modelBuilder.Entity<Registration>(entity =>
            {
                entity.HasIndex(e => e.EventId);

                entity.HasIndex(e => e.PersonId);

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
                entity.HasIndex(e => e.Blocking1Id);

                entity.HasIndex(e => e.Blocking2Id);

                entity.HasIndex(e => e.ChurchId);

                entity.HasIndex(e => e.PersonId);

                entity.HasIndex(e => e.Worker1Id);

                entity.HasIndex(e => e.Worker2Id);

                entity.HasIndex(e => e.Worker3Id);

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

                entity.HasOne(d => d.Event)
                    .WithMany(p => p.Task)
                    .HasForeignKey(d => d.EventId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Task_Event");

                entity.HasOne(d => d.ParentTask)
                    .WithMany(p => p.InverseParentTask)
                    .HasForeignKey(d => d.ParentTaskId)
                    .HasConstraintName("FK_Task_Task2");

                entity.HasOne(d => d.Person)
                    .WithMany(p => p.TaskPerson)
                    .HasForeignKey(d => d.PersonId)
                    .HasConstraintName("FK_Task_Person");

                entity.HasOne(d => d.Worker1)
                    .WithMany(p => p.TaskWorker1)
                    .HasForeignKey(d => d.Worker1Id)
                    .HasConstraintName("FK_Task_Person1");

                entity.HasOne(d => d.Worker2)
                    .WithMany(p => p.TaskWorker2)
                    .HasForeignKey(d => d.Worker2Id)
                    .HasConstraintName("FK_Task_Person2");

                entity.HasOne(d => d.Worker3)
                    .WithMany(p => p.TaskWorker3)
                    .HasForeignKey(d => d.Worker3Id)
                    .HasConstraintName("FK_Task_Person3");
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasIndex(e => e.BusinessId);

                entity.HasIndex(e => e.CcaAccountId);

                entity.HasIndex(e => e.CcaPersonId);

                entity.HasIndex(e => e.ChurchId);

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
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

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
