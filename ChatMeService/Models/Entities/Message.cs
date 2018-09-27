using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ChatMeService.Models.Entities
{
    public class Message
    {
        [Key]
        public Guid ID { get; set; }
        public ApplicationUser From { get; set; }
        public string FromID { get; set; }
        public Conversation Conversation { get; set; }
        public string ConversationID { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public DateTime DateTime { get; set; }
        public bool Deleted { get; set; }

        public virtual List<UserMessage> MessageUsers { get; set; }
    }
}