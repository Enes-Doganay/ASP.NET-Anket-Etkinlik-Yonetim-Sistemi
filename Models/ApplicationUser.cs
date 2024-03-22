using Microsoft.AspNetCore.Identity;
using Basics.Models;
public class ApplicationUser : IdentityUser
{
    public ICollection<EventPoll> EventPolls { get; set; } = new List<EventPoll>();
    public ICollection<Event> Events { get; set; } = new List<Event>();
}