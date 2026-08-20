namespace Billing.Api.Exceptions;

public class InventoryUnavailableException : Exception
{
    private const string DefaultMessage =
        "The inventory service is temporarily unavailable. Try again.";

    public InventoryUnavailableException()
        : base(DefaultMessage)
    {
    }

    public InventoryUnavailableException(Exception innerException)
        : base(DefaultMessage, innerException)
    {
    }
}
