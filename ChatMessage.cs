using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace VitaTrack
{
    public class ChatMessage
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("senderId")]
        public int SenderId { get; set; }

        [JsonPropertyName("receiverId")]
        public int ReceiverId { get; set; }

        [JsonPropertyName("message1")]
        public string Message { get; set; }

        [JsonPropertyName("sentAt")]
        public DateTime SentAt { get; set; }

        [JsonPropertyName("isRead")]
        public bool IsRead { get; set; }

        public bool IsOwnMessage { get; set; } // calculat client-side
    }

    public class SendMessageDto
    {
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string Message { get; set; }
    }
}
