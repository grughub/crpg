using Crpg.Application.Characters.Commands;
using Crpg.Application.Common;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters;

public class RetireCharacterCommandTest : TestBase
{
    private static readonly Constants Constants = new()
    {
        MinimumLevel = 1,
        MinimumRetirementLevel = 31,
        ExperienceMultiplierByGeneration = 0.03f,
        MaxExperienceMultiplierForGeneration = 1.48f,
    };

    [TestCase(31, 1.00f, 1, 1.03f)]
    [TestCase(32, 1.00f, 1, 1.03f)]
    [TestCase(33, 1.00f, 2, 1.03f)]
    [TestCase(34, 1.00f, 2, 1.03f)]
    [TestCase(35, 1.00f, 3, 1.03f)]
    [TestCase(36, 1.00f, 3, 1.03f)]
    [TestCase(31, 1.06f, 1, 1.09f)]
    [TestCase(31, 1.06f, 1, 1.09f)]
    [TestCase(31, 1.45f, 1, 1.48f)]
    [TestCase(31, 1.48f, 1, 1.48f)]
    public async Task Basic(int level, float experienceMultiplier, int expectedPoints, float expectedExperienceMultiplier)
    {
        Character character = new()
        {
            Generation = 0,
            Level = level,
            Experience = 32000,
            EquippedItems =
            {
                new EquippedItem { Slot = ItemSlot.Head },
                new EquippedItem { Slot = ItemSlot.Hand },
            },
            User = new User
            {
                HeirloomPoints = 0,
                ExperienceMultiplier = experienceMultiplier,
            },
        };
        ArrangeDb.Add(character);
        await ArrangeDb.SaveChangesAsync();

        Mock<ICharacterService> characterServiceMock = new();

        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };

        RetireCharacterCommand.Handler handler = new(ActDb, Mapper, characterServiceMock.Object,
            activityLogServiceMock.Object, Constants);
        await handler.Handle(new RetireCharacterCommand
        {
            CharacterId = character.Id,
            UserId = character.UserId,
        }, CancellationToken.None);

        character = await AssertDb.Characters
            .Include(c => c.User)
            .Include(c => c.EquippedItems)
            .FirstAsync(c => c.Id == character.Id);
        Assert.That(character.Generation, Is.EqualTo(1));
        Assert.That(character.Level, Is.EqualTo(Constants.MinimumLevel));
        Assert.That(character.Experience, Is.EqualTo(0));
        Assert.That(character.User!.HeirloomPoints, Is.EqualTo(expectedPoints));
        Assert.That(character.User.ExperienceMultiplier, Is.EqualTo(expectedExperienceMultiplier).Within(0.001f));
        Assert.That(character.EquippedItems, Is.Empty);

        characterServiceMock.Verify(cs => cs.ResetCharacterCharacteristics(It.IsAny<Character>(), false));
    }

    [Test]
    public async Task NotFoundIfUserDoesntExist()
    {
        var characterService = Mock.Of<ICharacterService>();
        var activityLogService = Mock.Of<IActivityLogService>();
        RetireCharacterCommand.Handler handler = new(ActDb, Mapper, characterService, activityLogService, Constants);
        var result = await handler.Handle(
            new RetireCharacterCommand
            {
                CharacterId = 1,
                UserId = 2,
            }, CancellationToken.None);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.CharacterNotFound));
    }

    [Test]
    public async Task NotFoundIfCharacterDoesntExist()
    {
        var user = ArrangeDb.Users.Add(new User());
        await ArrangeDb.SaveChangesAsync();

        var characterService = Mock.Of<ICharacterService>();
        var activityLogService = Mock.Of<IActivityLogService>();
        RetireCharacterCommand.Handler handler = new(ActDb, Mapper, characterService, activityLogService, Constants);
        var result = await handler.Handle(
            new RetireCharacterCommand
            {
                CharacterId = 1,
                UserId = user.Entity.Id,
            }, CancellationToken.None);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.CharacterNotFound));
    }

    [Test]
    public async Task BadRequestIfLevelTooLow()
    {
        Character character = new()
        {
            Level = 30,
            User = new User(),
        };
        ArrangeDb.Characters.Add(character);
        await ArrangeDb.SaveChangesAsync();

        var characterService = Mock.Of<ICharacterService>();
        var activityLogService = Mock.Of<IActivityLogService>();
        RetireCharacterCommand.Handler handler = new(ActDb, Mapper, characterService, activityLogService, Constants);
        var result = await handler.Handle(
            new RetireCharacterCommand
            {
                CharacterId = character.Id,
                UserId = character.UserId,
            }, CancellationToken.None);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.CharacterLevelRequirementNotMet));
    }
}
