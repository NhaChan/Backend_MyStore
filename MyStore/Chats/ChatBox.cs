namespace MyStore.Chats;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MyStore.Chats.Message;
using System;
using System.Security.Claims;
using static MyStore.Chats.Message.MessageMannager;


public class MessageReponse
{
    public string ConnectionId { get; set; }
    public IEnumerable<MessageStruct> Messages { get; set; }
}
public class ChatBox(IMessageMannager messageMannager) : Hub
{
    private readonly IMessageMannager _messageMannager = messageMannager;

    public async Task SenToAdmin(string message)
    {
        _messageMannager.TryAddMessage(Context.ConnectionId, message);
        await Clients.Group("AdminGroup").SendAsync("onAdmin", Context.ConnectionId, message);
    }

    public bool GetAdminOnline()
        => _messageMannager.AdminCount > 0;

    [Authorize(Roles = "Admin")]
    public async Task SendToUser(string connectionId, string message)
    {
        _messageMannager.TryAddMessage(connectionId, message, false);
        await Clients.Client(connectionId).SendAsync("onUser", message);
    }

    [Authorize(Roles = "Admin")]
    public IEnumerable<string> GetUserConnections()
        => _messageMannager.TryGetAllConnection();

    [Authorize(Roles = "Admin")]
    public IEnumerable<MessageReponse> GetMessages()
        => _messageMannager.GetMessages().Select(item => new MessageReponse
        {
            ConnectionId = item.Key,
            Messages = item.Value
        });

    [Authorize(Roles = "Admin")]
    public async Task CloseChat(string connectionId)
    {
        _messageMannager.StopChatting(connectionId);
        await Clients.Client(connectionId).SendAsync("CloseChat", "CLOSE_CHAT");
    }

    public override async Task OnConnectedAsync()
    {
        var roles = Context.User?.FindAll(ClaimTypes.Role).Select(e => e.Value);
        string adminGroup = "AdminGroup";

        if (roles != null && roles.Contains("Admin"))
        {
            _messageMannager.TryAddAdmin(Context.ConnectionId);
            Console.WriteLine("Connected: " + Context.ConnectionId + " " + _messageMannager.AdminCount);
            await Groups.AddToGroupAsync(Context.ConnectionId, adminGroup);
        }
        else
        {
            _messageMannager.StartChatting(Context.ConnectionId);
            await Clients.Group(adminGroup).SendAsync("USER_CONNECT", Context.ConnectionId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var roles = Context.User?.FindAll(ClaimTypes.Role).Select(e => e.Value);
        string adminGroup = "AdminGroup";

        if (roles != null && roles.Contains("Admin"))
        {
            _messageMannager.TryAddAdmin(Context.ConnectionId);
            Console.WriteLine("Connected: " + Context.ConnectionId + " " + _messageMannager.AdminCount);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, adminGroup);
        }
        else
        {
            _messageMannager.StartChatting(Context.ConnectionId);
            await Clients.Group(adminGroup).SendAsync("USER_DISCONNECT", Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

}
