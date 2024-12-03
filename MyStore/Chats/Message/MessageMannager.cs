
using System.Collections.Concurrent;

namespace MyStore.Chats.Message
{
    public class MessageMannager : IMessageMannager

    {
        public struct MessageStruct(string message, bool isUser)
        {
            public string Message { get; } = message;
            public bool IsUser { get; } = isUser;
            public DateTime DateTime { get; } = DateTime.Now;
        }

        public readonly ConcurrentDictionary<string, IList<MessageStruct>> _messageManager = new();
        private readonly ConcurrentDictionary<string, bool> _adminConnections = new();

        public int UserCount => _messageManager.Count;
        public int AdminCount => _adminConnections.Count;

        public bool StartChatting(string key)
            => !_messageManager.TryGetValue(key, out var _) && _messageManager.TryAdd(key, []);

        public bool TryAddMessage(string key, string message, bool isUser = true)
        {
            var newMessage = new MessageStruct(message, isUser);
            if (_messageManager.TryGetValue(key, out var messageList))
            {
                messageList.Add(newMessage);
                return true;
            }
            else
            {
                return _messageManager.TryAdd(key, [newMessage]);
            }
        }

        public bool TryGetMessages(string key, out IList<MessageStruct> messages)
            => _messageManager.TryGetValue(key, out messages);

        public Dictionary<string, IList<MessageStruct>> GetMessages()
            => _messageManager.ToDictionary();

        public IEnumerable<string> TryGetAllConnection()
            => _messageManager.Select(x => x.Key);

        public bool StopChatting(string key)
            => _messageManager.TryRemove(key, out _);

        public IEnumerable<string> GetAdminConnection()
            => _adminConnections.Select(x => x.Key);

        public bool TryAddAdmin(string key)
            => _adminConnections.TryAdd(key, true);

        public bool TryRemoveAdmin(string key)
            => _adminConnections.TryRemove(key, out _);
    }
}
