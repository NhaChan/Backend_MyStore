using static MyStore.Chats.Message.MessageMannager;

namespace MyStore.Chats.Message
{
    public interface IMessageMannager
    {
       int UserCount { get; }
        int AdminCount { get; }
        bool StartChatting(string key);
        bool TryAddMessage(string key, string message, bool isUser = true);
        bool TryGetMessages(string key, out IList<MessageStruct> messages);
        Dictionary<string, IList<MessageStruct>> GetMessages();
        IEnumerable<string> TryGetAllConnection();
        bool StopChatting(string key);

        IEnumerable<string> GetAdminConnection();
        bool TryAddAdmin(string key);
        bool TryRemoveAdmin(string key);
    }
}
