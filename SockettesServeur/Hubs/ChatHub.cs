using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Primitives;

namespace SockettesServeur.Hubs
{
    public class ChatHub : Hub
    {
        static int _nbConnexion=0;
        static Dictionary<string,string> _connectedUsers = new Dictionary<string, string>();


        public override Task OnConnectedAsync()
        {
            _nbConnexion++;
            SendSystemInfo(_nbConnexion);

            HttpContext httpContext = Context.GetHttpContext();
            string connexionId = Context.ConnectionId;
            StringValues username;
            httpContext.Request.Query.TryGetValue("username", out username);

            string message = "Welcome";

            if(httpContext.Request.Query.TryGetValue("username", out username) && !StringValues.IsNullOrEmpty(username))
            {
                message = string.Format("Welcome {0}", username.ToString());
                _connectedUsers.Add(connexionId,username);
            }


            SendSystemMessage("SystemMessage", message);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            _nbConnexion--;
            SendSystemInfo(_nbConnexion);

            _connectedUsers.Remove(Context.ConnectionId);

            return base.OnDisconnectedAsync(exception);
        }

        public Task SendMessage(string sender, string message)
        {
            Console.WriteLine(string.Format("SendMessage depuis : {0} ; message : {1}", sender, message));
            return Clients.All.SendAsync("ReceiveMessage", sender, message);
        }

        public Task SendSystemMessage(string sender, string message)
        {
            Console.WriteLine(string.Format("SendSystemMessage vers : {0} ; message : {1}", sender, message));
            return Clients.Caller.SendAsync("SystemMessage", sender, message);
        }

        public Task SendSystemInfo(int message)
        {
            return Clients.All.SendAsync("SystemInfo", message.ToString());
        }
    }
}
