namespace Give_AID.Services;

// Admin ka Live Chat page khula ho to woh har kuch second baad server ko batata hai.
// Aakhri 2 minute mein admin dikha ho to website par chat "Online" dikhti hai.
// Singleton hai, is liye value memory mein rehti hai (app restart par reset).
public class ChatPresence
{
    private static readonly TimeSpan OnlineWindow = TimeSpan.FromMinutes(2);
    private DateTime _lastAdminSeen = DateTime.MinValue;

    public void AdminSeen() => _lastAdminSeen = DateTime.Now;

    public bool IsAdminOnline => DateTime.Now - _lastAdminSeen < OnlineWindow;
}
