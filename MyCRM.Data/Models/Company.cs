using MyCRM.Data.Constants;

namespace MyCRM.Data.Models;

public class Company : BaseEntity
{
    public required string Name { get; set; }

    public Industry? Industry { get; set; }

    public string? Street { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Country { get; set; }

    public string? PostalCode { get; set; }

    public int? EmployeeCount { get; set; }

    public LeadStatus Status { get; set; } = LeadStatus.Lead;

    public LeadSource Source { get; set; } = LeadSource.Unknown;

    public ICollection<CompanyPerson> CompanyPeople { get; set; } = [];

    public ICollection<Correspondence> CorrespondenceRecords { get; set; } = [];

    public ICollection<Email> EmailAddresses { get; set; } = [];

    public ICollection<FollowUp> FollowUps { get; set; } = [];

    public ICollection<Note> Notes { get; set; } = [];

    public ICollection<Phone> PhoneNumbers { get; set; } = [];

    public ICollection<WebLink> WebLinks { get; set; } = [];
}
