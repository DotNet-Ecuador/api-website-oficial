using System.Text.Json;

namespace DotNetEcuador.API.Services;

public class MessageService : IMessageService
{
    private readonly Dictionary<string, string> _messages;

    public MessageService(IWebHostEnvironment environment)
    {
        _messages = new Dictionary<string, string>();
        LoadMessages(environment);
    }

    private void LoadMessages(IWebHostEnvironment environment)
    {
        var filePath = Path.Combine(environment.ContentRootPath, "Resources", "messages.es.json");
        
        if (File.Exists(filePath))
        {
            var jsonContent = File.ReadAllText(filePath);
            var messages = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);
            
            if (messages != null)
            {
                foreach (var kvp in messages)
                {
                    _messages[kvp.Key] = kvp.Value;
                }
            }
        }
    }

    public string GetMessage(string key)
    {
        return _messages.TryGetValue(key, out var message) ? message : key;
    }

    public string GetMessage(string key, params object[] args)
    {
        var template = GetMessage(key);
        return args.Length > 0 ? string.Format(template, args) : template;
    }
}