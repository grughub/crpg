using AutoMapper;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Characters.Commands;

public record RetireCharacterCommand : IMediatorRequest<CharacterViewModel>
{
    public int CharacterId { get; init; }
    public int UserId { get; init; }

    internal class Handler : IMediatorRequestHandler<RetireCharacterCommand, CharacterViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<RetireCharacterCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;
        private readonly ICharacterService _characterService;
        private readonly IActivityLogService _activityLogService;
        private readonly Constants _constants;

        public Handler(ICrpgDbContext db, IMapper mapper, ICharacterService characterService,
            IActivityLogService activityLogService, Constants constants)
        {
            _db = db;
            _mapper = mapper;
            _characterService = characterService;
            _activityLogService = activityLogService;
            _constants = constants;
        }

        public async Task<Result<CharacterViewModel>> Handle(RetireCharacterCommand req, CancellationToken cancellationToken)
        {
            var character = await _db.Characters
                .Include(c => c.User)
                .Include(c => c.EquippedItems)
                .FirstOrDefaultAsync(c => c.Id == req.CharacterId && c.UserId == req.UserId, cancellationToken);
            if (character == null)
            {
                return new(CommonErrors.CharacterNotFound(req.CharacterId, req.UserId));
            }

            if (character.Level < _constants.MinimumRetirementLevel)
            {
                return new(CommonErrors.CharacterLevelRequirementNotMet(_constants.MinimumRetirementLevel, character.Level));
            }

            _db.ActivityLogs.Add(_activityLogService.CreateCharacterRetiredLog(character.UserId, character.Id, character.Level));

            character.User!.HeirloomPoints += character.Level switch
            {
                < 33 => 1,
                < 35 => 2,
                _ => 3,
            };
            character.User.ExperienceMultiplier = Math.Min(
                character.User.ExperienceMultiplier + _constants.ExperienceMultiplierByGeneration,
                _constants.MaxExperienceMultiplierForGeneration);

            character.Generation += 1;
            character.Level = _constants.MinimumLevel;
            character.Experience = 0;
            character.EquippedItems.Clear();
            _characterService.ResetCharacterCharacteristics(character);

            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("User '{0}' retired character '{1}'", req.UserId, req.CharacterId);
            return new(_mapper.Map<CharacterViewModel>(character));
        }
    }
}
