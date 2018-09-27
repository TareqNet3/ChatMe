using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatMeService.Data;
using ChatMeService.Models;
using ChatMeService.Models.Entities;
using ChatMeService.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace ChatMeService.Hubs
{
    // TODO: What is default Javascript SignalR Client AuthenticationScheme?
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + CookieAuthenticationDefaults.AuthenticationScheme)]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext db;
        private readonly Dictionary<string, ApplicationUser> connectedUsers;

        public ChatHub(ApplicationDbContext db)
        {
            this.db = db;

            connectedUsers = new Dictionary<string, ApplicationUser>();
        }

        public override Task OnConnectedAsync()
        {
            var user = db
                .Users
                .FirstOrDefault(u => u.UserName == Context.User.Identity.Name);

            connectedUsers.Add(Context.ConnectionId, user);

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            connectedUsers.Remove(Context.ConnectionId);

            return base.OnDisconnectedAsync(exception);
        }

        public async Task Test(object message)
        {
            await Clients.Caller.SendAsync("Test", message);
        }

        public async Task Search(string q, int Page = 1, int Count = 50)
        {
            var users = db
                .Users
                .Where(u => u.Name.Contains(q) || u.Email.Contains(q) || u.UserName.Contains(q))
                .OrderByDescending(u => u.LastLoginDateTime)
                .Skip((Page - 1) * Count)
                .Take(Count);

            await Clients.Caller.SendAsync("SearchResult", Map.MapUsers(users));
        }

        public async Task GetFriendsList(int Page = 1, int Count = 50)
        {
            var user = db
                .Users
                .Include(u => u.Friends)
                .SingleOrDefault(u => u.UserName == Context.User.Identity.Name);

            var users = user
                .Friends
                .OrderByDescending(u => u.LastLoginDateTime)
                .Skip((Page - 1) * Count)
                .Take(Count);

            await Clients.Caller.SendAsync("FriendsList", Map.MapUsers(users));
        }

        public async Task GetConversations(int Page = 1, int Count = 50)
        {
            var user = db.Users.SingleOrDefault(u => u.UserName == Context.User.Identity.Name);

            var conversations = db
                .Conversations
                .Where(c => !c.Deleted && c.ConversationUsers.Select(cu => cu.User).Contains(user))
                .OrderByDescending(c => c.Messages.OrderByDescending(m => m.DateTime))
                .Skip((Page - 1) * Count)
                .Take(Count);

            await Clients.Caller.SendAsync("Conversations", Map.MapConversations(conversations));
        }

        public async Task GetConversationMessages(Guid ID, int Page = 1, int Count = 50)
        {
            var user = db.Users.SingleOrDefault(u => u.UserName == Context.User.Identity.Name);

            var messages = db
                .Conversations
                .Where(c => c.ConversationUsers.Select(cu => cu.User).Contains(user))
                .FirstOrDefault(c => c.ID.Equals(ID))
                .Messages
                .Where(m => !m.Deleted)
                .OrderByDescending(m => m.DateTime)
                .Skip((Page - 1) * Count)
                .Take(Count);

            var r = new
            {
                ConversationID = ID,
                Messages = Map.MapMessages(messages)
            };

            await Clients.Caller.SendAsync("ConversationMessages", r);
        }

        public async Task CreateConversation(string Name, string[] Users)
        {
            var user = db.Users.SingleOrDefault(u => u.UserName == Context.User.Identity.Name);

            var conversation = new Conversation
            {
                ID = Guid.NewGuid().ToString(),
                Name = Name,
                ConversationUsers = db.Users.Where(u => Users.Contains(u.Id)).Select(u => new ConversationUser { User = u }).ToList(),
                ConversationAdmins = new List<ConversationAdmin> { new ConversationAdmin { Admin = user } }
            };

            db.Conversations.Add(conversation);

            await db.SaveChangesAsync();

            var receivers = conversation.ConversationUsers.Select(cu => cu.User).Where(connectedUsers.Values.Contains);

            foreach (var r in receivers)
            {
                await Clients.Client(connectedUsers.FirstOrDefault(ku => ku.Value == r).Key).SendAsync("ConversationCreated", Map.MapConversation(conversation));
            }
        }

        public async Task AddUseraToConversation(Guid ID, string[] Users)
        {
            var user = db
                .Users
                //.Include(u => u.Friends)
                .SingleOrDefault(u => u.UserName == Context.User.Identity.Name);

            var conversation = db
                .Conversations
                .Include(c => c.ConversationUsers)
                .Where(c => c.ConversationAdmins.Select(ca => ca.Admin).Contains(user))
                .FirstOrDefault(c => c.ID.Equals(ID));

            if (conversation != null)
            {
                conversation
                    .ConversationUsers
                    .AddRange(db
                              .Users
                              .Where(u => Users.Contains(u.Id) && !conversation.ConversationUsers.Select(cu => cu.UserID).Contains(u.Id))
                              .Select(u => new ConversationUser { User = u }).ToList()
                             );

                await db.SaveChangesAsync();

                var receivers = conversation.ConversationUsers.Select(cu => cu.User).Where(connectedUsers.Values.Contains);

                foreach (var r in receivers)
                {
                    await Clients.Client(connectedUsers.FirstOrDefault(ku => ku.Value == r).Key).SendAsync("ConversationUpdated", Map.MapConversation(conversation));
                }
            }
        }

        public async Task AddAdminsToConversation(Guid ID, string[] Users)
        {
            var user = db
               .Users
               //.Include(u => u.Friends)
               .SingleOrDefault(u => u.UserName == Context.User.Identity.Name);

            var conversation = db
                .Conversations
                .Include(c => c.ConversationAdmins)
                .Where(c => c.ConversationAdmins.Select(ca => ca.Admin).Contains(user))
                .FirstOrDefault(c => c.ID.Equals(ID));

            if (conversation != null)
            {
                conversation
                    .ConversationAdmins
                    .AddRange(db
                              .Users
                              .Where(u => Users.Contains(u.Id) && !conversation.ConversationAdmins.Select(ca => ca.AdminID).Contains(u.Id))
                              .Select(u => new ConversationAdmin { Admin = u }).ToList());

                await db.SaveChangesAsync();

                var receivers = conversation.ConversationUsers.Select(cu => cu.User).Where(connectedUsers.Values.Contains);

                foreach (var r in receivers)
                {
                    await Clients.Client(connectedUsers.FirstOrDefault(ku => ku.Value == r).Key).SendAsync("ConversationUpdated", Map.MapConversation(conversation));
                }
            }
        }

        public async Task RemoveUsersFromConversation(Guid ID, string[] Users)
        {
            var user = db
               .Users
               //.Include(u => u.Friends)
               .SingleOrDefault(u => u.UserName == Context.User.Identity.Name);

            var conversation = db
                .Conversations
                .Include(c => c.ConversationUsers)
                .Where(c => c.ConversationAdmins.Select(ca => ca.Admin).Contains(user))
                .FirstOrDefault(c => c.ID.Equals(ID));

            if (conversation != null)
            {
                foreach (var item in db.ConversationUsers.Where(cu => Users.Contains(cu.UserID)))
                {
                    conversation.ConversationUsers.Remove(item);
                }

                await db.SaveChangesAsync();

                var receivers = conversation.ConversationUsers.Select(cu => cu.User).Where(connectedUsers.Values.Contains);

                foreach (var r in receivers)
                {
                    await Clients.Client(connectedUsers.FirstOrDefault(ku => ku.Value == r).Key).SendAsync("ConversationUpdated", Map.MapConversation(conversation));
                }
            }
        }

        public async Task RemoveAdminsFromConversation(Guid ID, string[] Users)
        {
            var user = db
                .Users
                //.Include(u => u.Friends)
                .SingleOrDefault(u => u.UserName == Context.User.Identity.Name);

            var conversation = db
                .Conversations
                .Where(c => c.ConversationAdmins.Select(ca => ca.Admin).Contains(user))
                .FirstOrDefault(c => c.ID.Equals(ID));

            if (conversation != null)
            {
                foreach (var item in db.ConversationAdmins.Where(ca => Users.Contains(ca.AdminID)))
                {
                    conversation.ConversationAdmins.Remove(item);
                }

                await db.SaveChangesAsync();

                var receivers = conversation.ConversationUsers.Select(cu => cu.User).Where(connectedUsers.Values.Contains);

                foreach (var r in receivers)
                {
                    await Clients.Client(connectedUsers.FirstOrDefault(ku => ku.Value == r).Key).SendAsync("ConversationUpdated", Map.MapConversation(conversation));
                }
            }
        }

        public async Task UpdateConversationName(Guid ID, string Name)
        {
            var user = db
                .Users
                //.Include(u => u.Friends)
                .SingleOrDefault(u => u.UserName == Context.User.Identity.Name);

            var conversation = db
                .Conversations
                .Where(c => c.ConversationAdmins.Select(ca => ca.Admin).Contains(user))
                .FirstOrDefault(c => c.ID.Equals(ID));

            if (conversation != null)
            {
                conversation.Name = Name;

                await db.SaveChangesAsync();

                var receivers = conversation.ConversationUsers.Select(cu => cu.User).Where(connectedUsers.Values.Contains);

                foreach (var r in receivers)
                {
                    await Clients.Client(connectedUsers.FirstOrDefault(ku => ku.Value == r).Key).SendAsync("ConversationUpdated", Map.MapConversation(conversation));
                }
            }
        }

        public async Task DeleteConversation(Guid ID)
        {
            var user = db
                .Users
                //.Include(u => u.Friends)
                .SingleOrDefault(u => u.UserName == Context.User.Identity.Name);

            var conversation = db
                .Conversations
                .Where(c => c.ConversationAdmins.Select(ca => ca.Admin).Contains(user))
                .FirstOrDefault(c => c.ID.Equals(ID));

            if (conversation != null)
            {
                conversation.Deleted = true;

                await db.SaveChangesAsync();

                var receivers = conversation.ConversationUsers.Select(cu => cu.User).Where(connectedUsers.Values.Contains);

                foreach (var r in receivers)
                {
                    await Clients.Client(connectedUsers.FirstOrDefault(ku => ku.Value == r).Key).SendAsync("ConversationDeleted", ID);
                }
            }
        }

        public async Task SendMessage(Guid ConversationID, string Message, string Title = null)
        {
            var user = db
                .Users
                //.Include(u => u.Friends)
                .SingleOrDefault(u => u.UserName == Context.User.Identity.Name);

            var conversation = db
                .Conversations
                .Include(c => c.Messages)
                .Where(c => c.ConversationUsers.Select(cu => cu.User).Contains(user))
                .FirstOrDefault(c => c.ID.Equals(ConversationID));

            if (conversation != null)
            {
                var message = new Message
                {
                    ID = Guid.NewGuid(),
                    Body = Message,
                    Title = Title,
                    DateTime = DateTime.Now,
                    From = user,
                    Conversation = conversation
                };

                db.Messages.Add(message);

                await db.SaveChangesAsync();

                var receivers = conversation.ConversationUsers.Select(cu => cu.User).Where(connectedUsers.Values.Contains);

                foreach (var r in receivers)
                {
                    await Clients.Client(connectedUsers.FirstOrDefault(ku => ku.Value == r).Key).SendAsync("Message", Map.MapMessage(message));
                }
            }
        }

        public async Task DeleteMessage(Guid ID)
        {
            var user = db
                .Users
                //.Include(u => u.Friends)
                .SingleOrDefault(u => u.UserName == Context.User.Identity.Name);

            var message = db
                .Messages
                .Include(m => m.Conversation)
                .Where(m => m.From == user || m.Conversation.ConversationAdmins.Select(ca => ca.Admin).Contains(user))
                .FirstOrDefault(m => m.ID == ID);

            if (message != null)
            {
                message.Deleted = true;

                await db.SaveChangesAsync();

                var receivers = message.Conversation.ConversationUsers.Select(cu => cu.User).Where(connectedUsers.Values.Contains);

                foreach (var r in receivers)
                {
                    await Clients.Client(connectedUsers.FirstOrDefault(ku => ku.Value == r).Key).SendAsync("MessageDeleted", ID);
                }
            }
        }

        public async Task SetMessageRead(Guid ID, bool IsReceived, bool IsRead)
        {
            var user = db
                .Users
                //.Include(u => u.Friends)
                .SingleOrDefault(u => u.UserName == Context.User.Identity.Name);

            var message = db
                .Messages
                .Include(m => m.Conversation)
                .Include(m => m.MessageUsers)
                .Where(m => m.From == user || m.Conversation.ConversationUsers.Select(cu => cu.User).Contains(user))
                .FirstOrDefault(m => m.ID == ID);

            if (message != null)
            {
                UserMessage userMessage = db
                    .UserMessages
                    .FirstOrDefault(m => m.UserID == user.Id && m.MessageID == ID);

                if (userMessage == null)
                {
                    userMessage = new UserMessage
                    {
                        User = user,
                        Message = message
                    };

                    db.UserMessages.Add(userMessage);
                }

                if (IsReceived && userMessage.ReceiveDateTime == null)
                {
                    userMessage.ReceiveDateTime = DateTime.Now;
                }

                if (IsRead && userMessage.ReadDateTime == null)
                {
                    userMessage.ReadDateTime = DateTime.Now;
                }

                await db.SaveChangesAsync();

                var receivers = message.Conversation.ConversationUsers.Select(cu => cu.User).Where(connectedUsers.Values.Contains);

                foreach (var r in receivers)
                {
                    await Clients.Client(connectedUsers.FirstOrDefault(ku => ku.Value == r).Key).SendAsync("Message", Map.MapMessage(message));
                }
            }
        }
    }
}