using Microsoft.EntityFrameworkCore;
using MyCRM.Data.Models;

namespace MyCRM.Data;

public class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
    {
    }

    public virtual DbSet<Company> Companies => Set<Company>();
    public virtual DbSet<Person> People => Set<Person>();
    public virtual DbSet<CompanyPerson> CompanyPeople => Set<CompanyPerson>();
    public virtual DbSet<CompanyRole> CompanyRoles => Set<CompanyRole>();
    public virtual DbSet<CompanyPersonRole> CompanyPersonRoles => Set<CompanyPersonRole>();
    public virtual DbSet<Email> Emails => Set<Email>();
    public virtual DbSet<Phone> Phones => Set<Phone>();
    public virtual DbSet<WebLink> WebLinks => Set<WebLink>();
    public virtual DbSet<Correspondence> Correspondences => Set<Correspondence>();
    public virtual DbSet<Note> Notes => Set<Note>();
    public virtual DbSet<FollowUp> FollowUps => Set<FollowUp>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CompanyPerson>(entity =>
        {
            entity.HasOne(cp => cp.Company)
                .WithMany(c => c.CompanyPeople)
                .HasForeignKey(cp => cp.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(cp => cp.Person)
                .WithMany(p => p.CompanyPeople)
                .HasForeignKey(cp => cp.PersonId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(cp => new { cp.CompanyId, cp.PersonId }).IsUnique();
        });

        modelBuilder.Entity<CompanyPersonRole>(entity =>
        {
            entity.HasOne(cpr => cpr.CompanyPerson)
                .WithMany(cp => cp.CompanyPersonRoles)
                .HasForeignKey(cpr => cpr.CompanyPersonId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(cpr => cpr.CompanyRole)
                .WithMany(cr => cr.CompanyPersonRoles)
                .HasForeignKey(cpr => cpr.CompanyRoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(cpr => new { cpr.CompanyPersonId, cpr.CompanyRoleId }).IsUnique();
        });

        modelBuilder.Entity<CompanyRole>()
            .HasIndex(cr => cr.Name)
            .IsUnique();

        modelBuilder.Entity<Company>()
            .HasIndex(c => c.Status);

        modelBuilder.Entity<Email>(entity =>
        {
            entity.HasOne(e => e.Company)
                .WithMany(c => c.EmailAddresses)
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Person)
                .WithMany(p => p.EmailAddresses)
                .HasForeignKey(e => e.PersonId)
                .OnDelete(DeleteBehavior.SetNull);

            // At most one primary email per company and per person.
            entity.HasIndex(e => e.CompanyId, "IX_Emails_CompanyId_Primary")
                .IsUnique()
                .HasFilter("\"IsPrimary\" AND \"CompanyId\" IS NOT NULL");

            entity.HasIndex(e => e.PersonId, "IX_Emails_PersonId_Primary")
                .IsUnique()
                .HasFilter("\"IsPrimary\" AND \"PersonId\" IS NOT NULL");
        });

        modelBuilder.Entity<Phone>(entity =>
        {
            entity.HasOne(ph => ph.Company)
                .WithMany(c => c.PhoneNumbers)
                .HasForeignKey(ph => ph.CompanyId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(ph => ph.Person)
                .WithMany(p => p.PhoneNumbers)
                .HasForeignKey(ph => ph.PersonId)
                .OnDelete(DeleteBehavior.SetNull);

            // At most one primary phone per company and per person.
            entity.HasIndex(ph => ph.CompanyId, "IX_Phones_CompanyId_Primary")
                .IsUnique()
                .HasFilter("\"IsPrimary\" AND \"CompanyId\" IS NOT NULL");

            entity.HasIndex(ph => ph.PersonId, "IX_Phones_PersonId_Primary")
                .IsUnique()
                .HasFilter("\"IsPrimary\" AND \"PersonId\" IS NOT NULL");
        });

        modelBuilder.Entity<WebLink>(entity =>
        {
            entity.HasOne(w => w.Company)
                .WithMany(c => c.WebLinks)
                .HasForeignKey(w => w.CompanyId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(w => w.Person)
                .WithMany(p => p.WebLinks)
                .HasForeignKey(w => w.PersonId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Correspondence>(entity =>
        {
            entity.HasOne(cr => cr.Company)
                .WithMany(c => c.CorrespondenceRecords)
                .HasForeignKey(cr => cr.CompanyId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(cr => cr.Person)
                .WithMany(p => p.CorrespondenceRecords)
                .HasForeignKey(cr => cr.PersonId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Note>(entity =>
        {
            entity.HasOne(n => n.Company)
                .WithMany(c => c.Notes)
                .HasForeignKey(n => n.CompanyId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(n => n.Person)
                .WithMany(p => p.Notes)
                .HasForeignKey(n => n.PersonId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<FollowUp>(entity =>
        {
            entity.HasOne(f => f.Company)
                .WithMany(c => c.FollowUps)
                .HasForeignKey(f => f.CompanyId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(f => f.Person)
                .WithMany(p => p.FollowUps)
                .HasForeignKey(f => f.PersonId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(f => new { f.IsDone, f.DueOn });
        });
    }
}
