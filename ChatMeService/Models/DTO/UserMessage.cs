using System;
using System.Collections.Generic;

namespace ChatMeService.Models.DTO
{
    public class UserMessage
    {
        public Guid ID { get; set; }
        public string UserID { get; set; }
        public Guid MessageID { get; set; }
        public DateTime? ReceiveDateTime { get; set; }
        public DateTime? ReadDateTime { get; set; }
    }
}