using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ChatMeService.Models.Entities
{
    public class Conversation
    {
        [Key]
        public string ID { get; set; } // ID should be string for M2M relation ConversationUser & ConversationAdmin
        public string Name { get; set; }
        public bool Deleted { get; set; }

        public virtual List<ConversationUser> ConversationUsers { get; set; }
        public virtual List<ConversationAdmin> ConversationAdmins { get; set; }
        public virtual List<Message> Messages { get; set; }
    }
}