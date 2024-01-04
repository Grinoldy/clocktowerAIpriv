﻿using Clocktower.Game;
using OpenAi;
using System.IO;

namespace Clocktower.Agent.RobotAgent
{
    /// <summary>
    /// Holds all messages the player has received and sent this game, can send and receive additional messages, 
    /// and will request the player summarize messages at the end of each day.
    /// </summary>
    internal class GameChat
    {
        /// <summary>
        /// Event is triggered whenever a new message is added to the chat. Note that this will include all
        /// messages (though not summaries) even if the actual prompts to the AI don't include all the messages.
        /// </summary>
        public event ChatMessageHandler? OnChatMessage;

        /// <summary>
        /// Event is triggered whenever the AI summarizes the previous day.
        /// </summary>
        public event DaySummaryHandler? OnDaySummary;

        /// <summary>
        /// Event is triggered whenever tokens are used, i.e. whenever a request is made to the AI.
        /// </summary>
        public event TokenCountHandler? OnTokenCount;

        public GameChat(string playerName, IReadOnlyCollection<string> playerNames, IReadOnlyCollection<Character> script)
        {
            logStream = new StreamWriter($"{playerName}-{DateTime.UtcNow:yyyyMMddTHHmmss}.log");

            openAiChat.OnChatMessageAdded += OnChatMessageAdded;
            openAiChat.OnSubChatSummarized += OnSubChatSummarized;
            openAiChat.OnAssistantRequest += OnAssistantRequest;

            openAiChat.SystemMessage = SystemMessage.GetSystemMessage(playerName, playerNames, script);
        }

        public async Task NewPhase(Phase phase, int dayNumber)
        {
            var phaseName = phase == Phase.Setup ? "Set-up" : $"{phase} {dayNumber}";
            var summarizePrompt = phase == Phase.Day ? $"Please provide a detailed summary, in bullet-point form, of what happened and what you learned in {phaseName.ToLowerInvariant()}. " +
                                                        "There should be a point for each private chat that you had; a point for the discussion around each nomination; " +
                                                        "as well as points for any general public discussion or abilities publicly used. There's no need to provide any concluding remarks - just the detailed points are enough."
                                                     : null;
            await openAiChat.StartNewSubChat(phaseName, summarizePrompt);
        }

        public void AddMessage(string message)
        {
            openAiChat.AddUserMessage(message);
        }

        public async Task<string> Request(string? prompt)
        {
            if (!string.IsNullOrEmpty(prompt))
            {
                AddMessage(prompt);
            }
            return await openAiChat.GetAssistantResponse();
        }

        /// <summary>
        /// Removes the last few messages from the list of messages. Useful when you don't want unneeded messages cluttering up the chat log.
        /// Only applies to the current phase.
        /// </summary>
        /// <param name="messageCount">The number of messages to remove.</param>
        public void Trim(int messageCount)
        {
            openAiChat.TrimMessages(messageCount);
        }

        private void OnChatMessageAdded(string subChatName, Role role, string message)
        {
            logStream.WriteLine($"[{role}] {message}");
            logStream.Flush();

            OnChatMessage?.Invoke(role, message);
        }

        private void OnSubChatSummarized(string subChatName, string summary)
        {
            logStream.WriteLine($"[Summary of {subChatName}] {summary}");
            logStream.Flush();

            // The sub-chat name should by "Day NN".
            if (int.TryParse(subChatName[4..], out int dayNumber))
            {
                OnDaySummary?.Invoke(dayNumber, summary);
            }
        }

        private void OnAssistantRequest(string subChatName, bool isSummaryRequest, IReadOnlyCollection<(Role role, string message)> messages,
                                        string response, int promptTokens, int completionTokens, int totalTokens)
        {
            OnTokenCount?.Invoke(promptTokens, completionTokens, totalTokens);
        }

        private readonly OpenAiChat openAiChat = new OpenAiChat("gpt-3.5-turbo-1106");
        private readonly TextWriter logStream;
    }
}
