using Microsoft.AspNetCore.DataProtection;
using TicketBurst.Contracts;

namespace TicketBurst.ServiceInfra;

public static class ServiceDataProtection
{
    public static IDataProtector CreateProtector(this IDataProtectionProvider provider, DataProtectionPurpose purpose)
    {
        return provider.CreateProtector(purpose.ToString());
    }
}
