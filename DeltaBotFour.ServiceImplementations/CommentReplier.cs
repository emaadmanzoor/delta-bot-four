﻿using DeltaBotFour.ServiceInterfaces;
using DeltaBotFour.Models;
using RedditSharp.Things;
using System.IO;
using Core.Foundation.Helpers;

namespace DeltaBotFour.ServiceImplementations
{
    public class CommentReplier : ICommentReplier
    {
        private AppConfiguration _appConfiguration;
        private string _replyTemplate;

        public CommentReplier(AppConfiguration appConfiguration)
        {
            _appConfiguration = appConfiguration;
        }

        public async void Reply(Comment comment, DeltaCommentValidationResult deltaCommentValidationResult)
        {
            if (_appConfiguration.ReadonlyMode) { return; }

            string replyMessage = getReplyMessage(deltaCommentValidationResult);

            await comment.ReplyAsync(replyMessage);

            ConsoleHelper.WriteLine($"DeltaBot replied -> result: {deltaCommentValidationResult.ResultType.ToString()} link: {comment.Shortlink}");
        }

        public async void EditReply(Comment commentToEdit, DeltaCommentValidationResult deltaCommentValidationResult)
        {
            if (_appConfiguration.ReadonlyMode) { return; }

            string replyMessage = getReplyMessage(deltaCommentValidationResult);

            await commentToEdit.EditTextAsync(replyMessage);

            ConsoleHelper.WriteLine($"DeltaBot edited a reply -> result: {deltaCommentValidationResult.ResultType.ToString()} link: {commentToEdit.Shortlink}");
        }

        public async void DeleteReply(Comment commentToDelete)
        {
            if (_appConfiguration.ReadonlyMode) { return; }

            await commentToDelete.DelAsync();

            ConsoleHelper.WriteLine($"DeltaBot deleted a reply -> link: {commentToDelete.Shortlink}");
        }

        private string getReplyMessage(DeltaCommentValidationResult deltaCommentValidationResult)
        {
            if(string.IsNullOrEmpty(_replyTemplate))
            {
                // Load reply template
                _replyTemplate = File.ReadAllText(_appConfiguration.TemplateFiles.DB4ReplyTemplateFile);
            }

            // TODO: Fix footer
            return _replyTemplate.Replace(_appConfiguration.ReplaceTokens.DB4ReplyToken, deltaCommentValidationResult.ReplyCommentBody);
        }
    }
}
