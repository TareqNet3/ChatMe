using System;
using System.Collections.Generic;

namespace ChatMeService.Models.DTO
{
    public class Message
    {
        public Guid ID { get; set; }
        public string FromUserID { get; set; }
        public Guid ConversationID { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public DateTime DateTime { get; set; }

        public virtual List<UserMessage> MessageUsers { get; set; }
    }
}