namespace eDom.Core.Entities;

public class UserTokenState
{
    public int UserId { get; set; }
    public DateTime? InvalidBeforeUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
