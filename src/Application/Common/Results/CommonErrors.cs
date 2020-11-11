﻿using System.Collections.Generic;
using Crpg.Domain.Entities;

namespace Crpg.Application.Common.Results
{
    public static class CommonErrors
    {
        public static Error CharacterLevelRequirementNotMet(int requiredLevel, int actualLevel) => new Error(ErrorType.Validation, ErrorCode.CharacterLevelRequirementNotMet)
        {
            Title = "Unmet level requirement",
            Detail = $"Level {requiredLevel} is required but the character is {actualLevel}",
        };

        public static Error CharacterNotFound(int characterId, int userId) => new Error(ErrorType.NotFound, ErrorCode.CharacterNotFound)
        {
            Title = "Character was not found",
            Detail = $"Character with id '{characterId}' for user with id '{userId}' was not found",
        };

        public static Error ItemAlreadyOwned(int itemId) => new Error(ErrorType.Validation, ErrorCode.ItemAlreadyOwned)
        {
            Title = "Item is already owned",
            Detail = $"Item with id '{itemId}' is already owned by the user",
        };

        public static Error ItemBadType(int itemId, IEnumerable<ItemType> expectedTypes, ItemType actualType) =>
            new Error(ErrorType.Validation, ErrorCode.ItemBadType)
            {
                Title = "Item has a bad type",
                Detail = $"Item with id '{itemId}' was expected to be of type '{string.Join(',', expectedTypes)} but was '{actualType}'",
            };

        public static Error ItemMaxRankReached(int itemId, int userId, int maxRank) =>
            new Error(ErrorType.Validation, ErrorCode.ItemMaxRankReached)
        {
            Title = "User item has reached its max rank",
            Detail = $"Item with id '{itemId}' owned by user with id '{userId}' has reached its max rank ({maxRank})",
        };

        public static Error ItemNotFound(int itemId) => new Error(ErrorType.NotFound, ErrorCode.ItemNotFound)
        {
            Title = "Item was not found",
            Detail = $"Item with id '{itemId}' was not found",
        };

        public static Error ItemNotOwned(int itemId) => new Error(ErrorType.NotFound, ErrorCode.ItemNotOwned)
        {
            Title = "Item is not owned",
            Detail = $"Item with id '{itemId}' is not owned by the user",
        };

        public static Error NotEnoughAttributePoints(int requiredPoints, int actualPoints) => new Error(ErrorType.Validation, ErrorCode.NotEnoughAttributePoints)
        {
            Title = "Not enough attribute points",
            Detail = $"{requiredPoints} attribute points are required but only {actualPoints} are available",
        };

        public static Error NotEnoughGold(int requiredGold, int actualGold) => new Error(ErrorType.Validation, ErrorCode.NotEnoughGold)
        {
            Title = "Not enough gold",
            Detail = $"{requiredGold} gold is required but only {actualGold} is available",
        };

        public static Error NotEnoughHeirloomPoints(int requiredPoints, int actualPoints) => new Error(ErrorType.Validation, ErrorCode.NotEnoughHeirloomPoints)
        {
            Title = "Not enough heirloom points",
            Detail = $"{requiredPoints} points are required but only {actualPoints} are available",
        };

        public static Error NotEnoughSkillPoints(int requiredPoints, int actualPoints) => new Error(ErrorType.Validation, ErrorCode.NotEnoughSkillPoints)
        {
            Title = "Not enough skill points",
            Detail = $"{requiredPoints} skill points are required but only {actualPoints} are available",
        };

        public static Error NotEnoughWeaponProficiencyPoints(int requiredPoints, int actualPoints) => new Error(ErrorType.Validation, ErrorCode.NotWeaponProficiencyPoints)
        {
            Title = "Not enough weapon proficiency points",
            Detail = $"{requiredPoints} weapon proficiency points are required but only {actualPoints} are available",
        };

        public static Error SkillRequirementNotMet() => new Error(ErrorType.Validation, ErrorCode.SkillRequirementNotMet)
        {
            Title = "Unmet skill requirement",
        };

        public static Error StatisticDecreased() => new Error(ErrorType.Validation, ErrorCode.StatisticDecreased)
        {
            Title = "A statistic was decreased when it is not allowed",
        };

        public static Error UserNotFound(int userId) => new Error(ErrorType.NotFound, ErrorCode.UserNotFound)
        {
            Title = "User was not found",
            Detail = $"User with id '{userId}' was not found",
        };
    }
}