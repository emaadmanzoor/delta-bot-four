﻿using Core.Foundation.IoC;
using DeltaBotFour.Infrastructure;
using DeltaBotFour.Infrastructure.Implementation;
using DeltaBotFour.Infrastructure.Interface;
using DeltaBotFour.Reddit.Implementation;
using DeltaBotFour.Reddit.Interface;
using DeltaBotFour.Shared.Implementation;
using DeltaBotFour.Shared.Interface;
using RedditSharp;

namespace DeltaBotFour.DependencyResolver
{
    public class DeltaBotFourRegistrationCatalog : IRegistrationCatalog
    {
        public void Register(IModularContainer container)
        {
            var appConfiguration = new AppConfiguration();

            // Actual login is performed here.
            // TODO: This should be abstracted away as well
            var botWebAgent = new BotWebAgent
            (
                appConfiguration.DB4Username,
                appConfiguration.DB4Password,
                appConfiguration.DB4ClientId,
                appConfiguration.DB4ClientSecret,
                 "http://localhost"
            );

            var reddit = new RedditSharp.Reddit(botWebAgent, false);
            var subreddit = reddit.GetSubredditAsync($"/r/{appConfiguration.SubredditName}").Result;

            // Register core / shared classes
            container.RegisterSingleton(appConfiguration);
            container.RegisterSingleton(botWebAgent);
            container.RegisterSingleton(reddit);
            container.RegisterSingleton(subreddit);

            // Register shared services
            container.Register<IDB4Queue, DB4MemoryQueue>();

            // Register Reddit Services
            container.Register<ICommentDispatcher, RedditSharpCommentDispatcher>();
            container.Register<ICommentMonitor, RedditSharpCommentMonitor>();
            container.Register<IFlairEditor, RedditSharpFlairEditor>();
            container.Register<IRedditThingService, RedditSharpThingService>();
            container.Register<IWikiEditor, RedditSharpWikiEditor>();

            // Register functionality implementations
            container.Register<IDB4QueueDispatcher, DB4QueueDispatcher>();
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
