using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ChatMeService.Models.Entities
{
    public class UserMessage
    {
        [Key]
        public Guid ID { get; set; }
        public ApplicationUser User { get; set; }
        public string UserID { get; set; }
        public Message Message { get; set; }
        public Guid MessageID { get; set; }
        public DateTime? ReceiveDateTime { get; set; }
        public DateTime? ReadDateTime { get; set; }
    }
}