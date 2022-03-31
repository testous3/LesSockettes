using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SockettesServeur.Hubs
{
    public class ChatHub : Hub
    {
        static int _nbConnexion=0;

        public override Task OnConnectedAsync()
        {
            _nbConnexion++;
            SendSystemInfo(_nbConnexion);
            SendSystemMessage("SystemMessage", string.Format("Welcome"));
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            _nbConnexion--;
            SendSystemInfo(_nbConnexion);
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
