using System;

namespace KMB_Coin.Services.Interfaces
{
    public interface IClock
    {
        bool IsPrecise { get; }
        DateTime UtcNow { get; }
    }
}