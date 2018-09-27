using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ChatMeService.Models.Entities
{
    public class ConversationAdmin
    {
        public Conversation Conversation { get; set; }

        [Display(Name = "Conversation ID")]
        public string ConversationID { get; set; }// ID should be string for M2M relation ConversationUser & ConversationAdmin

        public ApplicationUser Admin { get; set; }

        [Display(Name = "Admin ID")]
        public string AdminID { get; set; }
    }
}