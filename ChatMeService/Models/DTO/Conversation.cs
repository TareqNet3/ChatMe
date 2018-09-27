using System;
using System.Collections.Generic;

namespace ChatMeService.Models.DTO
{
    public class Conversation
    {
        public Guid ID { get; set; }
        public string Name { get; set; }

        public List<User> Users { get; set; }
        public List<User> Admins { get; set; }
        public List<Message> Messages { get; set; }
    }
}