public class EventPoll
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public DateTime StartTime1 { get; set; }
    public DateTime EndTime1 { get; set; }
    public DateTime StartTime2 { get; set; }
    public DateTime EndTime2 { get; set; }
    public int VotesForTime1 { get; set; }
    public int VotesForTime2 { get; set; }
    public DateTime PollEndTime { get; set; }
    public string? UserId { get; set; }
    public ICollection<ApplicationUser> Participants { get; set; } = new List<ApplicationUser>();
}