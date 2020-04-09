using System;
using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;

namespace Crpg.Application.Bans.Models
{
    public class BanViewModel : IMapFrom<Ban>
    {
        public int Id { get; set; }
        public int BannedUserId { get; set; }
        public DateTimeOffset Until { get; set; }
        public string Reason { get; set; } = string.Empty;
        public int BannedByUserId { get; set; }
    }
}