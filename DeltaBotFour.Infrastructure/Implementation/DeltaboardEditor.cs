﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.Foundation.Extensions;
using DeltaBotFour.Infrastructure.Interface;
using DeltaBotFour.Models;
using DeltaBotFour.Persistence.Interface;
using DeltaBotFour.Reddit.Interface;

namespace DeltaBotFour.Infrastructure.Implementation
{
    public class DeltaboardEditor : IDeltaboardEditor
    {
        private const int RanksToShow = 10;

        private readonly AppConfiguration _appConfiguration;
        private readonly IDB4Repository _deltaBotRepository;
        private readonly ISubredditService _subredditService;
        private readonly string _deltaboardsTemplate;
        private readonly string _deltaboardTemplate;
        private readonly string _deltaboardRowTemplate;
        private readonly string _deltaboardSidebarTemplate;

        public DeltaboardEditor(AppConfiguration appConfiguration, 
            IDB4Repository deltaBotRepository,
            ISubredditService subredditService)
        {
            _appConfiguration = appConfiguration;
            _deltaBotRepository = deltaBotRepository;
            _subredditService = subredditService;

            _deltaboardsTemplate = File.ReadAllText(appConfiguration.TemplateFiles.DeltaboardsTemplateFile);
            _deltaboardTemplate = File.ReadAllText(appConfiguration.TemplateFiles.DeltaboardTemplateFile);
            _deltaboardRowTemplate = File.ReadAllText(appConfiguration.TemplateFiles.DeltaboardRowTemplateFile);
            _deltaboardSidebarTemplate = File.ReadAllText(appConfiguration.TemplateFiles.DeltaboardSidebarTemplateFile);
        }

        public void AddDelta(string username)
        {
            // Add an entry for this user to the local db
            _deltaBotRepository.AddDeltaboardEntry(username);

            // Build and update wiki
            buildAndUpdateDeltaboards();
        }

        public void RemoveDelta(string username)
        {
            // Remove an entry for this user from the local db
            _deltaBotRepository.RemoveDeltaboardEntry(username);

            // Build and update wiki
            buildAndUpdateDeltaboards();
        }

        private List<Deltaboard> getDeltaboards()
        {
            return _deltaBotRepository.GetCurrentDeltaboards();
        }

        private void buildAndUpdateDeltaboards()
        {
            // Get the updated deltaboards
            var deltaboards = getDeltaboards();

            // Build the actual string content
            string updatedDeltaboards = buildDeltaboardsContent(deltaboards);

            // Update the wiki page
            _subredditService.EditPage(_appConfiguration.WikiUrlDeltaboards, updatedDeltaboards);

            // Get sidebar
            string sidebar = _subredditService.GetSidebar();

            var sidebarDeltaboardMatch = _appConfiguration.DeltaboardSidebarRegex.Match(sidebar);

            // Note: the second group is the content we're interested in
            if (sidebarDeltaboardMatch.Groups.Count == 2)
            {
                string currentSidebarDeltaboard = sidebarDeltaboardMatch.Groups[1].Value;
                string updatedSidebarDeltaboard =
                    $"\r\n{buildSidebarDeltaboard(deltaboards.First(db => db.DeltaboardType == DeltaboardType.Monthly))}";

                // Update sidebar with new content
                string updatedSidebar = sidebar.Replace(currentSidebarDeltaboard, updatedSidebarDeltaboard);

                _subredditService.UpdateSidebar(updatedSidebar);
            }
        }

        private string buildDeltaboardsContent(List<Deltaboard> deltaboards)
        {
            string deltaboardsContent = _deltaboardsTemplate;

            // Build daily deltaboard
            deltaboardsContent = deltaboardsContent.Replace(_appConfiguration.ReplaceTokens.DailyDeltaboardToken,
                buildDeltaboard(deltaboards.First(db => db.DeltaboardType == DeltaboardType.Daily)));

            // Build weekly deltaboard
            deltaboardsContent = deltaboardsContent.Replace(_appConfiguration.ReplaceTokens.WeeklyDeltaboardToken,
                buildDeltaboard(deltaboards.First(db => db.DeltaboardType == DeltaboardType.Weekly)));

            // Build montly deltaboard
            deltaboardsContent = deltaboardsContent.Replace(_appConfiguration.ReplaceTokens.MonthlyDeltaboardToken,
                buildDeltaboard(deltaboards.First(db => db.DeltaboardType == DeltaboardType.Monthly)));

            // Build yearly deltaboard
            deltaboardsContent = deltaboardsContent.Replace(_appConfiguration.ReplaceTokens.YearlyDeltaboardToken,
                buildDeltaboard(deltaboards.First(db => db.DeltaboardType == DeltaboardType.Yearly)));

            // Build all time deltaboard
            deltaboardsContent = deltaboardsContent.Replace(_appConfiguration.ReplaceTokens.AllTimeDeltaboardToken,
                buildDeltaboard(deltaboards.First(db => db.DeltaboardType == DeltaboardType.AllTime)));

            return deltaboardsContent;
        }

        private string buildDeltaboard(Deltaboard deltaboard)
        {
            string deltaboardContent = _deltaboardTemplate;

            deltaboardContent = deltaboardContent
                .Replace(_appConfiguration.ReplaceTokens.DeltaboardTypeToken, deltaboard.DeltaboardType.GetDescription())
                .Replace(_appConfiguration.ReplaceTokens.DeltaboardRowsToken,
                    buildDeltaboardRows(deltaboard.Entries.OrderBy(e => e.Rank).Take(RanksToShow).ToList()))
                .Replace(_appConfiguration.ReplaceTokens.DateToken, DateTime.UtcNow.ToString("M/d/yyyy HH:mm:ss UTC"));

            return deltaboardContent;
        }

        private string buildDeltaboardRows(List<DeltaboardEntry> entries)
        {
            string rowsContent = string.Empty;

            foreach (var entry in entries)
            {
                string rowContent = _deltaboardRowTemplate
                    .Replace(_appConfiguration.ReplaceTokens.RankToken, entry.Rank.ToString())
                    .Replace(_appConfiguration.ReplaceTokens.UsernameToken, entry.Username)
                    .Replace(_appConfiguration.ReplaceTokens.UserWikiLinkToken, getUserWikiUrl(entry.Username))
                    .Replace(_appConfiguration.ReplaceTokens.CountToken, entry.Count.ToString());

                rowsContent = $"{rowsContent}{rowContent}\r\n";
            }

            return rowsContent.TrimEnd("\r\n".ToCharArray());
        }

        private string buildSidebarDeltaboard(Deltaboard deltaboard)
        {
            string deltaboardContent = _deltaboardSidebarTemplate;

            deltaboardContent = deltaboardContent
                .Replace(_appConfiguration.ReplaceTokens.DeltaboardRowsToken,
                    buildDeltaboardRows(deltaboard.Entries.OrderBy(e => e.Rank).Take(RanksToShow).ToList()))
                .Replace(_appConfiguration.ReplaceTokens.DateToken, DateTime.UtcNow.ToString("M/d/yyyy HH:mm:ss UTC"))
                .Replace(_appConfiguration.ReplaceTokens.SubredditToken, _appConfiguration.SubredditName);

            return deltaboardContent;
        }

        private string getUserWikiUrl(string username)
        {
            string userUrl = _appConfiguration.WikiUrlUser.Replace(_appConfiguration.ReplaceTokens.UsernameToken, username);
            return $"{_appConfiguration.RedditBaseUrl}{_subredditService.GetWikiUrl()}{userUrl}";
        }
    }
}
