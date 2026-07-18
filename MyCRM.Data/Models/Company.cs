using System.ComponentModel.DataAnnotations.Schema;
using MyCRM.Data.Constants;

namespace MyCRM.Data.Models;

[Table("companies")]
public class Company : BaseEntity
{
    [Column("name")]
    public required string Name { get; set; }

    [Column("industry")]
    public Industry? Industry { get; set; }

    [Column("street")]
    public string? Street { get; set; }

    [Column("city")]
    public string? City { get; set; }

    [Column("state")]
    public string? State { get; set; }

    [Column("country")]
    public string? Country { get; set; }

    [Column("postal_code")]
    public string? PostalCode { get; set; }

    [Column("employee_count")]
    public int? EmployeeCount { get; set; }

    [Column("status")]
    public LeadStatus Status { get; set; } = LeadStatus.Lead;

    [Column("source")]
    public LeadSource Source { get; set; } = LeadSource.Unknown;

    public ICollection<CompanyPerson> CompanyPeople { get; set; } = [];

    public ICollection<Correspondence> CorrespondenceRecords { get; set; } = [];

    public ICollection<Email> EmailAddresses { get; set; } = [];

    public ICollection<FollowUp> FollowUps { get; set; } = [];

    public ICollection<Note> Notes { get; set; } = [];

    public ICollection<Phone> PhoneNumbers { get; set; } = [];

    public ICollection<WebLink> WebLinks { get; set; } = [];
}
