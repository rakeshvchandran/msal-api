namespace msal_api
{
    public interface IServiceBusMessageHandler
    {
        Task SendMessageAsync(FileInfoMessage fileInfoMessage);
    }
}
