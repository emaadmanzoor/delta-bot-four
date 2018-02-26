﻿using Core.Foundation.IoC;
using DeltaBotFour.ServiceImplementations;
using DeltaBotFour.ServiceInterfaces;
using RedditSharp;
using RedditSharp.Things;
using System;

namespace DeltaBotFour.DependencyResolver
{
    public class DeltaBotFourRegistrationCatalog : IRegistrationCatalog
    {
        public void Register(IModularContainer container)
        {
            var appConfiguration = new AppConfiguration();

            var botWebAgent = new BotWebAgent
            (
                username: appConfiguration.DB4Username,
                password: appConfiguration.DB4Password,
                clientID: appConfiguration.DB4ClientId,
                clientSecret: appConfiguration.DB4ClientSecret,
                redirectURI: "http://localhost"
            );

            var reddit = new Reddit(botWebAgent, false);
            var subreddit = reddit.GetSubredditAsync($"/r/{appConfiguration.SubredditName}").Result;

            // Register core / shared classes
            container.RegisterSingleton(appConfiguration);
            container.RegisterSingleton(botWebAgent);
            container.RegisterSingleton(reddit);
            container.RegisterSingleton(subreddit);

            // Register functionality implementations
            container.Register<IDB4Queue, DB4MemoryQueue>();
            container.Register<IDB4QueueDispatcher, DB4QueueDispatcher>();
            container.Register<ICommentMonitor, CommentMonitor>();
            container.Register<ICommentDispatcher, CommentDispatcher>();
            container.Register<IObserver<VotableThing>, IncomingCommentObserver>();
            container.Register<ICommentProcessor, CommentProcessor>();
            container.Register<ICommentReplyDetector, CommentReplyDetector>();
            container.Register<ICommentValidator, CommentValidator>();
            container.Register<ICommentReplier, CommentReplier>();
            container.Register<IDeltaAwarder, DeltaAwarder>();
            container.Register<IUserWikiEditor, UserWikiEditor>();
            container.Register<IDeltaboardBuilder, DeltaboardBuilder>();
        }
    }
}
