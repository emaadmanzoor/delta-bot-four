﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DeltaBotFour.Models;
using Microsoft.Extensions.Configuration;

namespace DeltaBotFour.Infrastructure
{
    public class AppConfiguration
    {
        private readonly IConfigurationRoot _configuration;

        public List<DB4Mode> DB4Modes => _configuration["db4_modes"].Split(',', StringSplitOptions.RemoveEmptyEntries).Select(Enum.Parse<DB4Mode>).ToList();
        public string DB4Username => _configuration["db4_username"];
        public string DB4Password => _configuration["db4_password"];
        public string DB4ClientId => _configuration["db4_client_id"];
        public string DB4ClientSecret => _configuration["db4_client_secret"];
        public string RedditBaseUrl => _configuration["reddit_base_url"];
        public string SubredditName => _configuration["subreddit_name"];
        public List<string> ValidDeltaIndicators => _configuration["valid_delta_indicators"].Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
        public int HoursToUnawardDelta => int.Parse(_configuration["hours_to_unaward_delta"]);
        public string WikiUrlDeltaboards => _configuration["wiki_url_deltaboards"];
        public string WikiUrlUser => _configuration["wiki_url_user"];
        public Regex HiddenParamsRegex => new Regex(_configuration["hidden_params_regex"], RegexOptions.Singleline);
        public string DefaultHiddenParamsComment => _configuration["default_hidden_params_comment"];
        public Regex GetWikiLinkRegex(string subredditName, string contextNumber)
        { 
            return new Regex(_configuration["wiki_link_regex"]
                .Replace(ReplaceTokens.SubredditToken, subredditName)
                .Replace(ReplaceTokens.ContextNumberToken, contextNumber));
        }
        public DeltaBotTemplateFiles TemplateFiles { get; }
        public DeltaBotReplaceTokens ReplaceTokens { get; }
        public DeltaBotReplies Replies { get; }
        public DeltaBotValidationValues ValidationValues { get; }

        public AppConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            _configuration = builder.Build();

            TemplateFiles = new DeltaBotTemplateFiles();
            TemplateFiles.Initialize(_configuration);

            ReplaceTokens = new DeltaBotReplaceTokens();
            ReplaceTokens.Initialize(_configuration);

            Replies = new DeltaBotReplies();
            Replies.Initialize(_configuration);

            ValidationValues = new DeltaBotValidationValues();
            ValidationValues.Initialize(_configuration);
        }

        public class DeltaBotTemplateFiles
        {
            private IConfigurationRoot _configuration;

            public string DB4ReplyTemplateFile => _configuration["template_files:db4_reply_template_file"];
            public string DeltaboardsTemplateFile => _configuration["template_files:deltaboards_template_file"];
            public string DeltaboardTemplateFile => _configuration["template_files:deltaboard_template_file"];
            public string DeltaboardRowTemplateFile => _configuration["template_files:deltaboard_row_template_file"];
            public string UserWikiTemplateFile => _configuration["template_files:user_wiki_template_file"];
            public string UserWikiRowTemplateFile => _configuration["template_files:user_wiki_row_template_file"];

            public void Initialize(IConfigurationRoot configuration)
            {
                _configuration = configuration;
            }
        }

        public class DeltaBotReplaceTokens
        {
            private IConfigurationRoot _configuration;

            public string DateToken => _configuration["replace_tokens:date_token"];
            public string UsernameToken => _configuration["replace_tokens:username_token"];
            public string UserWikiLinkToken => _configuration["replace_tokens:user_wiki_link_token"];
            public string HiddenParamsToken => _configuration["replace_tokens:hidden_params_token"];
            public string DailyDeltaboardToken => _configuration["replace_tokens:daily_deltaboard_token"];
            public string WeeklyDeltaboardToken => _configuration["replace_tokens:weekly_deltaboard_token"];
            public string MonthlyDeltaboardToken => _configuration["replace_tokens:monthly_deltaboard_token"];
            public string YearlyDeltaboardToken => _configuration["replace_tokens:yearly_deltaboard_token"];
            public string AllTimeDeltaboardToken => _configuration["replace_tokens:all_time_deltaboard_token"];
            public string DeltaboardTypeToken => _configuration["replace_tokens:deltaboard_type_token"];
            public string DeltaboardRowsToken => _configuration["replace_tokens:deltaboard_rows_token"];
            public string RankToken => _configuration["replace_tokens:rank_token"];
            public string CountToken => _configuration["replace_tokens:count_token"];
            public string DeltasGivenCountToken => _configuration["replace_tokens:deltas_given_count_token"];
            public string DeltasReceivedCountToken => _configuration["replace_tokens:deltas_received_count_token"];
            public string WikiRowsGivenToken => _configuration["replace_tokens:wiki_rows_given_token"];
            public string WikiRowsReceivedToken => _configuration["replace_tokens:wiki_rows_received_token"];
            public string DateMMDYYYY => _configuration["replace_tokens:date_mmdyyyy"];
            public string PostTitle => _configuration["replace_tokens:post_title"];
            public string PostLink => _configuration["replace_tokens:post_link"];
            public string CommentLink => _configuration["replace_tokens:comment_link"];
            public string ParentAuthorNameToken => _configuration["replace_tokens:parent_author_name_token"];
            public string DeltasToken => _configuration["replace_tokens:deltas_token"];
            public string SubredditToken => _configuration["replace_tokens:subreddit_token"];
            public string IssueCountToken => _configuration["replace_tokens:issue_count_token"];
            public string DB4ReplyToken => _configuration["replace_tokens:db4_reply_token"];
            public string ContextNumberToken => _configuration["replace_tokens:context_number_token"];

            public void Initialize(IConfigurationRoot configuration)
            {
                _configuration = configuration;
            }
        }

        public class DeltaBotReplies
        {
            private IConfigurationRoot _configuration;

            public string DeltaAwarded => _configuration["validation_replies:delta_awarded"];
            public string CommentTooShort => _configuration["validation_replies:comment_too_short"];
            public string CannotAwardOP => _configuration["validation_replies:cannot_award_op"];
            public string CannotAwardDeltaBot => _configuration["validation_replies:cannot_award_deltabot"];
            public string CannotAwardSelf => _configuration["validation_replies:cannot_award_self"];
            public string WithIssues => _configuration["validation_replies:fail_with_issues"];
            public List<string> SuccessReplies { get; private set; }
            public List<string> FailReplies { get; private set; }

            public void Initialize(IConfigurationRoot configuration)
            {
                _configuration = configuration;

                SuccessReplies = new List<string> { DeltaAwarded };

                FailReplies = new List<string>
                {
                    CommentTooShort,
                    CannotAwardOP,
                    CannotAwardDeltaBot,
                    CannotAwardSelf,
                    WithIssues
                };
            }
        }

        public class DeltaBotValidationValues
        {
            private IConfigurationRoot _configuration;

            public int CommentTooShortLength => int.Parse(_configuration["validation_values:comment_too_short_length"]);

            public void Initialize(IConfigurationRoot configuration)
            {
                _configuration = configuration;
            }
        }
    }
}