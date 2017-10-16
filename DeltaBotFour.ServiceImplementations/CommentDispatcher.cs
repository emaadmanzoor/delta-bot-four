﻿using DeltaBotFour.ServiceInterfaces;
using RedditSharp.Things;
using DeltaBotFour.Models;
using Newtonsoft.Json;

namespace DeltaBotFour.ServiceImplementations
{
    public class CommentDispatcher : ICommentDispatcher
    {
        private string SHORT_LINK_FROM = "www.reddit.com";
        private string SHORT_LINK_TO = "oauth.reddit.com";

        private IDB4Queue _queue;

        public CommentDispatcher(IDB4Queue queue)
        {
            _queue = queue;
        }

        public void SendToQueue(Comment comment)
        {
            var db4Comment = getDB4Comment(comment);
            pushCommentToQueue(db4Comment);
        }

        private DB4Comment getDB4Comment(Comment comment)
        {
            // Convert to a DB4Comment
            return new DB4Comment
            {
                ParentId = comment.ParentId,
                ShortLink = comment.Shortlink.Replace(SHORT_LINK_FROM, SHORT_LINK_TO),
                Body = comment.Body,
                IsEdited = comment.Edited
            };
        }

        private void pushCommentToQueue(DB4Comment db4Comment)
        {
            // Put on the queue for comment processing
            _queue.Push(new QueueMessage(QueueMessageType.Comment, JsonConvert.SerializeObject(db4Comment)));
        }
    }
}
