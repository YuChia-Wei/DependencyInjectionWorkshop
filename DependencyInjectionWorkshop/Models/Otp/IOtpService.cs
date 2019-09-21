namespace DependencyInjectionWorkshop.Models.Otp
{
    public interface IOtpService
    {
        string GetCurrentOtp(string userAccount);
    }
}