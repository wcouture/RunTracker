namespace RunTracker.Models;

public enum FriendInviteStatus {
    Pending,
    Accepted,
    Rejected
}

public class FriendInvite {
    public int Id {get; set;}
    public int SenderId {get; set;}
    public int ReceiverId {get; set;}
    public FriendInviteStatus Status {get; set;} = FriendInviteStatus.Pending;
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
}