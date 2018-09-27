using System;
using System.Linq;
using System.Collections.Generic;
using AutoMapper;

namespace ChatMeService.Utils
{
    public static class Map
    {
        static Map()
        {
            try
            {
                Mapper.Initialize(
                    cfg =>
                    {
                        cfg.CreateMap<Models.Entities.Conversation, Models.DTO.Conversation>();
                        cfg.CreateMap<Models.Entities.Message, Models.DTO.Message>();
                        cfg.CreateMap<Models.Entities.UserMessage, Models.DTO.UserMessage>();
                        cfg.CreateMap<Models.ApplicationUser, Models.DTO.User>();
                    });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static TDTO MapObject<T, TDTO>(T Data)
        {
            return Mapper.Map<T, TDTO>(Data);
        }

        public static Models.DTO.User MapUser(Models.ApplicationUser user)
        {
            return MapObject<Models.ApplicationUser, Models.DTO.User>(user);
        }

        public static List<Models.DTO.User> MapUsers(IEnumerable<Models.ApplicationUser> users)
        {
            return MapObject<IEnumerable<Models.ApplicationUser>, List<Models.DTO.User>>(users);
        }

        public static List<Models.DTO.User> MapUsers(IQueryable<Models.ApplicationUser> users)
        {
            return MapObject<IQueryable<Models.ApplicationUser>, List<Models.DTO.User>>(users);
        }

        public static Models.DTO.Conversation MapConversation(Models.Entities.Conversation conversation)
        {
            return MapObject<Models.Entities.Conversation, Models.DTO.Conversation>(conversation);
        }

        public static List<Models.DTO.Conversation> MapConversations(IEnumerable<Models.Entities.Conversation> conversations)
        {
            return MapObject<IEnumerable<Models.Entities.Conversation>, List<Models.DTO.Conversation>>(conversations);
        }

        public static List<Models.DTO.Conversation> MapConversations(IQueryable<Models.Entities.Conversation> conversations)
        {
            return MapObject<IQueryable<Models.Entities.Conversation>, List<Models.DTO.Conversation>>(conversations);
        }

        public static Models.DTO.Message MapMessage(Models.Entities.Message message)
        {
            return MapObject<Models.Entities.Message, Models.DTO.Message>(message);
        }

        public static List<Models.DTO.Message> MapMessages(IEnumerable<Models.Entities.Message> messages)
        {
            return MapObject<IEnumerable<Models.Entities.Message>, List<Models.DTO.Message>>(messages);
        }

        public static List<Models.DTO.Message> MapMessages(IQueryable<Models.Entities.Message> messages)
        {
            return MapObject<IQueryable<Models.Entities.Message>, List<Models.DTO.Message>>(messages);
        }
    }
}