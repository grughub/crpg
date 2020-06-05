﻿using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;

namespace Crpg.Application.Items.Models
{
    public class ItemHorseComponentViewModel : IMapFrom<ItemHorseComponent>
    {
        public int BodyLength { get; set; }
        public int ChargeDamage { get; set; }
        public int Maneuver { get; set; }
        public int Speed { get; set; }
        public int HitPoints { get; set; }
    }
}