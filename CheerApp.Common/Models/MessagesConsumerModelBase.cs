using System.Collections.Generic;
using Google.Cloud.Firestore;

namespace CheerApp.Common.Models
{
    public abstract class MessagesConsumerModelBase : ModelBase
    {
        public MessagesConsumerModelBase() : base()
        { }

        [FirestoreProperty]
        public List<string> NewMessageIds { get; set; } = new List<string>();
        [FirestoreProperty]
        public List<string> ConsumedMessageIds { get; set; } = new List<string>();
    }
}