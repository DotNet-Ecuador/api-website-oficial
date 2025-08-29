namespace DotNetEcuador.API.Services;

public interface IMessageService
{
    string GetMessage(string key);
    string GetMessage(string key, params object[] args);
}