namespace MyCRM.Data.Models;

public class Person : BaseEntity
{
    public required string FirstName { get; set; }

    public string? LastName { get; set; }

    public ICollection<CompanyPerson> CompanyPeople { get; set; } = [];

    public ICollection<Correspondence> CorrespondenceRecords { get; set; } = [];

    public ICollection<Email> EmailAddresses { get; set; } = [];

    public ICollection<FollowUp> FollowUps { get; set; } = [];

    public ICollection<Note> Notes { get; set; } = [];

    public ICollection<Phone> PhoneNumbers { get; set; } = [];

    public ICollection<WebLink> WebLinks { get; set; } = [];
}
