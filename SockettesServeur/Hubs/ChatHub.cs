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
        static int _nbConnexion =0;
        static Dictionary<string,string> _connectedUsers = new Dictionary<string, string>();

        /// <summary>
        /// Méthode appelée à chaque connexion
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Méthode appelée après chaque déconnexion
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public override Task OnDisconnectedAsync(Exception exception)
        {
            _nbConnexion--;
            SendSystemInfo(_nbConnexion);

            _connectedUsers.Remove(Context.ConnectionId);

            return base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Tache invoquée par le client lors de l'envoi sur le canal "SendMessage"
        ///  message envoyé à tous les clients
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task SendMessage(string sender, string message)
        {
            Console.WriteLine(string.Format("SendMessage depuis : {0} ; message : {1}", sender, message));
            return Clients.All.SendAsync("ReceiveMessage", sender, message);
        }

        /// <summary>
        /// Tache écoutée par le client appelant pour lui souhaiter la bienvenue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task SendSystemMessage(string sender, string message)
        {
            Console.WriteLine(string.Format("SendSystemMessage vers : {0} ; message : {1}", sender, message));
            return Clients.Caller.SendAsync("SystemMessage", sender, message);
        }

        /// <summary>
        /// Tache écoutée par les clients pour recevoir le nombre de clients connectés
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task SendSystemInfo(int message)
        {
            return Clients.All.SendAsync("SystemInfo", message.ToString());
        }
    }
}
